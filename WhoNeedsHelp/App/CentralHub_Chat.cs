using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoNeedsHelp.DB;
using WhoNeedsHelp.Models;
using WhoNeedsHelp.Server.Chat;
using WhoNeedsHelp.Simples;

namespace WhoNeedsHelp.App
{
    public partial class CentralHub
    {
        public void RemoveChatMessage(int messageId)
        {
            if (messageId == 0) return;
            User user = db.Connections.Find(Context.ConnectionId).User;
            if (user == null) return;
            ChatMessage message = db.ChatMessages.Find(messageId);
            if (message == null)
            {
                return;
            }
            Channel c = message.Channel;
            if (c == null) return;
            if (!c.IsUserAdministrator(user) && !message.User.Equals(user))
                return;
            Clients.Clients(c.Users.SelectMany(u => u.Connections).Select(co => co.ConnectionId).ToList()).RemoveChatMessage(message.Id);
            db.ChatMessages.Remove(message);
            db.SaveChanges();
        }

        public void Chat(string message, int channelId)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            User user = db.Connections.Find(Context.ConnectionId).User;
            if (user == null) return;
            Channel channel = db.Channels.Find(channelId);
            if (channel == null)
            {
                Clients.Caller.Alert("Something went odd");
                return;
            }
            if (message.StartsWith("/"))
            {
                string[] parts = message.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length <= 0)
                {
                    Clients.Caller.Alert("Du har ikke indtastet nogen kommando.");
                    return;
                }
                switch (parts[0])
                {
                    case "/op":
                        PromoteToAdmin(parts, channelId);
                        break;
                    default:
                        Clients.Caller.Alert("Ukendt kommando");
                        return;
                }
            }
            else
            {
                ChatMessage chatMessage = channel.AddChatMessage(user, message);
                if (chatMessage != null)
                {
                    db.ChatMessages.Add(chatMessage);
                    db.SaveChanges();
                    SimpleChatMessage scm = chatMessage.ToSimpleChatMessage();
                    Clients.Clients(channel.Users.SelectMany(u => u.Connections).Select(c => c.ConnectionId).ToList()).SendChatMessage(scm, channelId);
                }
            }

            db.SaveChanges();

        }

        public void ClearChat(int channelId)
        {
            Connection con = db.Connections.Find(Context.ConnectionId);
            if (con == null) return;
            User user = con.User;
            if (user == null) return;
            Channel channel = db.Channels.Find(channelId);
            if (channel == null) return;
            if (channel.IsUserAdministrator(user))
            {
                db.ChatMessages.RemoveRange(channel.ChatMessages);
                db.SaveChanges();
                foreach (Connection connection in channel.Users.SelectMany(u => u.Connections))
                {
                    Clients.Client(connection.ConnectionId).ClearChat(channelId);
                }
            }
            else
            {
                foreach (Connection connection in user.Connections)
                {
                    Clients.Client(connection.ConnectionId).ClearChat(channelId);
                }
            }

        }

        private void PromoteToAdmin(string[] parts, int channelId)
        {
            User callingUser = db.Connections.Find(Context.ConnectionId).User;
            Channel channel = db.Channels.Find(channelId);
            if (!channel.IsUserAdministrator(callingUser))
            {
                Clients.Caller.Alert("Du skal være administrator i kanalen for at kunne bruge denne command.");
                return;
            }
            if (parts.Length < 2)
            {
                Clients.Caller.Alert("Du har ikke specificeret nogen personer at promote til administrator.");
                return;
            }
            foreach (string s in parts)
            {
                if (s.Equals("/op", StringComparison.OrdinalIgnoreCase)) continue;
                if (s.Contains("@"))
                {
                    string mail = s;
                    User user = db.Users.First(u => u.EmailAddress.Equals(mail, StringComparison.OrdinalIgnoreCase));
                    if (user == null)
                    {
                        Clients.Caller.Alert("Ingen bruger fundet med email \"" + mail + "\"");
                        continue;
                    }
                    if (!user.ChannelsIn.Contains(channel))
                    {
                        Clients.Caller.Alert("Brugeren med \"" + mail + "\" er ikke i denne kanal.");
                        continue;
                    }
                    channel.AddAdministrator(user);
                    db.SaveChanges();
                    foreach (Connection con in user.Connections)
                    {
                        Clients.Client(con.ConnectionId)
                            .Alert(
                                "Du er blevet gjort til administrator i kanalen \"" +
                                channel.ChannelName + "\".");
                        Clients.Client(con.ConnectionId).SetAdminState(channel.Id, true);
                    }
                    Clients.Caller.Alert(user.Name + " er nu blevet gjort til administrator.");

                }
                else
                {
                    Clients.Caller.Alert(s + " Er ikke en emailaddresse");
                }

            }
        }
    }
}
