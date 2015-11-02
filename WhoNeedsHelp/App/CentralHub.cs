using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using WhoNeedsHelp.Models;
using WhoNeedsHelp.Server.Chat;
using WhoNeedsHelp.Server.Mail;
using WhoNeedsHelp.Simples;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace WhoNeedsHelp.App
{
    public partial class CentralHub : Hub<IClient>
    {

        private string GetIpAddress()
        {
            object temp;

            Context.Request.Environment.TryGetValue("server.RemoteIpAddress", out temp);

            string ip = temp != null ? temp as string : "";

            return ip;
        }
        

        public void GetData(int action)
        {
            switch (action)
            {
                case 2:
                    // Request a version number from the server
                    Clients.Caller.CheckVersion(4);
                    break;
            }
        }

        public override Task OnConnected()
        {
            using (HelpContext db = new HelpContext())
            {
                //var user =
                //    db.Users.SingleOrDefault(u => u.ConnectionId == Context.ConnectionId);

                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con != null) return base.OnConnected();
                User user = new User
                {
                    Ip = GetIpAddress()
                };
                user.Connections.Add(new Connection(user) { ConnectionId = Context.ConnectionId });
                db.Users.Add(user);
                db.SaveChanges();
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            using (HelpContext db = new HelpContext())
            {
                //var user = db.Users.Include(u => u.ChannelsIn).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return base.OnDisconnected(stopCalled);
                User user = con.User;
                if (user == null) return base.OnDisconnected(stopCalled);
                if (string.IsNullOrWhiteSpace(user.Pw))
                {
                    foreach (Channel c in user.ChannelsIn)
                    {
                        ExitChannel(c.Id);
                    }
                    RemoveUser(user.Id);
                }
                else
                {
                    user.Connections.Remove(con);
                }
            }
            return base.OnDisconnected(stopCalled);
        }
        
        public override Task OnReconnected()
        {
            using (HelpContext db = new HelpContext())
            {
                User user = db.Connections.Find(Context.ConnectionId).User;
                Clients.Caller.ClearChannels();
                foreach (Channel channel in user.ChannelsIn)
                {
                    var sc = channel.ToSimpleChannel();
                    sc.IsAdmin = channel.IsUserAdministrator(user);
                    Clients.Caller.AppendChannel(sc);
                }
            }

            return base.OnReconnected();
        }

        public void SendCountdownTime(int timeLeft, int channelId)
        {
            using (HelpContext db = new HelpContext())
            {
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                User user = con.User;
                if (user == null) return;

                Channel channel = db.Channels.Find(channelId);
                if (channel.IsUserAdministrator(user))
                {
                    channel.TimeLeft = timeLeft;
                }
                db.SaveChanges();
            }
        }
    }
}