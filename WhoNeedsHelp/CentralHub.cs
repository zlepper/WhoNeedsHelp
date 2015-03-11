using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using WhoNeedsHelp.server;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
// ReSharper disable MemberCanBePrivate.Global

namespace WhoNeedsHelp
{
    public class CentralHub : Hub<IClient>
    {

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

        public void LoadNearbyChannels()
        {
            using (var db = new HelpContext())
            {
                string ip = GetIpAddress();
                //var users = (from user in db.Users where user.Ip.Equals(ip) select user).ToList();
                //List<Channel> channels = users.SelectMany(user => user.ChannelsIn).ToList();
                List<Channel> channels = db.Users.Where(u => u.Ip.Equals(ip)).SelectMany(u => u.ChannelsIn).Distinct().ToList();
                int count = channels.Count;
                string[] channelIds = new string[count];
                string[] channelNames = new string[count];
                for (int i = 0; i < count; i++)
                {
                    channelIds[i] = channels[i].Id.ToString();
                    channelNames[i] = channels[i].ChannelName;
                }
                Clients.Caller.IpDiscover(channelIds, channelNames);
            }
        }

        public void RemoveChatMessage(string messageId)
        {
            if (String.IsNullOrWhiteSpace(messageId)) return;
            using (var db = new HelpContext())
            {
                var user = db.Users.Include(u => u.Channel).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user == null) return;
                Channel c = user.Channel;
                if (c == null) return;
                var message = db.ChatMessages.Find(Int32.Parse(messageId));
                if (message == null || (!c.IsUserAdministrator(user) && !message.User.Equals(user)))
                    return;
                IEnumerable<User> users = c.GetActiveUsers();
                foreach (User u in users)
                {
                    Clients.Client(u.ConnectionId).RemoveChatMessage(message.Id.ToString());
                }
            }

        }


