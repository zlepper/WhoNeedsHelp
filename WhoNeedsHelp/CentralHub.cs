using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using WhoNeedsHelp.server;
using WhoNeedsHelp.Simples;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable SuggestVarOrType_SimpleTypes

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

        public void LoadNearbyChannels()
        {
            using (var db = new HelpContext())
            {
                string ip = GetIpAddress();
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
                //var user = db.Users.Include(u => u.Channel).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var user = con.User;
                if (user == null) return;
                Channel c = user.Channel;
                if (c == null) return;
                var message = db.ChatMessages.Find(Int32.Parse(messageId));
                if (message == null || (!c.IsUserAdministrator(user) && !message.User.Equals(user)))
                    return;
                foreach (Connection connection in c.ActiveUsers.SelectMany(u => u.Connections))
                {
                    Clients.Client(connection.ConnectionId).RemoveChatMessage(message.Id.ToString());
                }
            }

        }


        public void Chat(string message)
        {
            if (String.IsNullOrWhiteSpace(message)) return;
            using (var db = new HelpContext())
            {
                //var user = db.Users.Include(u => u.Channel).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var user = con.User;
                if (user == null) return;
                var channel = user.Channel;
                if (channel == null)
                {
                    Clients.Caller.ErrorChat("Du er ikke i nogen kanal");
                    return;
                }
                if (message.StartsWith("/"))
                {
                    string[] parts = message.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length <= 0)
                    {
                        Clients.Caller.Alert("Du har ikke indtastet nogen kommando.", "Ingen kommando", "error");
                        return;
                    }
                    switch (parts[0])
                    {
                        case "/op":
                            PromoteToAdmin(parts);
                            break;
                        default:
                            Clients.Caller.Alert("Ukendt kommando", "Ukendt kommando", "error");
                            return;
                    }
                }
                else
                {
                    ChatMessage chatMessage = channel.AddChatMessage(user, message);
                    if (chatMessage != null)
                    {
                        db.ChatMessages.Add(chatMessage);
                        db.SaveChanges();
                        foreach (User u in channel.ActiveUsers)
                        {
                            foreach (Connection connection in u.Connections)
                            {
                                Clients.Client(connection.ConnectionId)
                                    .SendChatMessage(chatMessage.Text, user.Name, chatMessage.Id.ToString(),
                                        chatMessage.User.Equals(u), channel.AppendMessageToLast(chatMessage),
                                        chatMessage.User.Equals(u) || channel.IsUserAdministrator(u));
                            }
                        }
                    }
                }

                db.SaveChanges();
            }
        }

        private void PromoteToAdmin(string[] parts)
        {
            using (var helpcontext = new HelpContext())
            {
                var callingUserConnection =
                    helpcontext.Connections.SingleOrDefault(c => c.ConnectionId.Equals(Context.ConnectionId));
                if (callingUserConnection == null || callingUserConnection.User == null) return;
                var callingUser = callingUserConnection.User;
                if (!callingUser.Channel.IsUserAdministrator(callingUser))
                {
                    Clients.Caller.Alert("Du skal være administrator i kanalen for at kunne bruge denne command.", "Manglende rettigheder", "error");
                    return;
                }
                if (parts.Length < 2)
                {
                    Clients.Caller.Alert("Du har ikke specificeret nogen personer at promote til administrator.",
                        "Ingen personer specificeret", "error");
                    return;
                }
                foreach (string s in parts)
                {
                    if (s.Equals("/op", StringComparison.OrdinalIgnoreCase)) continue;
                    if (s.Contains("@"))
                    {
                        var mail = s;
                        var user = helpcontext.Users.First(u => u.EmailAddress.Equals(mail, StringComparison.OrdinalIgnoreCase));
                        if (user == null)
                        {
                            Clients.Caller.Alert("Ingen bruger fundet med email \"" + mail + "\"", "Bruger ikke fundet",
                                "error");
                            continue;
                        }
                        if (!user.ChannelsIn.Contains(callingUser.Channel))
                        {
                            Clients.Caller.Alert("Brugeren med \"" + mail + "\" er ikke i denne kanal.",
                                "Bruger ikke i kanal", "error");
                            continue;
                        }
                        callingUser.Channel.AddAdministrator(user);
                        helpcontext.SaveChanges();
                        foreach (Connection con in user.Connections)
                        {
                            Clients.Client(con.ConnectionId)
                                .Alert(
                                    "Du er blevet gjort til administrator i kanalen \"" +
                                    callingUser.Channel.ChannelName + "\".", "Administrator", "info");
                        }
                        Clients.Caller.Alert(user.Name + " er nu blevet gjort til administrator.", "Admin", "success");
                        ReloadChannelData(user.Id);
                    }
                    else
                    {
                        Clients.Caller.Alert(s + " Er ikke en emailaddresse", "Forkert format", "error");
                    }
                }
            }
        }

        private void ReloadChannelData(int userID)
        {
            using (var db = new HelpContext())
            {
                var user = db.Users.Find(userID);
                var channel = user.Channel;
                if (user == null) return;
                List<Question> questions = db.Questions.Where(q => q.Channel.Id == channel.Id).OrderBy(q => q.AskedTime).ToList();
                List<string> usernames = new List<string>(), questiontexts = new List<string>(), questionIds = new List<string>();
                foreach (Question ques in questions)
                {
                    usernames.Add(ques.User.Name);
                    questiontexts.Add(ques.Text);
                    questionIds.Add(ques.Id.ToString());
                }
                var usernamesArray = usernames.ToArray();
                var questionsArray = questiontexts.ToArray();
                var questionidsArray = questionIds.ToArray();
                bool admin = channel.IsUserAdministrator(user);
                foreach (Connection connection in user.Connections)
                {
                    //Clients.Client(connection.ConnectionId).AddQuestions(usernamesArray, questionsArray, questionidsArray, admin);
                }
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
                foreach (Connection connection in user.Connections)
                {
                    Clients.Client(connection.ConnectionId)
                        .SendChatMessages(textList.ToArray(), authorList.ToArray(), messageIdsList.ToArray(),
                            senderList.ToArray(), appendToLastList.ToArray(), canEditList.ToArray());

                }
                var users = channel.Users;
                var userNames = users.Select(u => u.Name);
                var ids = users.Select(u => u.Id);
                bool a = channel.IsUserAdministrator(user);
                foreach (Connection connection in user.Connections)
                {
                    Clients.Client(connection.ConnectionId).AppendUsers(userNames.ToArray(), ids.ToArray(), a);
                }
            }
        }

        public void ChangeQuestion(string question)
        {
            using (var db = new HelpContext())
            {
                //var user = db.Users.Include(u => u.Channel).Include(u => u.Questions).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var user = con.User;
                if (user == null) return;
                Channel channel = user.Channel;
                if (channel == null) return;
                user.UpdateQuestion(channel, question);
                Question q = user.GetQuestion(channel);
                if (q == null) return;
                foreach (Connection connection in channel.ActiveUsers.SelectMany(use => use.Connections))
                {
                    Clients.Client(connection.ConnectionId).UpdateQuestion(String.IsNullOrWhiteSpace(q.Text) ? "" : question, q.Id);
                }
                db.SaveChanges();
            }
        }

        public void ClearChat()
        {
            using (var db = new HelpContext())
            {
                //var user = db.Users.Include(u => u.Channel).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var user = con.User;
                if (user == null) return;
                var channel = user.Channel;
                if (channel == null) return;
                if (channel.IsUserAdministrator(user))
                {
                    db.ChatMessages.RemoveRange(channel.ChatMessages);
                    db.SaveChanges();
                    foreach (Connection connection in channel.ActiveUsers.SelectMany(u => u.Connections))
                    {
                        Clients.Client(connection.ConnectionId).ClearChat();
                    }
                }
                else
                {
                    //Clients.Client(user.ConnectionId).ClearChat();
                    foreach (Connection connection in user.Connections)
                    {
                        Clients.Client(connection.ConnectionId).ClearChat();
                    }
                }
            }
        }

        public void RemoveQuestion(int questionId)
        {
            if (questionId == 0)
            {
                using (var db = new HelpContext())
                {
                    //var user = db.Users.Include(u => u.Channel).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                    var con = db.Connections.Find(Context.ConnectionId);
                    if (con == null) return;
                    var user = con.User;
                    if (user == null) return;
                    Question question = db.Questions.Find(questionId);
                    Channel channel = question.Channel;
                    user.RemoveQuestion(question);
                    foreach (Connection connection in user.Connections)
                    {
                        Clients.Client(connection.ConnectionId).SetQuestionState(false, channel.Id);
                    }
                    channel.RemoveUserRequestingHelp(user);
                    foreach (Connection connection in channel.Users.SelectMany(u => u.Connections))
                    {
                        Clients.Client(connection.ConnectionId).RemoveQuestion(question.Id);
                    }
                    db.Questions.Remove(question);
                    db.SaveChanges();
                }
            }
            else
            {
                using (var db = new HelpContext())
                {
                    Question q = db.Questions.Include(qu => qu.User).Include(qu => qu.Channel).SingleOrDefault(qu => qu.Id.Equals(questionId));
                    if (q == null)
                    {
                        Clients.Caller.RemoveQuestion(questionId);
                        return;
                    }
                    User user = q.User;
                    Channel channel = q.Channel;
                    if (channel == null || user == null) return;
                    channel.RemoveUserRequestingHelp(user);
                    foreach (Connection connection in channel.Users.SelectMany(use => use.Connections))
                    {
                        Clients.Client(connection.ConnectionId).RemoveQuestion(questionId);
                    }
                    //Clients.Client(user.ConnectionId).SetLayout(1);
                    foreach (Connection connection in user.Connections)
                    {
                        Clients.Client(connection.ConnectionId).SetQuestionState(false, channel.Id);
                    }
                    db.Questions.Remove(q);
                    db.SaveChanges();
                }
            }
        }

        public void ChangeToChannel(string channelId)
        {
            using (var db = new HelpContext())
            {
                //var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var user = con.User;
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
                        bool areUserQuestioning = user.AreUserQuestioning(channel);
                        foreach (Connection connection in user.Connections)
                        {
                            Clients.Client(connection.ConnectionId).SetChannel(id, areUserQuestioning);
                        }
                        ReloadChannelData(user.Id);
                    }
                }
                db.SaveChanges();
            }

        }

        public void JoinChannel(string channelId)
        {
            int id;
            bool parsed = Int32.TryParse(channelId, out id);
            if (!parsed) return;
            using (var db = new HelpContext())
            {
                var channel = db.Channels.Find(id);
                if (channel == null)
                {
                    Clients.Caller.Alert("Kanalen med id \"" + id + "\" blev ikke fundet.", "Kanal ikke fundet", "info");
                    return;
                }
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var user = con.User;
                if (user == null) return;
                if (channel.GetUsers().Contains(user)) return;
                channel.AddUser(user);
                foreach (Connection connection in user.Connections)
                {
                    //Clients.Client(connection.ConnectionId).AppendChannel(channel.ChannelName, id);
                }
                foreach (User us in channel.ActiveUsers)
                {
                    bool admin = channel.IsUserAdministrator(us);
                    foreach (Connection connection in us.Connections)
                    {
                        Clients.Client(connection.ConnectionId).AppendUser(user.Name, user.Id, admin);
                    }
                }
                db.SaveChanges();
                var users = channel.Users;
                var userNames = users.Select(u => u.Name);
                var ids = users.Select(u => u.Id);
                bool ad = channel.IsUserAdministrator(user);
                foreach (Connection connection in user.Connections)
                {
                    Clients.Client(connection.ConnectionId).AppendUsers(userNames.ToArray(), ids.ToArray(), ad);
                }
            }
        }

        public void ExitChannel(int channelId)
        {
            Debug.WriteLine(channelId);
            if (channelId == 0) return;
            using (var db = new HelpContext())
            {
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var user = con.User;
                if (user != null)
                {
                    var channel = db.Channels.Find(channelId);
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
                            var question = user.GetQuestion(channel);
                            if (channel.GetUsersRequestingHelp().Contains(user))
                            {
                                channel.RemoveUserRequestingHelp(user);
                            }
                            if (user.Channel != null) user.Channel = null;
                            if (question != null)
                                foreach (Connection connec in channel.Users.SelectMany(us => us.Connections))
                                {
                                    Clients.Client(connec.ConnectionId).RemoveQuestion(question.Id);
                                }
                            foreach (Connection connection in user.Connections)
                            {
                                Clients.Client(connection.ConnectionId).ExitChannel(channelId);
                            }
                            foreach (var connection in channel.Users.SelectMany(us => us.Connections))
                            {
                                Clients.Client(connection.ConnectionId).RemoveUser(user.Id);
                            }
                        }
                    }
                    else
                    {
                        Clients.Caller.Alert("Channel not found", "Missing", "error");
                    }
                }
                db.SaveChanges();
            }


        }

        public void CreateNewChannel(string channelName)
        {
            using (var db = new HelpContext())
            {
                //var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var user = con.User;
                if (user == null) return;
                if (user.AreAdministratorIn.Count > 4)
                {
                    Clients.Caller.Alert("Du kan ikke oprette mere end 5 kanaler af gangen", "Maks oprettede kanaler nået", "error");
                    return;
                }
                var channel = new Channel(user, channelName);
                channel.AddUser(user);
                user.Channel = channel;
                db.Users.AddOrUpdate(user);
                db.Channels.Add(channel);
                db.SaveChanges();
                var sc = channel.ToSimpleChannel();
                sc.HasAdmin = true;
                foreach (Connection connection in user.Connections)
                {
                    Clients.Client(connection.ConnectionId).AppendChannel(sc);
                }
            }
        }

        public void SetUsername(string name)
        {
            using (var db = new HelpContext())
            {
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var user = con.User;
                if (user == null)
                {
                    user = new User
                    {
                        Name = name,
                        //Connections = new List<Connection>(),
                        //ConnectionId = Context.ConnectionId
                    };
                    user.Connections.Add(new Connection() { ConnectionId = Context.ConnectionId });
                    db.Users.Add(user);
                }
                else
                {
                    user.Name = name;
                    //Clients.Caller.UpdateUsername(name);
                    foreach (Connection connection in user.Connections)
                    {
                        Clients.Client(connection.ConnectionId).UpdateUsername(name);
                    }

                    foreach (var question in user.Questions)
                    {
                        foreach (Connection connection in question.Channel.ActiveUsers.SelectMany(u => u.Connections))
                        {
                            Clients.Client(connection.ConnectionId).UpdateQuestionAuthorName(user.Name, question.Id.ToString());
                        }
                    }

                    var chatMessages = user.ChatMessages.ToList();
                    foreach (Channel channel in chatMessages.Select(cm => cm.Channel).Distinct())
                    {
                        var updatingChatMessages = chatMessages.Where(cm => cm.Channel.Equals(channel));
                        var ids = updatingChatMessages.Select(chatMessage => chatMessage.Id.ToString()).ToList();
                        foreach (Connection connection in channel.ActiveUsers.SelectMany(u => u.Connections))
                        {
                            Clients.Client(connection.ConnectionId).UpdateChatMessageAuthorName(name, ids.ToArray());
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        public void RequestHelp(string question, int channelid)
        {
            if (String.IsNullOrWhiteSpace(question)) question = "";
            using (var db = new HelpContext())
            {
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var userFromDb = con.User;
                if (userFromDb == null)
                {
                    return;
                }
                Question q = userFromDb.RequestHelp(channelid, question);
                if (q == null) return;
                db.Questions.Add(q);
                db.SaveChanges();
                Channel channel = userFromDb.Channel;
                SimpleUser su = new SimpleUser(userFromDb.Id, userFromDb.Name);
                SimpleQuestion sq = new SimpleQuestion(q.Id, q.Text, su);
                foreach (Connection connection in channel.Users.SelectMany(user => user.Connections))
                {
                    Clients.Client(connection.ConnectionId).AddQuestion(sq, channel.Id);
                }
                //foreach (Connection connection in userFromDb.Connections)
                //{
                //    Clients.Client(connection.ConnectionId).SetLayout(3);
                //}
            }

        }

        public void RemoveUserFromChannel(string id)
        {
            if (String.IsNullOrEmpty(id)) return;
            int Id;
            bool parse = Int32.TryParse(id, out Id);
            if (!parse) return;
            using (var db = new HelpContext())
            {
                var user = db.Users.Find(Id);
                if (user == null)
                {
                    Clients.Caller.RemoveUser(Id);
                    return;
                }
                var con = db.Connections.SingleOrDefault(c => c.ConnectionId.Equals(Context.ConnectionId));
                if (con == null) return;
                var callingUser = con.User;
                if (callingUser == null) return;
                if (callingUser.Id == user.Id)
                {
                    Clients.Caller.Alert("Du har lige forsøgt at smide dig selv ud af kanalen...", "Really?!", "warning");
                    return;
                }
                if (callingUser.Channel.IsUserAdministrator(callingUser))
                {
                    ExitChannel(callingUser.Channel, user);
                }
            }
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
            using (var db = new HelpContext())
            {
                //var user =
                //    db.Users.SingleOrDefault(u => u.ConnectionId == Context.ConnectionId);

                var con = db.Connections.Find(Context.ConnectionId);
                if (con != null) return base.OnConnected();
                var user = new User
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
            using (var db = new HelpContext())
            {
                //var user = db.Users.Include(u => u.ChannelsIn).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return base.OnDisconnected(stopCalled);
                var user = con.User;
                if (user == null) return base.OnDisconnected(stopCalled);
                if (String.IsNullOrWhiteSpace(user.EmailAddress))
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

        private void RemoveUser(int id)
        {
            using (var db = new HelpContext())
            {
                var user = db.Users.Find(id);
                if (user == null) return;
                foreach (Channel c in user.ChannelsIn)
                {
                    ExitChannel(c, user);
                }
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
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var user = con.User;
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
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var user = con.User;
                if (user == null || user.Channel == null) return;
                bool question = user.AreUserQuestioning(user.Channel);
                string id = user.Channel.Id.ToString();
                foreach (Connection connection in user.Connections)
                {
                    Clients.Client(connection.ConnectionId).SetChannel(user.Channel.Id, question);
                }
            }
        }

        public void CreateNewUser(string username, string email, string pw)
        {
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(email) || String.IsNullOrWhiteSpace(pw))
            {
                Clients.Caller.UserCreationFailed("En af værdier er ikke blevet sat.");
                return;
            }
            using (var db = new HelpContext())
            {
                var user = db.Users.SingleOrDefault(u => u.EmailAddress.Equals(email));
                if (user != null)
                {
                    Clients.Caller.UserCreationFailed("Emailadressen er allerede i brug.");
                    return;
                }
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                user = con.User;
                if (user == null) return;
                if (!String.IsNullOrWhiteSpace(user.Pw) || !String.IsNullOrWhiteSpace(user.EmailAddress))
                {
                    Clients.Caller.UserCreationFailed("Du er allerede logget ind.");
                    return;
                }
                var pass = PasswordHash.CreateHash(pw);
                user.Pw = pass;
                user.EmailAddress = email;
                db.SaveChanges();
                Clients.Caller.UserCreationSuccess();
            }
        }

        public void LoginUser(string email, string password)
        {
            if (String.IsNullOrWhiteSpace(email) || String.IsNullOrWhiteSpace(password))
            {
                Clients.Caller.LoginFailed();
                return;
            }
            using (var db = new HelpContext())
            {
                //var currentUser = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var currentUser = con.User;
                if (currentUser == null || !String.IsNullOrWhiteSpace(currentUser.EmailAddress) || !String.IsNullOrWhiteSpace(currentUser.Pw))
                {
                    Clients.Caller.LoginFailed();
                    return;
                }
                var user = db.Users.SingleOrDefault(u => u.EmailAddress.Equals(email));
                if (user == null)
                {
                    Clients.Caller.LoginFailed();
                    return;
                }
                bool success = PasswordHash.ValidatePassword(password, user.Pw);
                if (success)
                {
                    //user.ConnectionId = Context.ConnectionId;
                    List<Channel> oldChannels =
                        currentUser.ChannelsIn.Where(channel => !user.ChannelsIn.Contains(channel)).ToList();
                    foreach (Channel channel in oldChannels)
                    {
                        user.ChannelsIn.Add(channel);
                        foreach (Connection conn in user.Connections)
                        {
                            //Clients.Client(conn.ConnectionId).AppendChannel2(channel.ChannelName, channel.Id);
                        }
                    }
                    Connection connection =
                        db.Connections.SingleOrDefault(conn => conn.ConnectionId.Equals(Context.ConnectionId));
                    currentUser.Connections.Remove(connection);
                    user.Connections.Add(connection);
                    foreach (Question question in currentUser.Questions.Where(question => user.AreUserQuestioning(question.Channel)))
                    {
                        RemoveQuestion(question.Id);
                    }

                    foreach (Channel channel in currentUser.ChannelsRequestingHelpIn.Where(channel => !user.ChannelsRequestingHelpIn.Contains(channel)))
                    {
                        user.ChannelsRequestingHelpIn.Add(channel);
                    }
                    foreach (Channel c in oldChannels)
                    {
                        ExitChannel(c, currentUser);
                    }

                    List<string> channelNames = new List<string>();
                    List<string> channelIds = new List<string>();
                    foreach (Channel c in user.ChannelsIn)
                    {
                        channelNames.Add(c.ChannelName);
                        channelIds.Add(c.Id.ToString());
                    }
                    Clients.Caller.ShowChannels(channelIds.ToArray(), channelNames.ToArray());
                    currentUser = db.Users.Find(currentUser.Id);
                    db.Users.Remove(currentUser);

                    db.SaveChanges();
                    Clients.Caller.LoginSuccess();
                    if (user.Channel != null)
                    {
                        List<Question> questions = user.Channel.Questions.ToList();
                        /*String[] usernames = new string[questions.Count];
                        String[] questionText = new string[questions.Count];
                        String[] questionIds = new string[questions.Count];
                        for (int i = 0; i < questions.Count; i++)
                        {
                            usernames[i] = questions[i].User.Name;
                            questionText[i] = questions[i].Text;
                            questionIds[i] = questions[i].Id.ToString();
                        }
                        bool admin = user.Channel.IsUserAdministrator(user);*/
                        //Clients.Caller.AddQuestions(usernames, questionText, questionIds, admin);
                        List<ChatMessage> chatMessages = user.Channel.ChatMessages.ToList();
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
                            appendToLastList.Add(false);
                            canEditList.Add(chatMessage.User.Equals(user) || user.Channel.IsUserAdministrator(user));
                        }
                        Clients.Caller.SendChatMessages(textList.ToArray(), authorList.ToArray(), messageIdsList.ToArray(), senderList.ToArray(), appendToLastList.ToArray(), canEditList.ToArray());
                    }
                    if (String.IsNullOrWhiteSpace(currentUser.Name) || !currentUser.Name.Equals(user.Name))
                    {
                        Clients.Caller.UpdateUsername(user.Name);
                    }
                }
                else
                {
                    Clients.Caller.LoginFailed();
                }
            }
        }

        private void ExitChannel(Channel channel, User user)
        {
            using (var db = new HelpContext())
            {
                user = db.Users.Find(user.Id);
                if (user == null) return;
                channel = db.Channels.Find(channel.Id);
                if (channel == null) return;
                foreach (Connection con in channel.ActiveUsers.SelectMany(u => u.Connections))
                {
                    Clients.Client(con.ConnectionId).RemoveUser(user.Id);
                }
                foreach (Connection con in user.Connections)
                {
                    Clients.Client(con.ConnectionId).ExitChannel(channel.Id);
                }
                channel.RemoveUser(user);
                db.SaveChanges();
            }
        }

        public void LogoutUser()
        {
            using (var db = new HelpContext())
            {
                //var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                var con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                var user = con.User;
                if (user == null)
                {
                    Clients.Caller.UserLoggedOut();
                    return;
                }
                //user.ConnectionId = null;
                var connectionToRemove = user.Connections.SingleOrDefault(c => c.ConnectionId.Equals(Context.ConnectionId));
                if (connectionToRemove != null)
                {
                    user.Connections.Remove(connectionToRemove);
                    db.Connections.Remove(connectionToRemove);
                }
                var newUser = new User()
                {
                    Name = user.Name,
                    Ip = user.Ip
                };
                newUser.Connections.Add(new Connection() { ConnectionId = Context.ConnectionId });
                db.Users.Add(newUser);
                db.SaveChanges();
                Clients.Caller.UserLoggedOut();
            }
        }
    }
}