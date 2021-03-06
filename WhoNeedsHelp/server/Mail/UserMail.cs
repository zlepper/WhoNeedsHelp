﻿using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;
using SendGrid;
using WhoNeedsHelp.DB;
using WhoNeedsHelp.Models;

namespace WhoNeedsHelp.Server.Mail
{
    public class UserMail
    {
        private static string key;

        public UserMail()
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = ConfigurationManager.AppSettings["email"];
            }
        }

        public bool SendPasswordRecovery(string email)
        {
            using (HelpContext db = new HelpContext())
            {
                User user =
                    db.Users.SingleOrDefault(
                        u => u.EmailAddress.Equals(email, StringComparison.InvariantCultureIgnoreCase));
                if (user == null) return false;

                string resetKey = user.GenerateResetKey();
                db.SaveChanges();

                string resetLink = "https://" + HttpContext.Current.Request.Url.Host +
                                   (HttpContext.Current.Request.Url.Port != 430
                                       ? ":" + HttpContext.Current.Request.Url.Port
                                       : "") + "/Account/ResetPassword2?key=" + resetKey + "&email=" + HttpUtility.UrlEncode(email);
                SendGridMessage message = new SendGridMessage
                {
                    From = new MailAddress("noreply@zlepper.dk", "NoReply"),
                    Subject = "Nulstilling af kodeord",
                    Html =
                        "<p>Du har anmodet om at få nulstillet dit kodeord.</p><p>Brug denne nøgle for at nulstille dit kodeord. </p>" +
                        "<pre>" + resetKey + "</pre>" +
                        "<p>Eller gå til <a href=\"" + resetLink + "\">" + resetLink + "</a>" +
                        "<p>Du har til " + user.ResetExpiresAt.ToUniversalTime() + " UTC til at nulstille dit kodeord.</p>" +
                        "<p>Har du ikke anmodet om at få dit kodeord nulstillet kan du se bort fra denne mail."
                };

                message.AddTo(email);

                Web transportWeb = new Web(key);
                transportWeb.DeliverAsync(message);
            }

            return true;
        }
    }
}
