using System;
using System.Collections.Generic;
using SysDrawing = System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using LeagueSharp;

namespace Shoutbox
{
    public class Message
    {
        public String Author;
        public String Msg;
        public String Time;
    }
    public static class Shoutbox
    {
        public static String forumUrl = "http://www.joduska.me/forum/";
        public static WebBrowser wb = new WebBrowser();
        public static List<Message> messages = new List<Message>();
        public static String account = "";
        public static String password = "";
        public static void Process(String msg)
        {
            Shoutbox.Run(Shoutbox.DoWorkMsgAsync, new object[] { account, password, msg });
        }
        public static async Task<object> DoWorkMsgAsync(object[] args)
        {
            using (wb = new WebBrowser())
            {
                wb.ScriptErrorsSuppressed = true;
                TaskCompletionSource<bool> tcs = null;
                WebBrowserDocumentCompletedEventHandler documentCompletedHandler = (s, e) =>
                {
                    if (e.Url.ToString() == forumUrl)
                    {
                        if (wb.Document.GetElementById("sign_in") != null)
                        {
                            wb.Navigate(forumUrl + "index.php?app=core&module=global&section=login");
                        }
                        else
                        {
                            if (args[2].ToString().StartsWith(".sb "))
                            {
                                wb.Document.GetElementById("shoutbox-global-shout").SetAttribute("value", args[2].ToString().Substring(4));
                                wb.Document.GetElementById("shoutbox-submit-button").InvokeMember("click");
                            }
                            else if (args[2].ToString().Equals("update msg"))
                            {
                                HtmlElement tb = wb.Document.GetElementById("shoutbox-shouts-table");
                                HtmlElementCollection shouts = tb.Children[0].Children;
                                for (int i = shouts.Count - 1; i >= 0; i--)
                                {
                                    HtmlElement el = shouts[i];
                                    if (messages.Count(msg => msg.Msg == el.Children[2].Children[1].InnerText) == 0)
                                    {
                                        Message msg = new Message()
                                        {
                                            Author = el.Children[0].Children[1].InnerText,
                                            Msg = el.Children[2].Children[1].InnerText,
                                            Time = el.Children[2].Children[0].InnerText
                                        };
                                        messages.Add(msg);
                                        int count = messages.Count;
                                        Game.PrintChat(msg.Author + " " + msg.Time + ": " + msg.Msg);
                                    }
                                }
                            }
                        }
                    }
                    else if (e.Url.ToString() == forumUrl + "index.php?app=core&module=global&section=login")
                    {
                        ((HtmlElement)wb.Document.All.GetElementsByName("ips_username")[0]).SetAttribute("value", args[0].ToString());
                        ((HtmlElement)wb.Document.All.GetElementsByName("ips_password")[0]).SetAttribute("value", args[1].ToString());
                        foreach (HtmlElement el in wb.Document.GetElementsByTagName("input"))
                        {
                            if (el.GetAttribute("value").Equals("Sign In") && el.GetAttribute("tabindex").Equals("5"))
                            {
                                el.InvokeMember("click");
                            }
                        }
                    }
                    tcs.TrySetResult(true);
                };
                tcs = new TaskCompletionSource<bool>();
                wb.DocumentCompleted += documentCompletedHandler;
                try
                {
                    wb.Navigate(forumUrl);
                    await tcs.Task;
                }
                finally
                {
                    wb.DocumentCompleted -= documentCompletedHandler;
                }
            }
            return null;
        }
        public static async Task<object> Run(Func<object[], Task<object>> worker, params object[] args)
        {
            var tcs = new TaskCompletionSource<object>();
            var thread = new Thread(() =>
            {
                EventHandler idleHandler = null;
                idleHandler = async (s, e) =>
                {
                    Application.Idle -= idleHandler;
                    await Task.Yield();
                    try
                    {
                        var result = await worker(args);
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                    Application.ExitThread();
                };
                Application.Idle += idleHandler;
                Application.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            try
            {
                return await tcs.Task;
            }
            finally
            {
                thread.Join();
            }
        }
    }
}