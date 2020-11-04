using System;
using System.Collections.Generic;
using System.Reflection;

namespace MTGLib
{

    public class MTG
    {
        private static MTG instance;
        public static MTG Instance {
            get { return instance; }
        }

        public List<Player> players = new List<Player>();

        int currentTurn = 0;

        public Dictionary<OID, MTGObject> objects = new Dictionary<OID, MTGObject>();

        public Battlefield battlefield = new Battlefield();
        public TheStack theStack = new TheStack();
        public Exile exile = new Exile();

        public List<ContinuousEffect> continuousEffects = new List<ContinuousEffect>();

        public struct Turn
        {
            public int turnCount;
            public int playerTurnIndex;
            public int playerPriorityIndex;
            public Phase phase;

            public void Init()
            {
                turnCount = 0;
                playerTurnIndex = 0;
                playerPriorityIndex = 0;
                phase = new Phase();
            }

            // Resets priority to AP
            public void ResetPriority()
            {
                playerPriorityIndex = playerTurnIndex;
            }

            public bool ActivePlayerPriority {
                get { return playerPriorityIndex == playerTurnIndex; }
            }

            // Returns true if it rolled over
            public bool IncTurn(int playerCount)
            {
                playerTurnIndex++;
                turnCount++;
                if (playerTurnIndex >= playerCount)
                {
                    playerTurnIndex = 0;
                    return true;
                }
                return false;
            }

            // Returns true if it rolled over
            // Does NOT increment turn
            public bool IncPhase()
            {
                if (phase.type == Phase.FinalPhase)
                {
                    phase.type = Phase.StartingPhase;
                    return true;
                } else
                {
                    phase.type = phase.type.Next();
                    return false;
                }
            }

            // Returns true if it rolled over
            public bool IncPriority(int playerCount)
            {
                playerPriorityIndex++;
                if (playerPriorityIndex >= playerCount)
                {
                    playerPriorityIndex = 0;
                    return true;
                }
                return false;
            }
        }
        public Turn turn = new Turn();

        int noActionPassCount = 0;
        bool haveAllPlayersPassed { get { return noActionPassCount >= players.Count; } }

        public MTG(params List<MTGObject.BaseCardAttributes>[] libraries)
        {
            instance = this;

            int playerIndex = 0;
            foreach (var library in libraries)
            {
                Player player = new Player();
                foreach (var i in library)
                {
                    var cardattr = i;
                    cardattr.owner = playerIndex;
                    MTGObject mtgObject = new MTGObject(cardattr);
                    OID oid = new OID();
                    objects.Add(oid, mtgObject);
                    player.library.Add(oid);
                }
                
                players.Add(player);
                playerIndex++;
            }
        }

        public void GameLoop()
        {
            // Main loop
            while (true)
            {
                // Beginning of step
                CalculateBoardState();
                // Stuff that triggers at start of step
                Console.WriteLine("Start " + turn.phase.type.GetString());
                Console.WriteLine("Active player - " + turn.playerTurnIndex);
                Console.WriteLine();
                theStack.PPrint();
                Console.WriteLine();
                turn.phase.StartCurrentPhase();
                CalculateBoardState();

                if (turn.phase.GivesPriority || theStack.Count > 0)
                {
                    // Loop until priority has finished
                    while (true)
                    {
                        // Active player gets priority
                        turn.ResetPriority();
                        int passCount = 0;
                        // Loop until all players have passed in a row
                        while (true)
                        {
                            // Loop until current player has passed
                            while (true)
                            {
                                Console.WriteLine("Priority for player " + turn.playerPriorityIndex);
                                CalculateBoardState();
                                SBALoop();
                                CalculateBoardState();
                                bool passed = ResolveCurrentPriority();
                                if (passed) break;
                                else
                                {
                                    passCount = 0;
                                }
                            }
                            passCount++;

                            if (passCount < players.Count)
                            {
                                turn.IncPriority(players.Count);
                            }
                            else break;
                        }
                        if (theStack.Count > 0)
                        {
                            CalculateBoardState();
                            theStack.PopAndResolve();
                        }
                        else break;
                    }
                }
                // End of step
                Console.WriteLine();
                CalculateBoardState();
                turn.phase.EndCurrentPhase();
                turn.IncPhase();
            }
        }

