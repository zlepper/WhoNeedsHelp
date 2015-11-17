using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using WhoNeedsHelp.DB;
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

        public HelpContext db = new HelpContext();

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
            Clients.Caller.SendUserId(user.Id);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Connection con = db.Connections.Find(Context.ConnectionId);
            if (con == null) return base.OnDisconnected(stopCalled);
            db.Connections.Remove(con);


            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            User user = db.Connections.Find(Context.ConnectionId).User;
            Clients.Caller.ClearChannels();
            foreach (Channel channel in user.ChannelsIn)
            {
                var sc = channel.ToSimpleChannel();
                sc.IsAdmin = channel.IsUserAdministrator(user);
                Clients.Caller.AppendChannel(sc);
            }

            return base.OnReconnected();
        }

        public void SendCountdownTime(int timeLeft, int channelId)
        {
            Connection con = db.Connections.Find(Context.ConnectionId);
            User user = con?.User;
            if (user == null) return;

            Channel channel = db.Channels.Find(channelId);
            if (channel.IsUserAdministrator(user))
            {
                channel.TimeLeft = timeLeft;
            }
        }

        public void CleaupTime(int id)
        {
            User user = db.Connections.Find(Context.ConnectionId)?.User;
            if (user == null) return;

            var timer = db.CleanupAlarms.Find(id);
            if (timer == null) return;

            if (timer.Channel == null)
            {
                if (timer.User?.Id != user.Id) return;

                // The timer is linked to the user

                // Tell all the users in the administrators administrating channels to cleanup
                Clients.Clients(
                    db.Users.Find(user.Id).AreAdministratorIn
                        .SelectMany(channel => channel.Users)
                        .SelectMany(u => u.Connections)
                        .Select(c => c.ConnectionId)
                        .ToArray()).CleanupTime();

            }
            else
            {
                if (timer.User != null) return;

                // The timer is linked to the channel

                // Tell all the users in the channel to clean up
                Clients.Clients(timer.Channel.Users.SelectMany(u => u.Connections).Select(c => c.ConnectionId).ToArray()).CleanupTime();

            }
        }

        protected override void Dispose(bool disposing)
        {
            db.SaveChanges();
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}