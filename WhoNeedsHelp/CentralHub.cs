using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace WhoNeedsHelp
{
    public class CentralHub : Hub<IClient>
    {
        public static Dictionary<string, User> Users = new Dictionary<string, User>();
        public static Dictionary<string, Channel> Channels = new Dictionary<string, Channel>(); 

        public void Send(int action, string[] parameters)
        {
            switch (action)
            {
                // Create User
                case 1:
                    // parameters[0] should be the username
                    SetUsername(parameters[0]);
                    break;
                case 2:
                    // parameters are ignored.
                    RequestHelp();
                    break;
                case 3:
                    // param[0] should be the new channel name
                    CreateNewChannel(parameters[0]);
                    break;

            }
        }

        private void CreateNewChannel(string channelName)
        {
            string channelId = Context.ConnectionId + channelName;
            if (Channels.ContainsKey(channelId))
            {
                Clients.Caller.ErrorChannelAlreadyMade();
            }
            else
            {
                Channels.Add(channelId, new Channel(){ChannelName = channelName, Administrator = Users[Context.ConnectionId], ChannelId = channelId});
                string html = Channels[channelId].CreateHtml();
                Clients.Caller.AppendChannel(html);

            }
        }

        private void SetUsername(string name)
        {
            if (Users.ContainsKey(Context.ConnectionId))
            {
                Users[Context.ConnectionId].Name = name;
            }
            else
            {
                Users.Add(Context.ConnectionId, new User() {ConnectionId = Context.ConnectionId, Name = name});
            }
            Clients.Caller.Log(Context.ConnectionId);
        }

        // TODO Add counter to specific rooms

        private void RequestHelp()
        {
            User u = Users[Context.ConnectionId];
            bool succesfuldRequest = u.RequestHelp();
            if (succesfuldRequest)
            {
                List<User> usersInChannel = u.CurrentChannel.GetUsers();
                String table = u.CurrentChannel.CreateTable();
                foreach (User b in usersInChannel)
                {
                    Clients.Client(b.ConnectionId).BroadcastTable(table);
                }
            }
            

        }

        public void GetData(int action)
        {
            switch (action)
            {
                case 1:
                    ListHelp();
                    break;
            }
        }

        private void ListHelp()
        {
            User u = Users[Context.ConnectionId];
            //string table = u.CurrentChannel.CreateTable();
            //Clients.Caller.BroadcastTable(table);
        }

        public override Task OnConnected()
        {
            Users.Add(Context.ConnectionId, new User() {ConnectionId = Context.ConnectionId});

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Users.Remove(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            if (!Users.ContainsKey(Context.ConnectionId))
            {
                Users.Add(Context.ConnectionId, new User() {ConnectionId = Context.ConnectionId});
            }

            return base.OnReconnected();
        }
    }

    public interface IClient
    {
        void BroadcastTable(string table);
        void AppendChannel(string html);
        void RemoveChannelFromList(string channelId);
        void AppendUserQuestion(string html);
        void RemoveUserQuestion(string userId);
        void ErrorChannelAlreadyMade();
        void Log(string text);
    }
}