        // Returns true if the player passed
        public bool ResolveCurrentPriority()
        {
            PriorityChoice choice = new PriorityChoice();
            choice.ConsoleResolve();
            switch (choice.FirstChoice.type)
            {
                case PriorityOption.OptionType.PassPriority:
                    return true;
                case PriorityOption.OptionType.CastSpell:
                    MoveZone(choice.FirstChoice.source, theStack);
                    break;
                case PriorityOption.OptionType.PlayLand:
                    MoveZone(choice.FirstChoice.source, battlefield);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return false;
        }

        public void PushChoice(Choice choice)
        {
            if (choice.Resolved)
                throw new ArgumentException("This choice is already resolved.");

            choice.ConsoleResolve();
        }

        private void UpdateAllModifications()
        {
            List<Modification> allMods = new List<Modification>();
            foreach (var effect in continuousEffects)
            {
                allMods.AddRange(effect.GetModifications());
            }
            foreach (var oid in battlefield)
            {
                var obj = objects[oid];
                foreach (StaticAbility ability in obj.attr.staticAbilities)
                {
                    allMods.AddRange(ability.GetModifications());
                }
            }
            _allModifications = allMods;
        }

        public void CalculateBoardState()
        {
            UpdateAllModifications();
            foreach (var x in objects)
            {
                OID oid = x.Key;
                MTGObject obj = x.Value;
                obj.CalculateAttributes();
            }
        }

        public void MoveZone(OID oid, BaseZone newZone)
        {
            BaseZone oldZone = FindZoneFromOID(oid);
            MoveZone(oid, oldZone, newZone);
        }

        public void MoveZone(OID oid, BaseZone oldZone, BaseZone newZone)
        {
            if (oldZone is TheStack)
                (oldZone as TheStack).Remove(oid);
            else if (oldZone is Zone)
                (oldZone as Zone).Remove(oid);

            if (newZone is TheStack)
                (newZone as TheStack).Push(oid);
            else if (newZone is Zone)
                (newZone as Zone).Push(oid);
        }

        public BaseZone FindZoneFromOID(OID oid)
        {
            foreach (BaseZone zone in GetAllZones())
            {
                if (zone is Zone castZone)
                    if (castZone.Has(oid))
                        return zone;
                if (zone is TheStack castStack)
                    if (castStack.Has(oid))
                        return zone;
            }
            return null;
        }

        public IReadOnlyList<BaseZone> GetAllZones()
        {
            List<BaseZone> zones = new List<BaseZone>
            {
                battlefield,
                theStack,
                exile
            };
            foreach (Player player in players)
            {
                zones.Add(player.graveyard);
                zones.Add(player.hand);
                zones.Add(player.library);
            }
            
            return zones.AsReadOnly();
        }
        public IReadOnlyList<BaseZone> GetRevealedZones()
        {
            List<BaseZone> zones = new List<BaseZone>
            {
                battlefield,
                theStack,
                exile
            };
            foreach (Player player in players)
            {
                zones.Add(player.graveyard);
            }
            return zones.AsReadOnly();
        }

        private List<Modification> _allModifications = new List<Modification>();

        public IReadOnlyList<Modification> AllModifications
        {
            get {
                return _allModifications.AsReadOnly();
            }
        }

        // Get a list of players in APNAP order
        public IReadOnlyList<Player> APNAP { get
            {
                var ret = new List<Player>();
                for (int i = 0; i<players.Count; i++)
                {
                    int index = (i + turn.playerTurnIndex) % players.Count;
                    ret.Add(players[index]);
                }
                return ret.AsReadOnly();
            }
        }

        public void Start()
        {
            turn.Init();
            foreach (var player in players)
            {
                player.library.Shuffle();
                player.Draw(7);
            }
        }

        public void SBALoop()
        {
            bool actionsToDo = true;
            while (actionsToDo)
            {
                actionsToDo = StateBasedActions() > 0;
            }
        }

        public int StateBasedActions()
        {

            //   *704.5a If a player has 0 or less life, that player loses the game.

            //  * 704.5b If a player attempted to draw a card from a library with no cards in it since the last time state   -based actions were checked, that player loses the game.

            //  * 704.5c If a player has ten or more poison counters, that player loses the game. Ignore this rule in Two - Headed Giant games; see rule 704.6b instead.
            //    *704.5d If a token is in a zone other than the battlefield, it ceases to exist.

            //   * 704.5e If a copy of a spell is in a zone other than the stack, it ceases to exist.If a copy of a card is in any zone other than the stack or the battlefield, it ceases to exist.
            //    *704.5f If a creature has toughness 0 or less, it’s put into its owner’s graveyard. Regeneration can’t replace this event.
            //    * 704.5g If a creature has toughness greater than 0, it has damage marked on it, and the total damage marked on it is greater than or equal to its toughness, that creature has been dealt lethal damage and is destroyed. Regeneration can replace this event.
            //    * 704.5h If a creature has toughness greater than 0, and it’s been dealt damage by a source with deathtouch since the last time state-based actions were checked, that creature is destroyed. Regeneration can replace this event.
            //    * 704.5i If a planeswalker has loyalty 0, it’s put into its owner’s graveyard.
            //    * 704.5j If a player controls two or more legendary permanents with the same name, that player chooses one of them, and the rest are put into their owners’ graveyards. This is called the “legend rule.” 
            //    * 704.5k If two or more permanents have the supertype world, all except the one that has had the world supertype for the shortest amount of time are put into their owners’ graveyards. In the event of a tie for the shortest amount of time, all are put into their owners’ graveyards.This is called the “world rule.” 704.5m If an Aura is attached to an illegal object or player, or is not attached to an object or player, that Aura is put into its owner’s graveyard.
            //    * 704.5n If an Equipment or Fortification is attached to an illegal permanent or to a player, it becomes unattached from that permanent or player. It remains on the battlefield.

            //* 704.5p If a creature is attached to an object or player, it becomes unattached and remains on the battlefield. Similarly, if a permanent that’s neither an Aura, an Equipment, nor a Fortification is attached to an object or player, it becomes unattached and remains on the battlefield.
            //    * 704.5q If a permanent has both a +1/+1 counter and a -1/-1 counter on it, N +1/+1 and N -1/-1 counters are removed from it, where N is the smaller of the number of +1/+1 and -1/-1 counters on it.

            //* 704.5r If a permanent with an ability that says it can’t have more than N counters of a certain kind on it has more than N counters of that kind on it, all but N of those counters are removed from it. 704.5s If the number of lore counters on a Saga permanent is greater than or equal to its final chapter number and it isn’t the source of a chapter ability that has triggered but not yet left the stack, that Saga’s controller sacrifices it. See rule 714, “Saga Cards.”

            return 0;
        }
    }
}