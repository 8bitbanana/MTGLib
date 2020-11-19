using System;
using System.Collections.Generic;
using System.Threading;
using EmbedIO;
using EmbedIO.WebApi;
using Swan.Logging;
using EmbedIO.Actions;
using EmbedIO.Routing;

using MTGLib;

namespace MTGTestApp
{
    class BoardViewerController<T> : WebApiController
    {
        private static T data;

        public static void Update(T data)
        {
            BoardViewerController<T>.data = data;
        }


        [Route(HttpVerbs.Get, "/")]
        public T DataRoute()
        {
            return data;
        }
    }

    class BoardViewer
    {

        public EventWaitHandle KillServer = new EventWaitHandle(false, EventResetMode.AutoReset);

        public struct BoardInfo
        {
            public Dictionary<string, ObjectInfo> objects;

            public List<PlayerInfo> players;

            public List<string> battlefield;
            public List<string> theStack;
            public List<string> exile;

            public void Import(MTG mtg)
            {
                objects = new Dictionary<string, ObjectInfo>();
                foreach (var kvp in mtg.objects)
                {
                    var obj = new ObjectInfo();
                    obj.Import(kvp.Value);
                    objects.Add(kvp.Key.ToString(), obj);
                }

                players = new List<PlayerInfo>();
                foreach (Player player in mtg.players)
                {
                    var info = new PlayerInfo();
                    info.Import(player);
                    players.Add(info);
                }

                battlefield = GetZoneInfo(mtg.battlefield);
                theStack = GetZoneInfo(mtg.theStack);
                exile = GetZoneInfo(mtg.exile);
            }
        }

        public struct PlayerInfo
        {
            public List<string> library;
            public List<string> hand;
            public List<string> graveyard;

            public int life;

            public void Import(Player player)
            {
                library = GetZoneInfo(player.library);
                hand = GetZoneInfo(player.hand);
                graveyard = GetZoneInfo(player.graveyard);
                life = player.life;
            }
        }

        public static List<string> GetZoneInfo(Zone zone)
        {
            var l = new List<string>();
            foreach (OID oid in zone)
            {
                l.Add(oid.ToString());
            }
            return l;
        }

        public struct ObjectInfo
        {
            public string originalName;
            public string name;
            public string typeLine;
            public string powerToughness;
            public int owner;
            public int controller;

            public MTGObject.PermanentStatus permanentStatus;

            public void Import(MTGObject obj)
            {
                originalName = obj.baseattr.name;
                name = obj.attr.name;

                
                if (obj is AbilityObject cast)
                {
                    switch (cast.abilityType)
                    {
                        case (AbilityObject.AbilityType.Activated):
                            typeLine = "Activated Ability";
                            break;
                        case (AbilityObject.AbilityType.Triggered):
                            typeLine = "Triggered Ability";
                            break;
                        default:
                            typeLine = "Ability";
                            break;
                    }
                } else
                {
                    typeLine = "";
                    foreach (var t in obj.attr.superTypes)
                        typeLine += t.GetString() + " ";
                    foreach (var t in obj.attr.cardTypes)
                        typeLine += t.GetString() + " ";
                    if (obj.attr.subTypes.Count > 0)
                        typeLine += "- ";
                    foreach (var t in obj.attr.subTypes)
                        typeLine += t.GetString() + " ";
                    typeLine = typeLine.Trim();
                }
                

                if (obj.attr.cardTypes.Contains(MTGObject.CardType.Creature))
                    powerToughness = $"{obj.attr.power}/{obj.attr.toughness}";
                else powerToughness = null;

                owner = obj.owner;
                controller = obj.attr.controller;
                permanentStatus = obj.permanentStatus;
            }
        }

        public BoardViewer() { }

        public void Update(MTG mtg)
        {
            BoardInfo info = new BoardInfo();
            info.Import(mtg);

            BoardViewerController<BoardInfo>.Update(info);
        }

        public void Run()
        {
            string url = "http://localhost:9696";

            using (var server = CreateWebServer(url))
            {
                server.RunAsync();
                KillServer.WaitOne();
            }
        }

        private static WebServer CreateWebServer(string url)
        {
            Logger.UnregisterLogger<ConsoleLogger>();
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                .WithModule(new WebApiModule("/data", ResponseSerializer.Json)
                    .WithController<BoardViewerController<BoardInfo>>())
                .WithStaticFolder("/", "./web", true);

            server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();
            return server;
        }
    }
}
