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
        public IHelpContext DB = new HelpContext();
        public CentralHub()
        {
            
        }

        public CentralHub(IHelpContext context)
        {
            DB = context;
        }


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
            User user = Context.User as User;
            if (user != null)
            {
                // Get a db instance instead
                user = DB.GetUserById(user.Id);
                Clients.Caller.UpdateUsername(user.Name);
                Clients.Caller.SendUserId(user.Id);
                Clients.Caller.LoginSuccess();
                foreach (Channel channel in user.ChannelsIn)
                {
                    Clients.Caller.AppendChannel(channel.ToSimpleChannel(channel.IsUserAdministrator(user)));
                }
                user.Connections.Add(new Connection() {ConnectionId = Context.ConnectionId});
            }
            else
            {

                Connection con = DB.Connections.Find(Context.ConnectionId);
                if (con != null) return base.OnConnected();
                user = new User
                {
                    Ip = GetIpAddress()
                };
                user.Connections.Add(new Connection(user) {ConnectionId = Context.ConnectionId});
                DB.Users.Add(user);
                DB.SaveChanges();
                Clients.Caller.SendUserId(user.Id);
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Connection con = DB.Connections.Find(Context.ConnectionId);
            if (con == null) return base.OnDisconnected(stopCalled);
            DB.Connections.Remove(con);


            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            User user = DB.GetUserByConnection(Context.ConnectionId);
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
            User user = DB.GetUserByConnection(Context.ConnectionId);
            Channel channel = DB.GetChannelById(channelId);
            if (channel.IsUserAdministrator(user))
            {
                channel.TimeLeft = timeLeft;
            }
        }

        public void CleaupTime(int id)
        {
            User user = DB.GetUserByConnection(Context.ConnectionId);
            if (user == null) return;

            var timer = DB.CleanupAlarms.Find(id);
            if (timer == null) return;

            if (timer.Channel == null)
            {
                if (timer.User?.Id != user.Id) return;

                // The timer is linked to the user

                // Tell all the users in the administrators administrating channels to cleanup
                Clients.Clients(
                    DB.Users.Find(user.Id).AreAdministratorIn
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
            DB.SaveChanges();
            DB.Dispose();
            base.Dispose(disposing);
        }
    }
}