using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using LeagueSharp;
using LeagueSharp.Common;

namespace AssemblySelector
{
    internal class Program
    {
        public static Menu Config;
        public static Dictionary<String, TogglePattern> assemblyList = new Dictionary<String, TogglePattern>();
        static void Main(String[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("AssemblySelector loaded!");
            Config = new Menu("ExeSelect", "AssemblySelector", true);
            Config.AddToMainMenu();
            new Thread(() =>
            {
                AutomationElement desktop = AutomationElement.RootElement;
                AutomationElement leaguesharp = desktop.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "LeagueSharp"));
                AutomationElement assemblyTab = leaguesharp.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "TabControl"));
                AutomationElement assemblyTabItem = assemblyTab.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Assemblies"));
                AutomationElement assemblyGrid = assemblyTabItem.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "DataGrid"));
                Boolean allAssemblies = false;
                int i = 0;
                Game.PrintChat(leaguesharp.Current.Name);
                while (!allAssemblies)
                {
                    AutomationElement assemblyName = ((GridPattern)assemblyGrid.GetCurrentPattern(GridPattern.Pattern)).GetItem(i, 1);
                    if (assemblyName == null)
                        break;
                    AutomationElement assemblyType = ((GridPattern)assemblyGrid.GetCurrentPattern(GridPattern.Pattern)).GetItem(i, 2);
                    if (assemblyType.Current.Name.Contains("Executable")){
                        AutomationElement checkBoxElement = ((GridPattern)assemblyGrid.GetCurrentPattern(GridPattern.Pattern)).GetItem(i, 0).FindFirst(TreeScope.Children, Condition.TrueCondition);
                        TogglePattern checkBox = (TogglePattern)checkBoxElement.GetCurrentPattern(TogglePattern.Pattern);
                        Boolean init = false;
                        if (checkBox.Current.ToggleState == ToggleState.On)
                            init = true;
                        MenuItem assemblyItem = Config.AddItem(new MenuItem(assemblyName.Current.Name, assemblyName.Current.Name).SetValue<bool>(init));
                        assemblyList.Add(assemblyName.Current.Name, checkBox);
                        assemblyItem.ValueChanged += (s, e) =>
                        {
                            try
                            {
                                Console.WriteLine(((MenuItem)s).Name);
                                new Thread(() =>
                                {
                                    Thread.Sleep(500);
                                    if (assemblyList[((MenuItem)s).Name].Current.ToggleState == ToggleState.Off && e.GetNewValue<Boolean>() ||
                                        assemblyList[((MenuItem)s).Name].Current.ToggleState == ToggleState.On && !e.GetNewValue<Boolean>())
                                        assemblyList[((MenuItem)s).Name].Toggle();
                                }).Start();
                            }
                            catch
                            {

                            }
                        };
                    }
                    i++;
                }
            }).Start();
        }
    }
}
