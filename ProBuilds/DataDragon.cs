using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Web.Script.Serialization;

namespace ProBuilds
{
    static class DataDragon
    {
        const String DataDragonHtml = @"http://ddragon.leagueoflegends.com/";
        static String lastestVersion;
        public static String LatestVersion
        {
            get
            {
                if (String.IsNullOrEmpty(lastestVersion))
                    lastestVersion = (String)new JavaScriptSerializer().Deserialize<Dictionary<Object, Object>>(new WebClient().DownloadString(DataDragonHtml + "realms/na.json"))["v"];
                return lastestVersion;
            }
        }
        static String champJson;
        public static String ChampJson
        {
            get
            {
                if (String.IsNullOrEmpty(champJson))
                    champJson = new WebClient().DownloadString(DataDragonHtml + "cdn/" + LatestVersion + "/data/en_US/champion.json");
                return champJson;
            }
        }
        static String itemJson;
        public static String ItemJson
        {
            get
            {
                if (String.IsNullOrEmpty(itemJson))
                    itemJson = new WebClient().DownloadString(DataDragonHtml + "cdn/" + LatestVersion + "/data/en_US/item.json");
                return itemJson;
            }
        }
        public static Image GetImageFromUrl(String url)
        {
            using (var webClient = new WebClient())
            {
                using (var stream = new MemoryStream(webClient.DownloadData(url)))
                {
                    return Image.FromStream(stream);
                }
            }
        }
        public static Image[] champImages;
        public static Image ChampImages(int i)
        {
            if (champImages == null)
            {
                champImages = new Image[5]{
                    GetImageFromUrl(DataDragonHtml + "cdn/" + LatestVersion + "/img/sprite/champion0.png"),
                    GetImageFromUrl(DataDragonHtml + "cdn/" + LatestVersion + "/img/sprite/champion1.png"),
                    GetImageFromUrl(DataDragonHtml + "cdn/" + LatestVersion + "/img/sprite/champion2.png"),
                    GetImageFromUrl(DataDragonHtml + "cdn/" + LatestVersion + "/img/sprite/champion3.png"),
                    GetImageFromUrl(DataDragonHtml + "cdn/" + LatestVersion + "/img/sprite/champion4.png")
                };
            }
            return champImages[i];
        }
        public static Image[] itemImages;
        public static Image ItemImages(int i)
        {
            if (itemImages == null)
            {
                itemImages = new Image[3]{
                    GetImageFromUrl(DataDragonHtml + "cdn/" + LatestVersion + "/img/sprite/item0.png"),
                    GetImageFromUrl(DataDragonHtml + "cdn/" + LatestVersion + "/img/sprite/item1.png"),
                    GetImageFromUrl(DataDragonHtml + "cdn/" + LatestVersion + "/img/sprite/item2.png"),
                };
            }
            return itemImages[i];
        }
        static Image cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }
        public static Image GetChampImage(String champ)
        {
            Dictionary<String, Object> imageInfo = (Dictionary<String, Object>)
                ((Dictionary<String, Object>)(
                (Dictionary<String, Object>)new JavaScriptSerializer().Deserialize<Dictionary<Object, Object>>(ChampJson)["data"])
                [champ])
                ["image"];
            String spriteName = (String)imageInfo["sprite"];
            return cropImage(ChampImages(Int32.Parse(Regex.Match(spriteName, @"\d+").Value)),
                new Rectangle((int)imageInfo["x"], (int)imageInfo["y"], (int)imageInfo["w"], (int)imageInfo["h"]));
        }
        public static Dictionary<String, Image> ItemImageCache = new Dictionary<String, Image>();
        public static Image GetItemImage(String itemId)
        {
            if (ItemImageCache.ContainsKey(itemId))
                return ItemImageCache[itemId];
            Dictionary<String, Object> imageInfo = (Dictionary<String, Object>)
                ((Dictionary<String, Object>)(
                (Dictionary<String, Object>)new JavaScriptSerializer().Deserialize<Dictionary<Object, Object>>(ItemJson)["data"])
                [itemId])
                ["image"];
            String spriteName = (String)imageInfo["sprite"];
            Image itemImage = cropImage(ItemImages(Int32.Parse(Regex.Match(spriteName, @"\d+").Value)),
                new Rectangle((int)imageInfo["x"], (int)imageInfo["y"], (int)imageInfo["w"], (int)imageInfo["h"]));
            ItemImageCache[itemId] = itemImage;
            return itemImage;
        }
        public static Dictionary<String, String> ReverseItemLookupCache = new Dictionary<String, String>();
        public static String ReverseItemLookup(int page, int x, int y)
        {
            if (ReverseItemLookupCache.Count == 0)
            {
                Dictionary<String, Object> items = (Dictionary<String, Object>)new JavaScriptSerializer().Deserialize<Dictionary<Object, Object>>(ItemJson)["data"];
                foreach (KeyValuePair<String, Object> item in items)
                {
                    Dictionary<String, Object> imageInfo = (Dictionary<String, Object>)((Dictionary<String, Object>)item.Value)["image"];
                    String spriteName = ((String)imageInfo["sprite"]).Substring(4, 1);
                    ReverseItemLookupCache.Add(Int32.Parse(spriteName) + ":" + (int)imageInfo["x"] + ":" + (int)imageInfo["y"], item.Key);
                }
            }
            return ReverseItemLookupCache[page + ":" + x + ":" + y];
        }
        public static String ItemName(String itemId)
        {
            return (String)((Dictionary<String, Object>)(
                (Dictionary<String, Object>)new JavaScriptSerializer().Deserialize<Dictionary<Object, Object>>(ItemJson)["data"])
                [itemId])
                ["name"];
        }
        static DataDragon()
        {
            ChampImages(0);
            ItemImages(0);
        }
    }
}