        public void Chat(string message)
        {
            if (String.IsNullOrWhiteSpace(message)) return;
            using (var db = new HelpContext())
            {
                var user = db.Users.Include(u => u.Channel).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user == null) return;
                var channel = user.Channel;
                if (channel == null) return;
                ChatMessage chatMessage = channel.AddChatMessage(user, message);
                if (chatMessage != null)
                {
                    db.ChatMessages.Add(chatMessage);
                    db.SaveChanges();
                    var users = channel.GetActiveUsers();
                    foreach (User u in users)
                    {
                        Clients.Client(u.ConnectionId)
                            .SendChatMessage(chatMessage.Text, user.Name,
                                chatMessage.Id.ToString(), chatMessage.User.Equals(u), channel.AppendMessageToLast(chatMessage),
                                chatMessage.User.Equals(u) || channel.IsUserAdministrator(u));
                    }
                }

                db.SaveChanges();
            }
        }

        public void ChangeQuestion(string question)
        {
            using (var db = new HelpContext())
            {
                var user = db.Users.Include(u => u.Channel).Include(u => u.Questions).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user == null) return;
                Channel channel = user.Channel;
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

        public void ClearChat()
        {
            using (var db = new HelpContext())
            {
                var user = db.Users.Include(u => u.Channel).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user == null) return;
                var channel = user.Channel;
                if (channel == null) return;
                if (channel.IsUserAdministrator(user))
                {
                    db.ChatMessages.RemoveRange(channel.ChatMessages);
                    db.SaveChanges();
                    foreach (User u in channel.GetActiveUsers())
                    {
                        Clients.Client(u.ConnectionId).ClearChat();
                    }
                }
                else
                {
                    Clients.Client(user.ConnectionId).ClearChat();
                }
            }
        }

        private void UpdateCount(int channelId)
        {
            using (var db = new HelpContext())
            {
                var channel = db.Channels.Find(channelId);
                if (channel == null) return;
                int totalUsers = channel.Users.Count;
                int activeUsers = channel.UsersRequestingHelp.Count;
                foreach (User user in channel.GetActiveUsers())
                {
                    Clients.Client(user.ConnectionId).UpdateChannelCount(activeUsers, totalUsers, channelId.ToString());
                }
            }
        }

        public void RemoveQuestion(string questionId)
        {
            if (String.IsNullOrWhiteSpace(questionId))
            {
                using (var db = new HelpContext())
                {
                    var user = db.Users.Include(u => u.Channel).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                    if (user != null)
                    {
                        Channel channel = user.Channel;
                        Question question = user.GetQuestion(channel);
                        user.RemoveQuestion(question);
                        channel.RemoveUserRequestingHelp(user);
                        questionId = question.Id.ToString();
                        foreach (User u in channel.GetActiveUsers())
                        {
                            Clients.Client(u.ConnectionId).RemoveQuestion(questionId);
                        }
                        db.Questions.Remove(question);
                        db.SaveChanges();
                        UpdateCount(channel.Id);
                    }
                }
            }
            else
            {
                using (var db = new HelpContext())
                {
                    int qId = Int32.Parse(questionId);
                    Question q = db.Questions.Include(qu => qu.User).Include(qu => qu.Channel).SingleOrDefault(qu => qu.Id.Equals(qId));
                    if (q == null)  {
                        Clients.Caller.RemoveQuestion(questionId);
                        return;
                    }
                    User user = q.User;
                    Channel channel = q.Channel;
                    if (channel == null || user == null) return;
                    channel.RemoveUserRequestingHelp(user);
                    foreach (User use in channel.GetActiveUsers())
                    {
                        Clients.Client(use.ConnectionId).RemoveQuestion(questionId);
                    }
                    Clients.Client(user.ConnectionId).SetLayout(1);
                    db.Questions.Remove(q);
                    db.SaveChanges();
                    UpdateCount(channel.Id);
                }
            }
        }

        public void ChangeToChannel(string channelId)
        {
            using (var db = new HelpContext())
            {
                var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user != null)
                {
                    if (String.IsNullOrWhiteSpace(channelId)) return;
                    int id;
                    bool parse = Int32.TryParse(channelId, out id);
                    if (!parse) return;
                    var channel = db.Channels.Find(id);
                    if (channel != null)
                    {
                        user.Channel = channel;
                        db.Users.AddOrUpdate(user);
                        db.SaveChanges();
                        Clients.Caller.SetChannel(channelId, user.AreUserQuestioning(channel));
                        UpdateCount(channel.Id);
                        List<User> questionUsers = channel.GetUsersRequestingHelp();
                        List<string> usernames = new List<string>(), questions = new List<string>(), questionIds = new List<string>();
                        foreach (User us in questionUsers)
                        {
                            usernames.Add(us.Name);
                            Question question = us.GetQuestion(channel);
                            questions.Add(question.Text);
                            questionIds.Add(question.Id.ToString());
                        }
                        Clients.Caller.AddQuestions(usernames.ToArray(), questions.ToArray(), questionIds.ToArray(), channel.IsUserAdministrator(user));
                        var chatMessages = db.ChatMessages.Include(cm => cm.User).Where(cm => cm.Channel.Id.Equals(channel.Id));
                        List<string> textList = new List<string>();
                        List<string> authorList = new List<string>();
                        List<string> messageIdsList = new List<string>();
                        List<bool> senderList = new List<bool>();
                        List<bool> appendToLastList = new List<bool>();
                        List<bool> canEditList = new List<bool>();
                        foreach (ChatMessage chatMessage in chatMessages)
                        {
                            var chatMessageAuthor = chatMessage.User;
                            textList.Add(chatMessage.Text);
                            authorList.Add(chatMessageAuthor.Name);
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

        public void JoinChannel(string channelId)
        {
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

        public void SearchForChannel(string channelId)
        {
            if (String.IsNullOrWhiteSpace(channelId)) return;
            int id;
            bool isInt = Int32.TryParse(channelId, out id);
            if (!isInt) return;
            using (var db = new HelpContext())
            {
                var matches = db.Channels.Where(c => c.Id == id).ToList();
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

        public void ExitChannel(string channelId)
        {
            if(string.IsNullOrWhiteSpace(channelId)) return;
            int id;
            bool isInt = Int32.TryParse(channelId, out id);
            if (!isInt) return;
            using (var db = new HelpContext())
            {
                var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user != null)
                {

                    var channel = db.Channels.Find(id);
                    if (channel != null)
                    {
                        if (channel.IsUserAdministrator(user))
                        {
                            // Make everybody quit the channel
                            foreach (User u in channel.GetUsers())
                            {
                                Clients.Client(u.ConnectionId).ExitChannel(id.ToString());
                            }
                            db.Channels.Remove(channel);
                        }
                        else
                        {
                            channel.RemoveUser(user);
                            if (channel.GetUsersRequestingHelp().Contains(user))
                                channel.RemoveUserRequestingHelp(user);
                            Clients.Caller.ExitChannel(id.ToString());
                            int activeUsers = channel.GetQuestingUserCount();
                            int userCount = channel.GetUsers().Count;
                            foreach (User us in channel.GetUsers())
                            {
                                Clients.Client(us.ConnectionId).UpdateChannelCount(activeUsers, userCount, id.ToString());
                            }
                        }
                    }
                }
                db.SaveChanges();
            }


        }

        public void CreateNewChannel(string channelName)
        {
            using (var db = new HelpContext())
            {
                var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user != null)
                {
                    var channel = new Channel(user, channelName);
                    channel.AddUser(user);
                    user.Channel = channel;
                    db.Users.AddOrUpdate(user);
                    db.Channels.Add(channel);
                    db.SaveChanges();
                    Clients.Caller.AppendChannel(channelName, channel.Id.ToString());
                }
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void SetUsername(string name)
        {
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
                    user.Name = name;
                    db.SaveChanges();
                }
            }
        }

        public void RequestHelp(string question)
        {
            if (String.IsNullOrWhiteSpace(question))
                question = "";
            using (var db = new HelpContext())
            {
                var userFromDb = db.Users.Include(u => u.Channel).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (userFromDb == null)
                {
                    return;
                }
                Question q = userFromDb.RequestHelp(question);
                if (q == null) return;
                db.Questions.Add(q);
                db.SaveChanges();
                Channel channel = userFromDb.Channel;
                IEnumerable<User> usersInChannel = channel.GetActiveUsers();
                foreach (User user in usersInChannel)
                {
                    Clients.Client(user.ConnectionId).AddQuestion(userFromDb.Name, q.Text, q.Id.ToString(), channel.IsUserAdministrator(user));
                }
                UpdateCount(channel.Id);
            }

        }

        public void GetData(int action)
        {
            switch (action)
            {
                case 2:
                    // Request a version number from the server
                    Clients.Caller.CheckVersion(1);
                    break;
            }
        }

        public override Task OnConnected()
        {
            using (var db = new HelpContext())
            {
                var user =
                    db.Users.SingleOrDefault(u => u.ConnectionId == Context.ConnectionId);
                if (user == null)
                {
                    user = new User
                    {
                        ConnectionId = Context.ConnectionId,
                        Ip = GetIpAddress()
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                }
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            using (var db = new HelpContext())
            {
                var user = db.Users.Include(u => u.ChannelsIn).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user == null) return base.OnDisconnected(stopCalled);
                foreach (Channel c in user.ChannelsIn)
                {
                    ExitChannel(c.Id.ToString());
                }
                RemoveUser(user.Id);
            }
            return base.OnDisconnected(stopCalled);
        }

        private void RemoveUser(int id)
        {
            using (var db = new HelpContext())
            {
                var user = db.Users.Find(id);
                if (user == null) return;
                db.Users.Remove(user);
                db.SaveChanges();
            }
        }

        public override Task OnReconnected()
        {
            
            ReloadClientChannels();


            return base.OnReconnected();
        }

        private void ReloadClientChannels()
        {
            // Resend channels to client
            using (var db = new HelpContext())
            {
                var user = db.Users.Include(u => u.ChannelsIn).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user == null) return;
                List<string> channelNames = new List<string>();
                List<string> channelIds = new List<string>();
                foreach (Channel c in user.ChannelsIn)
                {
                    channelNames.Add(c.ChannelName);
                    channelIds.Add(c.Id.ToString());
                }
                Clients.Caller.ShowChannels(channelIds.ToArray(), channelNames.ToArray());
            }
        }

        public void RequestActiveChannel()
        {
            using (var db = new HelpContext())
            {
                var user =
                    db.Users.Include(u => u.Channel).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                if (user == null) return;
                Clients.Caller.SetChannel(user.Channel.Id.ToString(), user.AreUserQuestioning(user.Channel));
            }
        }

    }
}