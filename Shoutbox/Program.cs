using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;

namespace Shoutbox
{
    internal class Program
    {
        public static LeagueSharp.Common.Menu Config;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("Shoutbox loaded!");
            Config = new LeagueSharp.Common.Menu("Shoutbox", "Shoutbox", true);
            Config.AddToMainMenu();
            Config.AddItem(new LeagueSharp.Common.MenuItem("login", ".login user pw"));
            Config.AddItem(new LeagueSharp.Common.MenuItem("msg", ".sb message"));
            Config.AddItem(new LeagueSharp.Common.MenuItem("user", Shoutbox.account));
            Config.AddItem(new LeagueSharp.Common.MenuItem("pw", Shoutbox.password));

            Config.Item("user").SetValue<StringList>(new StringList(new string[] { Shoutbox.account }));
            StringList us = Config.Item("user").GetValue<StringList>();
            Config.Item("user").SetValue<StringList>(new StringList(new string[] { us.SList[0] }));

            Config.Item("pw").SetValue<StringList>(new StringList(new string[] { Shoutbox.account }));
            StringList pw = Config.Item("pw").GetValue<StringList>();
            Config.Item("pw").SetValue<StringList>(new StringList(new string[] { pw.SList[0] }));

            Shoutbox.account = us.SList[0];
            Shoutbox.password = pw.SList[0];

            if (Shoutbox.account != "")
                Shoutbox.Process("update msg");
            Game.OnGameInput += Game_OnGameInput;
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        static void Game_OnGameInput(GameInputEventArgs args)
        {
            if (args.Input.StartsWith(".sb "))
            {
                Shoutbox.Process(args.Input);
                args.Process = false;
            }
            else if (args.Input.StartsWith(".login"))
            {
                String msg = args.Input.Substring(7);
                Shoutbox.account = msg.Substring(0, msg.IndexOf(" "));
                Shoutbox.password = msg.Substring(msg.IndexOf(" ") + 1);
                Config.Item("user").SetValue<StringList>(new StringList(new string[] { Shoutbox.account }));
                Config.Item("pw").SetValue<StringList>(new StringList(new string[] { Shoutbox.password }));
                if (Shoutbox.account != "")
                    Shoutbox.Process("update msg");
                args.Process = false;
            }
        }

        static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == 0xC1)
            {
                if (Shoutbox.account != "")
                    Shoutbox.Process("update msg");
            }
        }
    }
}
