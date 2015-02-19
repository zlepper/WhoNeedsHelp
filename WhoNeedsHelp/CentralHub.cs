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
                    // Param should be the question to ask
                    RequestHelp(parameters);
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
                case "8":
                    // Param are ignored
                    RemoveQuestion();
                    break;
                case "9":
                    // Param are the new question
                    ChangeQuestion(parameters);
                    break;
            }
        }

        private void ChangeQuestion(string question)
        {
            User u = Users[Context.ConnectionId];
            u.UpdateQuestion(u.CurrentChannel, question);
            foreach (User user in u.CurrentChannel.GetActiveUsers())
            {
                
            }
        }

        private void RemoveQuestion()
        {
            User u = Users[Context.ConnectionId];
            Channel c = u.CurrentChannel;
            string questionId = u.ConnectionId + "-" + c.ChannelId;
            u.RemoveQuestion();
            c.UsersRequestingHelp.Remove(u);
            foreach (User user in c.GetActiveUsers())
            {
                Clients.Client(user.ConnectionId).RemoveQuestion(questionId);
            }
        }

        private void ChangeToChannel(string channelId)
        {
            User u = Users[Context.ConnectionId];
            int activeUsers = 0;
            if (u.CurrentChannel != null)
            {
                activeUsers = u.CurrentChannel.GetActiveUserCount()-1;
                foreach (KeyValuePair<string, User> userPair in u.CurrentChannel.Users)
                {
                    Clients.Client(userPair.Key).UpdateChannelCount(activeUsers, u.CurrentChannel.Users.Count, u.CurrentChannel.ChannelId);
                }
            }
            if (Channels.ContainsKey(channelId))
            {
                Channel c = Channels[channelId];
                u.CurrentChannel = c;
                Clients.Caller.SetChannel(channelId);
                activeUsers = c.GetActiveUserCount();
                foreach (KeyValuePair<string, User> userPair in c.Users)
                {
                    Clients.Client(userPair.Key).UpdateChannelCount(activeUsers, c.Users.Count, channelId);
                }
                List<User> questionUsers = c.UsersRequestingHelp;
                List<string> usernames = new List<string>(), questions = new List<string>(), questionIds = new List<string>();
                foreach (User user in questionUsers)
                {
                    usernames.Add(user.Name);
                    questions.Add(user.GetQuestion(c));
                    questionIds.Add(user.ConnectionId + "-" + c.ChannelId);
                }
                Clients.Caller.AddQuestions(usernames.ToArray(), questions.ToArray(), questionIds.ToArray(), c.Administrator == u);
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
                    int activeUsers = c.GetActiveUserCount();
                    foreach (string connectionId in Channels[channelId].Users.Keys)
                    {
                        Clients.Client(connectionId).UpdateChannelCount(activeUsers, c.Users.Count, c.ChannelId);
                    }
                }
                
            }
        }

        private void CreateNewChannel(string channelName)
        {
            string channelId = Context.ConnectionId.Substring(0, 5) + "-" + channelName.Replace(" ", "-");
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

        private void RequestHelp(string question)
        {
            if (String.IsNullOrWhiteSpace(question))
            {
                question = "";
            }
            User u = Users[Context.ConnectionId];
            bool succesfuldRequest = u.RequestHelp(question);
            if (succesfuldRequest)
            {
                Channel c = u.CurrentChannel;
                List<User> usersInChannel = c.GetActiveUsers();
                string questionId = Context.ConnectionId + "-" + c.ChannelId;
                foreach (User user in usersInChannel)
                {
                    Clients.Client(user.ConnectionId).AddQuestion(u.Name, question, questionId, c.Administrator == user);
                }
            }
            

        }

        public void GetData(int action)
        {
            switch (action)
            {
                case 1:
                    // Request question text in current channel
                    GetQuestion();
                    break;
            }
        }

        private void GetQuestion()
        {
            string question = Users[Context.ConnectionId].GetQuestion(Users[Context.ConnectionId].CurrentChannel);
            Debug.WriteLine(question);
            if (!String.IsNullOrWhiteSpace(question))
            {
                Clients.Caller.SendQuestion(question);
            }
        }

        public override Task OnConnected()
        {
            Clients.Caller.Log("Connected");
            Debug.WriteLine("Connected: " + Context.ConnectionId);
            Users.Add(Context.ConnectionId, new User() {ConnectionId = Context.ConnectionId});

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            User u = Users[Context.ConnectionId];
            Debug.WriteLine("Disconnected: " + Context.ConnectionId);
            var channelsToClear = from channel in Channels.Values
                where channel.Administrator == u
                select channel;
            var ctc = channelsToClear.ToList();
            for (int i = 0; i < ctc.Count(); i++)
            {
                ExitChannel(ctc[i].ChannelId);
            }
            Users.Remove(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            Clients.Caller.Log("Reconnected");
            Debug.WriteLine("Reconnected: " + Context.ConnectionId);
            if (!Users.ContainsKey(Context.ConnectionId))
            {
                Users.Add(Context.ConnectionId, new User() {ConnectionId = Context.ConnectionId});
            }

            var channels = from channel in Channels.Values
                where channel.Users.ContainsKey(Context.ConnectionId)
                orderby channel.ChannelName
                select channel;
            var cs = channels.ToList();
            List<string> channelNames = new List<string>(), channelIds = new List<string>();
            foreach (Channel c in cs)
            {
                channelNames.Add(c.ChannelName);
                channelIds.Add(c.ChannelId);
            }

            Clients.Caller.ListChannels(channelNames.ToArray(), channelIds.ToArray(),
                Users[Context.ConnectionId].CurrentChannel != null
                    ? Users[Context.ConnectionId].CurrentChannel.ChannelId
                    : null);
            Clients.Caller.GetUsername();

            return base.OnReconnected();
        }


    }

    public interface IClient
    {
        void AppendChannel(string channelname, string channelid);
        void AddQuestions(string[] usernames, string[] questions, string[] questionIds, bool admin = false);
        void AddQuestion(string username, string question, string questionId, bool admin = false);
        void RemoveQuestion(string questionId);
        void ErrorChannelAlreadyMade();
        void Log(string text);
        void ExitChannel(string channelId);
        void ChannelsFound(string[] channelId, string[] channelName);
        void SetChannel(string channel);
        void UpdateChannelCount(int activeUsers, int connectedUsers, string channelId);
        void ListChannels(string[] channelNames, string[] channelIds, string activeChannelId);
        void GetUsername();
        void SendQuestion(string question);
        void UpdateQuestion(string question, string questionId);
    }
}