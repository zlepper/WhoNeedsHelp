using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoNeedsHelp.Server.Chat;
using WhoNeedsHelp.Simples;

namespace WhoNeedsHelp.App
{
    public partial class CentralHub
    {
        public void JoinChannel(int channelId)
        {
            using (HelpContext db = new HelpContext())
            {
                Channel channel = db.Channels.Find(channelId);
                if (channel == null)
                {
                    Clients.Caller.Alert("Kanalen med id \"" + channelId + "\" blev ikke fundet.", "Kanal ikke fundet", "info");
                    return;
                }
                User user = db.Connections.Find(Context.ConnectionId).User;
                if (user == null) return;
                if (channel.GetUsers().Contains(user))
                {
                    Clients.Caller.Alert("Du er allerede i denne kanal.", "Allerede i kanal", "info");
                    return;
                }
                channel.AddUser(user);
                db.SaveChanges();
                SimpleChannel sc = channel.ToSimpleChannel();
                foreach (Connection connection in user.Connections)
                {
                    Clients.Client(connection.ConnectionId).AppendChannel(sc);
                }
                SimpleUser su = user.ToSimpleUser();
                foreach (Connection connection in channel.Users.SelectMany(us => us.Connections))
                {
                    Clients.Client(connection.ConnectionId).AppendUser(su, channel.Id);
                }
            }
        }

        public void ExitChannel(int channelId)
        {
            if (channelId == 0) return;
            using (HelpContext db = new HelpContext())
            {
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                User user = con.User;
                if (user != null)
                {
                    Channel channel = db.Channels.Find(channelId);
                    if (channel != null)
                    {
                        if (channel.IsUserAdministrator(user))
                        {
                            if (channel.Administrators.Count <= 1)
                            {
                                // Make everybody quit the channel
                                foreach (Connection connection in channel.Users.SelectMany(u => u.Connections))
                                {
                                    Clients.Client(connection.ConnectionId).ExitChannel(channelId);
                                }
                                db.Channels.Remove(channel);
                            }
                            else
                            {
                                channel.RemoveAdministrator(user);
                                db.SaveChanges();
                                ExitChannel(channelId);
                            }
                        }
                        else
                        {
                            channel.RemoveUser(user);
                            Question question = user.GetQuestion(channel);
                            if (channel.GetUsersRequestingHelp().Contains(user))
                            {
                                channel.RemoveUserRequestingHelp(user);
                            }
                            if (question != null)
                                foreach (Connection connec in channel.Users.SelectMany(us => us.Connections))
                                {
                                    Clients.Client(connec.ConnectionId).RemoveQuestion(question.Id);
                                }
                            foreach (Connection connection in user.Connections)
                            {
                                Clients.Client(connection.ConnectionId).ExitChannel(channelId);
                            }
                            foreach (Connection connection in channel.Users.SelectMany(us => us.Connections))
                            {
                                Clients.Client(connection.ConnectionId).RemoveUser(user.Id, channelId);
                            }
                        }
                    }
                    else
                    {
                        Clients.Caller.Alert("Kanalen blev ikke funder", "Kan ikke findes", "error");
                    }
                }
                db.SaveChanges();
            }
        }

        public void CreateNewChannel(string channelName)
        {
            using (HelpContext db = new HelpContext())
            {
                //var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                User user = con.User;
                if (user == null) return;
                if (user.AreAdministratorIn.Count > 4)
                {
                    Clients.Caller.Alert("Du kan ikke oprette mere end 5 kanaler af gangen", "Maks oprettede kanaler nået", "error");
                    return;
                }
                Channel channel = new Channel(user, channelName);
                channel.AddUser(user);
                db.Users.AddOrUpdate(user);
                db.Channels.Add(channel);
                db.SaveChanges();
                SimpleChannel sc = channel.ToSimpleChannel();
                sc.IsAdmin = true;
                foreach (Connection connection in user.Connections)
                {
                    Clients.Client(connection.ConnectionId).AppendChannel(sc);
                }
            }
        }

        public void RemoveUserFromChannel(int id, int channelId)
        {
            if (id == 0) return;
            using (HelpContext db = new HelpContext())
            {
                User user = db.Users.Find(id);
                User callingUser = db.Connections.Find(Context.ConnectionId).User;
                if (callingUser == null) return;
                Channel channel = db.Channels.Find(channelId);
                if (channel == null) return;
                if (user == null)
                {
                    Clients.Caller.RemoveUser(id, channel.Id);
                    return;
                }
                if (callingUser.Id == user.Id)
                {
                    Clients.Caller.Alert("Du har lige forsøgt at smide dig selv ud af kanalen...", "Really?!", "warning");
                    return;
                }
                if (channel.IsUserAdministrator(callingUser))
                {
                    ExitChannel(channel, user);
                }
            }
        }

        private void RemoveUser(int id)
        {
            using (HelpContext db = new HelpContext())
            {
                User user = db.Users.Find(id);
                if (user == null) return;
                foreach (Channel c in user.ChannelsIn)
                {
                    ExitChannel(c, user);
                }
                db.Users.Remove(user);
                db.SaveChanges();
            }
        }
    }
}
