using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace ProBuilds
{
    internal class Program
    {
        public static Menu Config;
        public static ProBuilds probuild;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        static void Game_OnGameLoad(EventArgs args)
        {
            new System.Threading.Thread(() =>
            {
                Config = new Menu("ProBuilds", "ProBuilds", true);
                probuild = new ProBuilds(ObjectManager.Player.ChampionName);
                foreach (ProBuilds.Build build in probuild.Builds)
                {
                    Menu subbuild = new Menu(build.Name + " " + build.Popularity + "%", build.Name + " " + build.Popularity + "%");

                    Menu summary = new Menu("Summary", "Summary");
                    foreach(ProBuilds.Item item in build.Summary){
                        summary.AddItem(new MenuItem(item.name, item.name));
                    }
                    subbuild.AddSubMenu(summary);

                    Menu starting = new Menu("Starting", "Starting");
                    foreach(var item in build.StartingItems){
                        starting.AddItem(new MenuItem(item.name, item.name));
                    }
                    subbuild.AddSubMenu(starting);
                    
                    Menu order = new Menu("Order", "Order");
                    foreach(var item in build.Order){
                        order.AddItem(new MenuItem(item.name, item.name));
                    }
                    subbuild.AddSubMenu(order);

                    Menu final = new Menu("Final", "Final");
                    foreach(var item in build.BestItems){
                        final.AddItem(new MenuItem(item.name, item.name));
                    }
                    subbuild.AddSubMenu(final);

                    Config.AddSubMenu(subbuild);
                }
                Config.AddToMainMenu();
            }).Start();
        }
    }
}
