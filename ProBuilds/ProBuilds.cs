using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ProBuilds
{
    public class ProBuilds
    {
        public List<Build> Builds { get; set; }
        public class Build
        {
            public String Name { get; set; }
            public String Popularity { get; set; }
            public List<Item> Summary { get; set; }
            public List<Item> StartingItems { get; set; }
            public List<Item> Order { get; set; }
            public List<Item> BestItems { get; set; }
        }
        public class Item
        {
            public String name { get; set; }
            public String id { get; set; }
        }

        public ProBuilds(String champ)
        {
            Builds = new List<Build>();
            String htmlPage = new WebClient().DownloadString("http://lolbuilder.net/" + champ);
            foreach (Match buildsRegex in new Regex("#(build\\-[0-9]+)\">([a-zA-Z ]+) \\(<span class=\"hover-text\" tooltip=\"Seen in up to ([0-9]+)").Matches(htmlPage))
            {
                Build build = new Build();

                Match buildRegex = new Regex(buildsRegex.Groups[1].ToString()).Matches(htmlPage)[1];
                String buildSectionHtml = htmlPage.Substring(buildRegex.Index);

                String summaryHtml = buildSectionHtml.Substring(buildSectionHtml.IndexOf("build-summary-separator separator"));
                summaryHtml = summaryHtml.Substring(0, summaryHtml.IndexOf("build-box starting-items"));
                String startingHtml = buildSectionHtml.Substring(buildSectionHtml.IndexOf("build-box starting-items"));
                startingHtml = startingHtml.Substring(0, startingHtml.IndexOf("build-box early-game"));
                String orderHtml = buildSectionHtml.Substring(buildSectionHtml.IndexOf("build-box early-game"));
                orderHtml = orderHtml.Substring(0, orderHtml.IndexOf("build-box final-items"));
                String finalHtml = buildSectionHtml.Substring(buildSectionHtml.IndexOf("build-box final-items"));
                if (finalHtml.IndexOf("build-app-text") > 0)
                    finalHtml = finalHtml.Substring(0, finalHtml.IndexOf("build-app-text"));

                build.Name = buildsRegex.Groups[2].ToString();
                build.Popularity = buildsRegex.Groups[3].ToString();
                build.Summary = GetItemsFromHtml(summaryHtml);
                build.StartingItems = GetItemsFromHtml(startingHtml);
                build.Order = GetItemsFromHtml(orderHtml);
                build.BestItems = GetItemsFromHtml(finalHtml);

                Builds.Add(build);
            }
        }

        public List<Item> GetItemsFromHtml(String html)
        {
            List<Item> itemList = new List<Item>();
            foreach (Match itemRegex in new Regex("item-wrapper").Matches(html))
            {
                String itemHtml = html.Substring(itemRegex.Index);
                Match itemMatch = new Regex("(\\d)\\.png'\\) -(\\d+)px -(\\d+)px").Matches(itemHtml)[0];
                Item item = new Item();
                item.id = DataDragon.ReverseItemLookup(Convert.ToInt32(itemMatch.Groups[1].Value), Convert.ToInt32(itemMatch.Groups[2].Value), Convert.ToInt32(itemMatch.Groups[3].Value));
                item.name = DataDragon.ItemName(item.id);
                itemList.Add(item);
            }
            return itemList;
        }
    }
}