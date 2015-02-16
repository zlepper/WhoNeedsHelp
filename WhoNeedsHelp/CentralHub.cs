using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace WhoNeedsHelp
{
    public class CentralHub : Hub<IClient>
    {
        private static readonly Dictionary<string, User> Users = new Dictionary<string, User>();
        private static readonly Dictionary<string, Channel> Channels = new Dictionary<string, Channel>(); 

        public void Send(string action, string parameters)
        {
            switch (action)
            {
                // Create User
                case "1":
                    // parameter should be the username
                    SetUsername(parameters);
                    break;
                case "2":
                    // parameter is ignored.
                    RequestHelp();
                    break;
                case "3":
                    // param should be the new channel name
                    CreateNewChannel(parameters);
                    break;
                case "4":
                    // Param should be the channel to exit
                    ExitChannel(parameters);
                    break;
                case "5":
                    // Param should be the channel to search for
                    SearchForChannel(parameters);
                    break;
                case "6":
                    // Param should be the channel to join
                    JoinChannel(parameters);
                    break;
                case "7":
                    // Param should be the channel to change to
                    ChangeToChannel(parameters);
                    break;
            }
        }

        private void ChangeToChannel(string channelId)
        {
            User u = Users[Context.ConnectionId];
            int activeUsers = 0;
            if (u.CurrentChannel != null)
            {
                activeUsers = u.CurrentChannel.Users.Values.Count(user => user.CurrentChannel == u.CurrentChannel)-1;
                foreach (KeyValuePair<string, User> userPair in u.CurrentChannel.Users)
                {
                    Clients.Client(userPair.Key).UpdateChannelCount(activeUsers, u.CurrentChannel.Users.Count, u.CurrentChannel.ChannelId);
                }
            }
            Channel c = Channels[channelId];
            u.CurrentChannel = c;
            Clients.Caller.SetChannel(channelId);
            activeUsers = c.Users.Values.Count(user => user.CurrentChannel == u.CurrentChannel);
            foreach (KeyValuePair<string, User> userPair in c.Users)
            {
                Clients.Client(userPair.Key).UpdateChannelCount(activeUsers, c.Users.Count, channelId);
            }
        }

        private void JoinChannel(string channelId)
        {
            Channels[channelId].AddUser(Users[Context.ConnectionId]);
            Clients.Caller.AppendChannel(Channels[channelId].ChannelName, channelId);
        }

        private void SearchForChannel(string parameter)
        {
            var matching = from channel in Channels.Values
                where channel.ChannelId.StartsWith(parameter)
                && !channel.Users.ContainsKey(Context.ConnectionId)
                orderby channel.ChannelId
                select channel;
            var foundChannels = matching.ToList();
            string[] channelIds = new string[foundChannels.Count];
            string[] channelNames = new string[foundChannels.Count];
            for (int i = 0; i < foundChannels.Count; i++)
            {
                channelIds[i] = foundChannels[i].ChannelId;
                channelNames[i] = foundChannels[i].ChannelName;
            }
            Clients.Caller.ChannelsFound(channelIds, channelNames);
        }

        private void ExitChannel(string channelId)
        {
            if (Channels.ContainsKey(channelId))
            {
                User u = Users[Context.ConnectionId];
                Channel c = Channels[channelId];
                if (c.Administrator == u)
                {
                    // Make everybody quit a channel
                    foreach (User user in c.Users.Values)
                    {
                        Clients.Client(user.ConnectionId).ExitChannel(c.ChannelId);
                    }
                    Channels.Remove(channelId);
                }
                else
                {
                    c.RemoveUser(u);
                    Clients.Caller.ExitChannel(c.ChannelId);
                }
                
            }
        }

        private void CreateNewChannel(string channelName)
        {
            string channelId = Context.ConnectionId.Substring(0, 5) + "-" + channelName;
            if (Channels.ContainsKey(channelId))
            {
                Clients.Caller.ErrorChannelAlreadyMade();
            }
            else
            {
                Channels.Add(channelId, new Channel(Users[Context.ConnectionId]){ChannelName = channelName, ChannelId = channelId});
                Channels[channelId].AddUser(Users[Context.ConnectionId]);
                Clients.Caller.AppendChannel(channelName, channelId);
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
            Debug.WriteLine(Context.ConnectionId);
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
        void AppendChannel(string channelname, string channelid);
        void AppendUserQuestion(string html);
        void RemoveUserQuestion(string userId);
        void ErrorChannelAlreadyMade();
        void Log(string text);
        void ExitChannel(string channelId);
        void ChannelsFound(string[] channelId, string[] channelName);
        void SetChannel(string channel);
        void UpdateChannelCount(int activeUsers, int connectedUsers, string channelId);
    }
}