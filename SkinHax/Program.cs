using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;
using LeagueSharp;
using LeagueSharp.Common;

namespace SkinHax
{
    internal class Program
    {
        public static Menu Config;
        public static String DataDragonBase = "http://ddragon.leagueoflegends.com/";
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            new Thread(() =>
            {
                Config = new Menu("SkinHax", "SkinHax", true);
                String versionJson = new WebClient().DownloadString(DataDragonBase + "realms/na.json");
                String gameVersion = (String)((Dictionary<String, Object>)new JavaScriptSerializer().Deserialize<Dictionary<String, Object>>(versionJson)["n"])["champion"];
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    String champJson = new WebClient().DownloadString(DataDragonBase + "cdn/" + gameVersion + "/data/en_US/champion/" + hero.ChampionName + ".json");
                    ArrayList skins = (ArrayList)((Dictionary<String, Object>)((Dictionary<String, Object>)new JavaScriptSerializer().Deserialize<Dictionary<String, Object>>(champJson)["data"])[hero.ChampionName])["skins"];
                    Menu champMenu = new Menu(hero.ChampionName, hero.ChampionName, false);
                    foreach (Dictionary<string, object> skin in skins)
                    {
                        String skinName = skin["name"].ToString();
                        if (skinName.Equals("default"))
                            skinName = hero.ChampionName;
                        MenuItem changeSkin = champMenu.AddItem(new MenuItem(skinName, skinName).SetValue<bool>(false));
                        changeSkin.ValueChanged += (s, e) =>
                        {
                            if (e.GetNewValue<bool>())
                            {
                                champMenu.Items.ForEach(p => { if (p.GetValue<bool>() && p.Name != skinName) p.SetValue(false); });
                                Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(hero.NetworkId, (int)skin["num"], hero.ChampionName)).Process(PacketChannel.S2C);
                            }
                        };
                    }
                    Config.AddSubMenu(champMenu);
                }
                Config.AddToMainMenu();
            }).Start();
        }
    }
}
