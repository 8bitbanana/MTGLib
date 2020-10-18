using System;
using System.Collections.Generic;

namespace MTGLib
{
    public class MTG
    {
        private static readonly Lazy<MTG>
            lazy = new Lazy<MTG>(() => new MTG());
        public static MTG Instance { get { return lazy.Value; } }

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
            foreach (var library in libraries)
            {
                int playerIndex = 0;
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

        private List<Modification> _allModifications = new List<Modification>();

        public IReadOnlyList<Modification> AllModifications
        {
            get {
                return _allModifications.AsReadOnly();
            }
        }

        private void UpdateAllModifications()
        {
            List<Modification> allMods = new List<Modification>();
            foreach (var effect in continuousEffects)
            {
                allMods.AddRange(effect.GetModifications());
            }
            _allModifications = allMods;
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

        public void PassPriority(bool actionsTaken)
        {
            if (!actionsTaken)
            {
                noActionPassCount++;
            } else
            {
                noActionPassCount = 0;
            }

            if (!haveAllPlayersPassed)
            {
                // Not all players have passed, so just pass priority
                turn.IncPriority(players.Count);
                return;
            }
            // All players have passed, continue

            if (theStack.stack.Count > 0)
            {
                // There are still items on the stack, so resolve and reset
                theStack.Resolve();
                turn.ResetPriority();
                noActionPassCount = 0;
                return;
            }
            // The stack is empty, so a phase has ended

            // Increment phase and increment turn if needed
            bool turnEnded = turn.IncPhase();
            if (turnEnded)
            {
                turn.IncTurn(players.Count);
            }
            // The new phase has started
            turn.phase.StartCurrentPhase();
        }

        

        public void StateBasedActions()
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


        }
    }
}