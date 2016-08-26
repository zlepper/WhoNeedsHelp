using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
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
        public void JoinChannel(int channelId)
        {
            Channel channel = DB.Channels.Find(channelId);
            if (channel == null)
            {
                Clients.Caller.Alert("Kanalen med id \"" + channelId + "\" blev ikke fundet.");
                return;
            }
            User user = DB.Connections.Find(Context.ConnectionId).User;
            if (user == null) return;
            if (channel.GetUsers().Contains(user))
            {
                Clients.Caller.Alert("Du er allerede i denne kanal.");
                return;
            }
            channel.AddUser(user);
            DB.SaveChanges();
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

        public void ExitChannel(int channelId)
        {
            if (channelId == 0) return;

            Connection con = DB.Connections.Find(Context.ConnectionId);
            if (con == null) return;
            User user = con.User;
            if (user != null)
            {
                Channel channel = DB.Channels.Find(channelId);
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
                            DB.Channels.Remove(channel);
                        }
                        else
                        {
                            channel.RemoveAdministrator(user);
                            DB.SaveChanges();
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
                    Clients.Caller.Alert("Kanalen blev ikke funder");
                }
            }
            DB.SaveChanges();

        }

        public void CreateNewChannel(string channelName)
        {
            //var user = DB.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
            Connection con = DB.Connections.Find(Context.ConnectionId);
            if (con == null) return;
            User user = con.User;
            if (user == null) return;
            if (user.AreAdministratorIn.Count > 14)
            {
                Clients.Caller.Alert("Du kan ikke oprette mere end 15 kanaler af gangen");
                return;
            }
            Channel channel = new Channel(user, channelName);
            channel.AddUser(user);
            DB.Users.AddOrUpdate(user);
            DB.Channels.Add(channel);
            DB.SaveChanges();
            SimpleChannel sc = channel.ToSimpleChannel();
            sc.IsAdmin = true;
            foreach (Connection connection in user.Connections)
            {
                Clients.Client(connection.ConnectionId).AppendChannel(sc);
            }

        }

        public void RemoveUserFromChannel(int id, int channelId)
        {
            if (id == 0) return;
            User user = DB.Users.Find(id);
            User callingUser = DB.Connections.Find(Context.ConnectionId).User;
            if (callingUser == null) return;
            Channel channel = DB.Channels.Find(channelId);
            if (channel == null) return;
            if (user == null)
            {
                Clients.Caller.RemoveUser(id, channel.Id);
                return;
            }
            if (callingUser.Id == user.Id)
            {
                Clients.Caller.Alert("Du har lige forsøgt at smide dig selv ud af kanalen...");
                return;
            }
            if (channel.IsUserAdministrator(callingUser))
            {
                ExitChannel(channel, user);
            }

        }

        private void RemoveUser(int id)
        {
            User user = DB.Users.Find(id);
            if (user == null) return;
            foreach (Channel c in user.ChannelsIn)
            {
                ExitChannel(c, user);
            }
            DB.Users.Remove(user);
            DB.SaveChanges();
        }
    }
}
