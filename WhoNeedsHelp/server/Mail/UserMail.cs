using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SendGrid;
using WhoNeedsHelp.Server.Chat;

namespace WhoNeedsHelp.Server.Mail
{
    public class UserMail
    {
        private static string key;

        public UserMail()
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                ReadKey();
            }
        }

        private void ReadKey()
        {
            string root = HttpContext.Current.Server.MapPath("~");
            FileInfo file = new FileInfo(Path.Combine(root, "key"));
            if (file.Exists)
            {
                key = File.ReadAllText(file.FullName);
                return;
            }
            throw new KeyNotFoundException("Please add the sendgrid key to the key file!");
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

                SendGridMessage message = new SendGridMessage
                {
                    From = new MailAddress("noreply@zlepper.dk", "NoReply"),
                    Subject = "Nulstilling af kodeord",
                    Html =
                        "<p>Du har anmodet om at få nulstillet dit kodeord.</p><p>Indtast denne nøgle for at nulstille dit kodeord. </p>" +
                        "<pre>" + resetKey + "</pre>" +
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
