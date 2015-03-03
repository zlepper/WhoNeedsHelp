using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using WhoNeedsHelp.server;
using System.Data.Entity;
using System.Linq;

namespace WhoNeedsHelp
{
    public class CentralHub : Hub<IClient>
    {
        //private static readonly Dictionary<string, User> Users = new Dictionary<string, User>();
        //private static readonly Dictionary<string, Channel> Channels = new Dictionary<string, Channel>();

        private string GetIpAddress()
        {
            object temp;

            Context.Request.Environment.TryGetValue("server.RemoteIpAddress", out temp);

            var ip = temp != null ? temp as string : "";

            return ip;
        }

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
                    RemoveQuestion(parameters);
                    break;
                case "9":
                    // Param are the new question
                    ChangeQuestion(parameters);
                    break;
                case "10":
                    // Param should be the new message
                    Chat(parameters);
                    break;
                case "11":
                    RemoveChatMessage(parameters);
                    break;
            }
        }

        private void RemoveChatMessage(string messageId)
        {
            /*Channel c = Users[Context.ConnectionId].CurrentChannel;
            var messages = from message in c.ChatMessages.Values
                where message.Id == messageId
                select message;
            var messagesList = messages.ToList();
            foreach (User user in c.GetActiveUsers())
            {
                foreach (ChatMessage message in messagesList)
                {
                    Clients.Client(user.ConnectionId).RemoveChatMessage(message.Id);
                    c.ChatMessages.Remove(message.Id);
                }
            }*/
            if (String.IsNullOrWhiteSpace(messageId)) return;
            using (var db = new HelpContext())
            {
                var tu = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (tu == null) return;
                Channel c = tu.Channel;
                if (c == null) return;
                var message = db.ChatMessages.Find(Int32.Parse(messageId));
                var user = db.Users.Find(Context.ConnectionId);
                if (message == null || (!c.IsUserAdministrator(user) && !message.User.Equals(user)))
                    return;
                List<User> users = c.GetActiveUsers();
                foreach (User u in users)
                {
                    Clients.Client(u.ConnectionId).RemoveChatMessage(message.Id.ToString());
                }
            }

        }


        private void Chat(string message)
        {
            if (String.IsNullOrWhiteSpace(message)) return;
            using (var db = new HelpContext())
            {
                var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user == null) return;
                var channel = user.Channel;
                if (channel == null) return;
                int chatMessage = channel.AddChatMessage(user, message);
                if (chatMessage != -1)
                {
                    var users = channel.GetActiveUsers();
                    ChatMessage cm = db.ChatMessages.Find(chatMessage);
                    foreach (User u in users)
                    {
                        Clients.Client(u.ConnectionId)
                            .SendChatMessage(cm.Text, u.Name,
                                cm.Id.ToString(), cm.User.Equals(u), channel.AppendMessageToLast(cm),
                                cm.User.Equals(u) || channel.IsUserAdministrator(u));
                    }
                }

                db.SaveChanges();
            }
        }

        private void ChangeQuestion(string question)
        {
            /*User u = Users[Context.ConnectionId];
            u.UpdateQuestion(u.CurrentChannel, question);
            string questionId = u.ConnectionId + "-" + u.CurrentChannel.ChannelId;
            foreach (User user in u.CurrentChannel.GetActiveUsers())
            {
                Clients.Client(user.ConnectionId).UpdateQuestion(String.IsNullOrWhiteSpace(question) ? "" : question, questionId);
            }*/
            using (var db = new HelpContext())
            {
                var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user == null) return;
                Channel channel = db.Channels.Find(user.Channel);
                if (channel == null) return;
                user.UpdateQuestion(channel, question);
                Question q = user.GetQuestion(channel);
                if (q == null) return;
                foreach (User use in channel.GetActiveUsers())
                {
                    Clients.Client(use.ConnectionId).UpdateQuestion(String.IsNullOrWhiteSpace(q.Text) ? "" : question, q.Id.ToString());
                }
                db.SaveChanges();
            }
        }

        private void RemoveQuestion(string questionId)
        {
            if (String.IsNullOrWhiteSpace(questionId))
            {
                using (var db = new HelpContext())
                {
                    var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                    if (user != null)
                    {
                        Channel channel = user.Channel;
                        questionId = user.GetQuestion(channel).ToString();
                        user.RemoveQuestion();
                        channel.RemoveUserRequestingHelp(user);
                        foreach (User u in channel.GetActiveUsers())
                        {
                            Clients.Client(u.ConnectionId).RemoveQuestion(questionId);
                        }
                    }
                    db.SaveChanges();
                }
            }
            else
            {
                using (var db = new HelpContext())
                {
                    var us = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                    if (us == null) return;
                    var channel = us.Channel;
                    if (channel == null) return;
                    Question q = db.Questions.Find(Int32.Parse(questionId));
                    User user = db.Users.Find(q.User);
                    if (user == null) return;
                    user.RemoveQuestion(channel);
                    channel.RemoveUserRequestingHelp(user);
                    foreach (User use in channel.GetActiveUsers())
                    {
                        Clients.Client(use.ConnectionId).RemoveQuestion(questionId);
                    }
                    Clients.Client(user.ConnectionId).SetLayout(1);
                }
            }
        }

        private void RemoveQuestion(User u, Channel c)
        {
            if (u == null || c == null)
            {
                return;
            }
            using (var db = new HelpContext())
            {
                Question question = u.GetQuestion(c);
                //Question question = db.Questions.Find(questionGuid);
                u.RemoveQuestion(c);
                Channel channel = db.Channels.Find(c);
                channel.RemoveUserRequestingHelp(u);
                foreach (User use in channel.GetActiveUsers())
                {
                    Clients.Client(use.ConnectionId).RemoveQuestion(question.Id.ToString());
                }

            }
        }

        private void ChangeToChannel(string channelId)
        {
            /*User u = Users[Context.ConnectionId];
            int activeUsers;
            if (u.CurrentChannel != null)
            {
                activeUsers = u.CurrentChannel.GetQuestingUserCount()-1;
                foreach (KeyValuePair<string, User> userPair in u.CurrentChannel.Users)
                {
                    Clients.Client(userPair.Key).UpdateChannelCount(activeUsers, u.CurrentChannel.Users.Count, u.CurrentChannel.ChannelId);
                }
            }
            if (!string.IsNullOrWhiteSpace(channelId) && Channels.ContainsKey(channelId))
            {
                Channel c = Channels[channelId];
                u.CurrentChannel = c;
                Clients.Caller.SetChannel(channelId, u.AreUserQuestioning(c));
                activeUsers = c.GetQuestingUserCount();
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
                List<ChatMessage> chatMessages = c.ChatMessages.Values.ToList();
                List<string> textList = new List<string>();
                List<string> authorList = new List<string>();
                List<string> messageIdsList = new List<string>();
                List<bool> senderList = new List<bool>();
                List<bool> appendToLastList = new List<bool>();
                List<bool> canEditList = new List<bool>();
                for (int index = 0; index < chatMessages.Count; index++)
                {
                    ChatMessage chatMessage = chatMessages[index];
                    textList.Add(chatMessage.Text);
                    authorList.Add(chatMessage.User.Name);
                    messageIdsList.Add(chatMessage.GetMessageId());
                    senderList.Add(chatMessage.User == u);
                    appendToLastList.Add(c.AppendMessageToLast(index, chatMessage.User));
                    canEditList.Add(chatMessage.User == u || c.Administrator == u);
                }
                Clients.Caller.SendChatMessages(textList.ToArray(), authorList.ToArray(), messageIdsList.ToArray(), senderList.ToArray(), appendToLastList.ToArray(), canEditList.ToArray());
            }*/
            using (var db = new HelpContext())
            {
                var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user != null)
                {
                    Channel channel = user.Channel;
                    if (channel != null)
                    {
                        int activeUsers = channel.GetQuestingUserCount()-1;
                        int userCount = channel.GetUsers().Count;
                        foreach (User use in channel.GetUsers())
                        {
                            Clients.Client(use.ConnectionId).UpdateChannelCount(activeUsers, userCount, channel.Id.ToString());
                        }
                    }
                    channel = null;
                    if (String.IsNullOrWhiteSpace(channelId)) return;
                    int id = Int32.Parse(channelId);
                    channel = db.Channels.Find(id);
                    if (channel != null)
                    {
                        user.Channel = channel;
                        Clients.Caller.SetChannel(channelId, user.AreUserQuestioning(channel));
                        int activeUsers = channel.GetQuestingUserCount();
                        int userCount = channel.GetUsers().Count;
                        foreach (User use in channel.GetUsers())
                        {
                            Clients.Client(use.ConnectionId).UpdateChannelCount(activeUsers, userCount, channelId);
                        }
                        List<User> questionUsers = channel.GetUsersRequestingHelp();
                        List<string> usernames = new List<string>(), questions = new List<string>(), questionIds = new List<string>();
                        foreach (User us in questionUsers)
                        {
                            var u = db.Users.Find(us);
                            usernames.Add(u.Name);
                            Question question = u.GetQuestion(channel);
                            //Question question = db.Questions.Find(questionGuid);
                            questions.Add(question.Text);
                            questionIds.Add(question.Id.ToString());
                        }
                        Clients.Caller.AddQuestions(usernames.ToArray(), questions.ToArray(), questionIds.ToArray(), channel.IsUserAdministrator(user));
                        var chatMessages = db.ChatMessages.Where(cm => cm.Channel.Id.Equals(channel.Id));
                        List<string> textList = new List<string>();
                        List<string> authorList = new List<string>();
                        List<string> messageIdsList = new List<string>();
                        List<bool> senderList = new List<bool>();
                        List<bool> appendToLastList = new List<bool>();
                        List<bool> canEditList = new List<bool>();
                        foreach (ChatMessage chatMessage in chatMessages)
                        {
                            var chatMessageAuthor = db.Users.Find(chatMessage.User);
                            textList.Add(chatMessage.Text);
                            authorList.Add(chatMessageAuthor.Name);
                            //authorList.Add(chatMessage.User.Name);
                            messageIdsList.Add(chatMessage.Id.ToString());
                            senderList.Add(chatMessage.User.Equals(user));
                            appendToLastList.Add(channel.AppendMessageToLast(chatMessage));
                            canEditList.Add(chatMessage.User.Equals(user) || channel.IsUserAdministrator(user));
                        }
                        Clients.Caller.SendChatMessages(textList.ToArray(), authorList.ToArray(), messageIdsList.ToArray(), senderList.ToArray(), appendToLastList.ToArray(), canEditList.ToArray());

                    }
                }
                db.SaveChanges();
            }

        }

        private void JoinChannel(string channelId)
        {
            /*if (!Channels[channelId].Users.ContainsValue(Users[Context.ConnectionId]))
            {
                Channels[channelId].AddUser(Users[Context.ConnectionId]);
                Clients.Caller.AppendChannel(Channels[channelId].ChannelName, channelId);
            }*/
            using (var db = new HelpContext())
            {
                var channel = db.Channels.Find(Int32.Parse(channelId));
                if (channel != null)
                {
                    var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                    if (user != null)
                    {
                        if (!channel.GetUsers().Contains(user))
                        {
                            channel.AddUser(user);
                            Clients.Caller.AppendChannel(channel.ChannelName, channelId);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        private void SearchForChannel(string parameter)
        {
            parameter = parameter.ToLower();
            /*var matching = from channel in Channels.Values
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
            Clients.Caller.ChannelsFound(channelIds, channelNames);*/
            using (var db = new HelpContext())
            {
                var matches = db.Channels.Where(c => c.Id.ToString().StartsWith(parameter)).ToList();
                string[] channelIds = new string[matches.Count];
                string[] channelNames = new string[matches.Count];
                for (int i = 0; i < matches.Count; i++)
                {
                    channelIds[i] = matches[i].Id.ToString();
                    channelNames[i] = matches[i].ChannelName;
                }
                Clients.Caller.ChannelsFound(channelIds, channelNames);
            }
        }

        private void ExitChannel(string channelId)
        {
            /*if (Channels.ContainsKey(channelId))
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
                    string ip = GetIpAddress();
                    var users = from user in Users.Values
                                where user.ip.Equals(ip)
                                select user;

                    var channels = from channel in Channels.Values
                                   where channel.Administrator.ip.Equals(ip)
                                   select channel;

                    List<string> channelIds = new List<string>();
                    List<string> channelNames = new List<string>();
                    foreach (Channel ch in channels)
                    {
                        channelIds.Add(ch.ChannelId);
                        channelNames.Add(ch.ChannelName);
                    }
                    string[] cIds = channelIds.ToArray();
                    string[] cNames = channelNames.ToArray();
                    foreach (User us in users)
                    {
                        Clients.Client(us.ConnectionId).IpDiscover(cIds, cNames);
                    }
                }
                else
                {
                    c.RemoveUser(u);
                    if (c.UsersRequestingHelp.Contains(u))
                    {
                        c.UsersRequestingHelp.Remove(u);
                    }
                    Clients.Caller.ExitChannel(c.ChannelId);
                    int activeUsers = c.GetQuestingUserCount();
                    foreach (string connectionId in Channels[channelId].Users.Keys)
                    {
                        Clients.Client(connectionId).UpdateChannelCount(activeUsers, c.Users.Count, c.ChannelId);
                    }


                }
                
            }*/
            using (var db = new HelpContext())
            {
                var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user != null)
                {

                    var channel = db.Channels.Find(new Guid(channelId));
                    if (channel != null)
                    {
                        if (channel.IsUserAdministrator(user))
                        {
                            // Make everybody quit the channel
                            foreach (User u in channel.GetUsers())
                            {
                                Clients.Client(u.ConnectionId).ExitChannel(channelId);
                            }
                            db.Channels.Remove(channel);
                        }
                        else
                        {
                            channel.RemoveUser(user);
                            if (channel.GetUsersRequestingHelp().Contains(user))
                                channel.RemoveUserRequestingHelp(user);
                            Clients.Caller.ExitChannel(channelId);
                            int activeUsers = channel.GetQuestingUserCount();
                            int userCount = channel.GetUsers().Count;
                            foreach (User us in channel.GetUsers())
                            {
                                Clients.Client(us.ConnectionId).UpdateChannelCount(activeUsers, userCount, channelId);
                            }
                        }
                    }
                }
                db.SaveChanges();
            }


        }

        private void CreateNewChannel(string channelName)
        {
            //string channelId = Context.ConnectionId.Substring(0, 5) + "-" + channelName.Replace(" ", "-");
            /*if (Channels.ContainsKey(channelId))
            {
                Clients.Caller.ErrorChannelAlreadyMade();
            }
            else
            {
                Channels.Add(channelId, new Channel(Users[Context.ConnectionId]){ChannelName = channelName, ChannelId = channelId});
                Channels[channelId].AddUser(Users[Context.ConnectionId]);
                Clients.Caller.AppendChannel(channelName, channelId);
            }
            string ip = GetIpAddress();
            var users = from user in Users.Values
                where user.ip.Equals(ip)
                select user;

            var channels = from channel in Channels.Values
                where channel.Administrator.ip.Equals(ip)
                select channel;

            List<string> channelIds = new List<string>();
            List<string> channelNames = new List<string>();
            foreach (Channel c in channels)
            {
                channelIds.Add(c.ChannelId);
                channelNames.Add(c.ChannelName);
            }
            string[] cIds = channelIds.ToArray();
            string[] cNames = channelNames.ToArray();
            foreach (User u in users)
            {
                Clients.Client(u.ConnectionId).IpDiscover(cIds, cNames);
            }*/
            using (var db = new HelpContext())
            {
                //var channel = new Channel(db.Users.First(u => u.ConnectionId.Equals(Context.ConnectionId)).Id);
                var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user != null)
                {
                    var channel = new Channel(user, channelName);
                    channel.AddUser(user);
                    user.Channel = channel;
                    db.Channels.Add(channel);
                    db.SaveChanges();
                    Clients.Caller.AppendChannel(channelName, channel.Id.ToString());
                }
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void SetUsername(string name)
        {
            /*if (Users.ContainsKey(Context.ConnectionId))
            {
                Users[Context.ConnectionId].Name = name;
                //Users[Context.ConnectionId].ip = GetIpAddress();
            }
            else
            {
                Users.Add(Context.ConnectionId, new User() {ConnectionId = Context.ConnectionId, Name = name, ip = GetIpAddress()});
            }
            Clients.Caller.Log(Context.ConnectionId);*/
            using (var db = new HelpContext())
            {
                var user =
                    db.Users.SingleOrDefault(u => u.ConnectionId == Context.ConnectionId);
                if (user == null)
                {
                    user = new User
                    {
                        Name = name,
                        //Connections = new List<Connection>(),
                        ConnectionId = Context.ConnectionId
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                else
                {
                    /*user.Connections.Add(new Connection()
                    {
                        ConnectionID = Context.ConnectionId
                    });*/
                    user.Name = name;
                    db.SaveChanges();
                }
            }
        }

        private void RequestHelp(string question)
        {
            /*if (String.IsNullOrWhiteSpace(question))
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
            }*/
            if (String.IsNullOrWhiteSpace(question))
                question = "";
            using (var db = new HelpContext())
            {
                var userFromDb = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                bool successfuldRequest = userFromDb != null && userFromDb.RequestHelp(question);
                db.SaveChanges();
                if (successfuldRequest)
                {
                    Channel channel = userFromDb.Channel;
                    Question q = userFromDb.GetQuestion(channel);
                    db.SaveChanges();
                    List<User> usersInChannel = channel.GetActiveUsers();
                    foreach (User user in usersInChannel)
                    {
                        Clients.Client(user.ConnectionId).AddQuestion(user.Name, q.Text, q.Id.ToString(), channel.IsUserAdministrator(user));
                    }
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
                case 2:
                    // Request a version number from the server
                    Clients.Caller.CheckVersion(1);
                    break;
            }
        }

        private void GetQuestion()
        {
            /*string question = Users[Context.ConnectionId].GetQuestion(Users[Context.ConnectionId].CurrentChannel);
            Debug.WriteLine(question);
            Clients.Caller.SendQuestion(String.IsNullOrWhiteSpace(question) ? "" : question);*/
        }

        public override Task OnConnected()
        {
            /*Clients.Caller.Log("Connected");
            Debug.WriteLine("Connected: " + Context.ConnectionId);
            Debug.WriteLine("Username is: " + Context.User.Identity.Name);
            Users.Add(Context.ConnectionId, new User() {ConnectionId = Context.ConnectionId, ip = GetIpAddress()});

            string ip = GetIpAddress();
            Clients.Caller.Log(ip);
            var channels = from channel in Channels.Values
                           where channel.Administrator.ip.Equals(ip)
                           select channel;

            List<string> channelIds = new List<string>();
            List<string> channelNames = new List<string>();
            foreach (Channel c in channels)
            {
                channelIds.Add(c.ChannelId);
                channelNames.Add(c.ChannelName);
            }
            string[] cIds = channelIds.ToArray();
            string[] cNames = channelNames.ToArray();
            Clients.Caller.IpDiscover(cIds, cNames);*/
            /*var name = Context.User.Identity.Name;
            using (var db = new HelpContext())
            {
                User user = db.Users
                    .Include(u => u.Connections)
                    .SingleOrDefault(u => u.UserName == name);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = name,
                        Connections = new List<Connection>()
                    };
                    db.Users.Add(user);
                }

                user.Connections.Add(new Connection
                {
                    ConnectionID = Context.ConnectionId
                });
                db.SaveChanges();
            }*/

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            /*if (Users.ContainsKey(Context.ConnectionId))
            {
                User u = Users[Context.ConnectionId];
                Debug.WriteLine("Disconnecting: " + Context.ConnectionId);
                var channelsToClear = from channel in Channels.Values
                    where channel.Administrator == u
                    select channel;
                var ctc = channelsToClear.ToList();
                for (int i = 0; i < ctc.Count(); i++)
                {
                    ExitChannel(ctc[i].ChannelId);
                }
                var channelsToLeave = from channel in Channels.Values
                    where channel.Users.ContainsKey(u.ConnectionId)
                    select channel;
                ctc = channelsToLeave.ToList();
                foreach (Channel channel in ctc)
                {
                    if (channel.UsersRequestingHelp.Contains(u))
                    {
                        RemoveQuestion(u, channel);
                    }
                    ExitChannel(channel.ChannelId);
                }
                if (Users.ContainsKey(Context.ConnectionId))
                {
                    Users.Remove(Context.ConnectionId);
                }
                Debug.WriteLine("Disconnected: " + Context.ConnectionId);
            }*/
            using (var db = new HelpContext())
            {

            }
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            Clients.Caller.ReloadPage();

            return base.OnReconnected();
        }


    }
}