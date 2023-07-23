using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Mail;
using System.Reflection;

namespace GaRyan2.Utilities
{
    public static partial class Logger
    {
        private static void SendNotification()
        {
            EpgNotifier emailConfig = Helper.ReadJsonFile(Helper.EmailNotifier, typeof(EpgNotifier));
            if (string.IsNullOrEmpty(emailConfig?.SmtpServer) ||
               (Status == 0x0000 && (emailConfig.NotifyOn & 0x01) == 0) ||
               (Status == 0x0001 && (emailConfig.NotifyOn & 0x02) == 0) ||
               (Status == 0xBAD1 && (emailConfig.NotifyOn & 0x04) == 0) ||
               (Status == 0xDEAD && (emailConfig.NotifyOn & 0x08) == 0)) return;

            var application = Assembly.GetEntryAssembly().GetName().Name.ToUpper();
            var sessionStatus = Status == 0 ? "[SUCCESS]" : Status == 1 ? "[UPDATE AVAILABLE]" : Status == 0xBAD1 ? "[WARNING]" : "[ERROR]";
            SmtpClient smtpClient = new SmtpClient
            {
                Port = emailConfig.SmtpPort,
                Host = emailConfig.SmtpServer,
                EnableSsl = emailConfig.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(emailConfig.Username, emailConfig.Password)
            };

            MailMessage message = new MailMessage
            {
                From = new MailAddress(emailConfig.SendFrom, Dns.GetHostName()),
                Subject = $"{sessionStatus} {application} on {Dns.GetHostName()}",
                IsBodyHtml = false,
                Body = $"There was a(n) {sessionStatus} during last update on station {Dns.GetHostName()}. Below is the relevant log session.\n\n{_sessionString}",
            };
            message.To.Add(emailConfig.SendTo);

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                WriteError($"Failed to send email notification upon {sessionStatus}.\n{ex}");
            }
        }

        public static bool SendTestMessage(EpgNotifier emailConfig)
        {
            var application = Assembly.GetEntryAssembly().GetName().Name.ToUpper();
            SmtpClient smtpClient = new SmtpClient
            {
                Port = emailConfig.SmtpPort,
                Host = emailConfig.SmtpServer,
                EnableSsl = emailConfig.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(emailConfig.Username, emailConfig.Password)
            };

            MailMessage message = new MailMessage
            {
                From = new MailAddress(emailConfig.SendFrom, Dns.GetHostName()),
                Subject = $"[TEST] {application} on {Dns.GetHostName()}",
                IsBodyHtml = false,
                Body = $"This is a test message to verify proper email configuration on {Dns.GetHostName()}.",
            };
            message.To.Add(emailConfig.SendTo);

            try
            {
                smtpClient.Send(message);
                return true;
            }
            catch { }
            return false;
        }
    }

    public class EpgNotifier
    {
        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }

        [JsonProperty("SmtpServer")]
        public string SmtpServer { get; set; }

        [JsonProperty("SmtpPort")]
        public int SmtpPort { get; set; } = 587;

        [JsonProperty("EnableSsl")]
        public bool EnableSsl { get; set; } = true;

        [JsonProperty("SendFrom")]
        public string SendFrom { get; set; }

        [JsonProperty("SendTo")]
        public string SendTo { get; set; }

        [JsonProperty("NotifyOn")]
        public int NotifyOn { get; set; } = 0x0E;

        [JsonProperty("StorageWarningGB")]
        public int StorageWarningGB { get; set; } = -1;

        [JsonProperty("StorageErrorGB")]
        public int StorageErrorGB { get; set; } = 0;

        [JsonProperty("ConflictWarningDays")]
        public int ConflictWarningDays { get; set; } = 3;

        [JsonProperty("ConflictErrorDays")]
        public int ConflictErrorDays { get; set; } = 1;
    }
}
