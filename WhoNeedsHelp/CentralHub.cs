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

            string ip = temp != null ? temp as string : "";

            return ip;
        }

        public void LoadNearbyChannels()
        {
            using (HelpContext db = new HelpContext())
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

        public void RemoveChatMessage(int messageId)
        {
            if (messageId == 0) return;
            using (HelpContext db = new HelpContext())
            {
                User user = db.Connections.Find(Context.ConnectionId).User;
                if (user == null) return;
                ChatMessage message = db.ChatMessages.Find(messageId);
                if (message == null)
                {
                    return;
                }
                Channel c = message.Channel;
                if (c == null) return;
                if (!c.IsUserAdministrator(user) && !message.User.Equals(user))
                    return;
                Clients.Clients(c.Users.SelectMany(u => u.Connections).Select(co => co.ConnectionId).ToList()).RemoveChatMessage(message.Id);
                db.ChatMessages.Remove(message);
                db.SaveChanges();
            }

        }


        public void Chat(string message, int channelId)
        {
            if (String.IsNullOrWhiteSpace(message)) return;
            using (HelpContext db = new HelpContext())
            {
                User user = db.Connections.Find(Context.ConnectionId).User;
                if (user == null) return;
                Channel channel = db.Channels.Find(channelId);
                if (channel == null)
                {
                    Clients.Caller.Alert("Something went odd", "ehh", "error");
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
                        SimpleChatMessage scm = chatMessage.ToSimpleChatMessage();
                        Clients.Clients(channel.Users.SelectMany(u => u.Connections).Select(c => c.ConnectionId).ToList()).SendChatMessage(scm, channelId);
                    }
                }

                db.SaveChanges();
            }
        }

        private void PromoteToAdmin(string[] parts)
        {
            using (HelpContext helpcontext = new HelpContext())
            {
                Connection callingUserConnection =
                    helpcontext.Connections.SingleOrDefault(c => c.ConnectionId.Equals(Context.ConnectionId));
                if (callingUserConnection == null || callingUserConnection.User == null) return;
                User callingUser = callingUserConnection.User;
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
                        string mail = s;
                        User user = helpcontext.Users.First(u => u.EmailAddress.Equals(mail, StringComparison.OrdinalIgnoreCase));
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
                            Clients.Client(con.ConnectionId).SetAdminState(callingUser.Channel.Id, true);
                        }
                        Clients.Caller.Alert(user.Name + " er nu blevet gjort til administrator.", "Admin", "success");
                        
                    }
                    else
                    {
                        Clients.Caller.Alert(s + " Er ikke en emailaddresse", "Forkert format", "error");
                    }
                }
            }
        }

        /*private void ReloadChannelData(int userID)
        {
            using (HelpContext db = new HelpContext())
            {
                User user = db.Users.Find(userID);
                Channel channel = user.Channel;
                if (user == null) return;
                List<Question> questions = db.Questions.Where(q => q.Channel.Id == channel.Id).OrderBy(q => q.AskedTime).ToList();
                List<string> usernames = new List<string>(), questiontexts = new List<string>(), questionIds = new List<string>();
                foreach (Question ques in questions)
                {
                    usernames.Add(ques.User.Name);
                    questiontexts.Add(ques.Text);
                    questionIds.Add(ques.Id.ToString());
                }
                string[] usernamesArray = usernames.ToArray();
                string[] questionsArray = questiontexts.ToArray();
                string[] questionidsArray = questionIds.ToArray();
                bool admin = channel.IsUserAdministrator(user);
                foreach (Connection connection in user.Connections)
                {
                    //Clients.Client(connection.ConnectionId).AddQuestions(usernamesArray, questionsArray, questionidsArray, admin);
                }
                IQueryable<ChatMessage> chatMessages = db.ChatMessages.Include(cm => cm.User).Where(cm => cm.Channel.Id.Equals(channel.Id));
                List<string> textList = new List<string>();
                List<string> authorList = new List<string>();
                List<string> messageIdsList = new List<string>();
                List<bool> senderList = new List<bool>();
                List<bool> appendToLastList = new List<bool>();
                List<bool> canEditList = new List<bool>();
                foreach (ChatMessage chatMessage in chatMessages)
                {
                    User chatMessageAuthor = chatMessage.User;
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
                ICollection<User> users = channel.Users;
                IEnumerable<string> userNames = users.Select(u => u.Name);
                IEnumerable<int> ids = users.Select(u => u.Id);
                bool a = channel.IsUserAdministrator(user);
                foreach (Connection connection in user.Connections)
                {
                    Clients.Client(connection.ConnectionId).AppendUsers(userNames.ToArray(), ids.ToArray(), a);
                }
            }
        }*/

        public void ChangeQuestion(string question, int channelId)
        {
            using (HelpContext db = new HelpContext())
            {
                //var user = db.Users.Include(u => u.Channel).Include(u => u.Questions).SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                User user = db.Connections.Find(Context.ConnectionId).User;
                if (user == null) return;
                Channel channel = db.Channels.Find(channelId);
                if (channel == null) return;
                Question q = user.UpdateQuestion(channel, question);
                if (q == null) return;
                foreach (Connection connection in channel.Users.SelectMany(use => use.Connections))
                {
                    Clients.Client(connection.ConnectionId).UpdateQuestion(String.IsNullOrWhiteSpace(q.Text) ? "" : question, q.Id, channelId);
                }
                db.SaveChanges();
            }
        }

        public void EditOwnQuestion(int channelId)
        {
            using (HelpContext db = new HelpContext())
            {
                User user = db.Connections.Find(Context.ConnectionId).User;
                Channel channel = user.GetChannel(channelId);
                if (channel == null) return;
                Question question = user.GetQuestion(channel);
                if (question == null)
                {
                    Clients.Caller.SetQuestionState(false, channelId);
                    return;
                }
                Clients.Caller.SendQuestion(question.Text);
            }
        }

        public void ClearChat(int channelId)
        {
            using (HelpContext db = new HelpContext())
            {
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                User user = con.User;
                if (user == null) return;
                Channel channel = user.Channel;
                if (channel == null) return;
                if (channel.IsUserAdministrator(user))
                {
                    db.ChatMessages.RemoveRange(channel.ChatMessages);
                    db.SaveChanges();
                    foreach (Connection connection in channel.Users.SelectMany(u => u.Connections))
                    {
                        Clients.Client(connection.ConnectionId).ClearChat(channelId);
                    }
                }
                else
                {
                    foreach (Connection connection in user.Connections)
                    {
                        Clients.Client(connection.ConnectionId).ClearChat(channelId);
                    }
                }
            }
        }

        public void RemoveOwnQuestion(int channelId)
        {
            using (HelpContext db = new HelpContext())
            {
                Channel channel = db.Channels.Find(channelId);
                if (channel == null) return;
                User user = db.Connections.Find(Context.ConnectionId).User;
                if (user == null) return;
                Question question = user.GetQuestion(channel);
                if (user.AreUserQuestioning(channel))
                {
                    channel.RemoveUserRequestingHelp(user);
                    foreach (Connection connection in channel.Users.SelectMany(u => u.Connections))
                    {
                        Clients.Client(connection.ConnectionId).RemoveQuestion(question.Id);
                    }
                    foreach (Connection connection in user.Connections)
                    {
                        Clients.Client(connection.ConnectionId).SetQuestionState(false, channel.Id);
                    }
                    db.Questions.Remove(question);
                    db.SaveChanges();
                }
                else
                {
                    Clients.Caller.SetQuestionState(false, channelId);
                }
            }
        }

        public void RemoveQuestion(int questionId)
        {
            if (questionId == 0)
            {
                return;
            }
            using (HelpContext db = new HelpContext())
            {
                Question q = db.Questions.Include(qu => qu.User).Include(qu => qu.Channel).SingleOrDefault(qu => qu.Id.Equals(questionId));
                if (q == null)
                {
                    Clients.Caller.RemoveQuestion(questionId);
                    return;
                }
                User callingUser = db.Connections.Find(Context.ConnectionId).User;
                User user = q.User;
                Channel channel = q.Channel;
                if (channel == null || user == null || !channel.IsUserAdministrator(callingUser))
                {
                    return;
                }
                channel.RemoveUserRequestingHelp(user);
                foreach (Connection connection in channel.Users.SelectMany(use => use.Connections))
                {
                    Clients.Client(connection.ConnectionId).RemoveQuestion(questionId);
                }
                foreach (Connection connection in user.Connections)
                {
                    Clients.Client(connection.ConnectionId).SetQuestionState(false, channel.Id);
                }
                db.Questions.Remove(q);
                db.SaveChanges();
            }
        }

        //public void ChangeToChannel(string channelId)
        //{
        //    using (HelpContext db = new HelpContext())
        //    {
        //        //var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
        //        Connection con = db.Connections.Find(Context.ConnectionId);
        //        if (con == null) return;
        //        User user = con.User;
        //        if (user != null)
        //        {
        //            if (String.IsNullOrWhiteSpace(channelId)) return;
        //            int id;
        //            bool parse = Int32.TryParse(channelId, out id);
        //            if (!parse) return;
        //            Channel channel = db.Channels.Find(id);
        //            if (channel != null)
        //            {
        //                user.Channel = channel;
        //                db.Users.AddOrUpdate(user);
        //                db.SaveChanges();
        //                bool areUserQuestioning = user.AreUserQuestioning(channel);
        //                foreach (Connection connection in user.Connections)
        //                {
        //                    Clients.Client(connection.ConnectionId).SetChannel(id, areUserQuestioning);
        //                }
        //                ReloadChannelData(user.Id);
        //            }
        //        }
        //        db.SaveChanges();
        //    }

        //}

        public void JoinChannel(int channelId)
        {
            using (HelpContext db = new HelpContext())
            {
                Channel channel = db.Channels.Find(channelId);
                if (channel == null)
                {
                    Clients.Caller.Alert("Kanalen med id \"" + channelId + "\" blev ikke fundet.", "Kanal ikke fundet", "info");
                    return;
                }
                User user = db.Connections.Find(Context.ConnectionId).User;
                if (user == null) return;
                if (channel.GetUsers().Contains(user))
                {
                    Clients.Caller.Alert("Du er allerede i denne kanal.", "Allerede i kanal", "info");
                    return;
                }
                channel.AddUser(user);
                db.SaveChanges();
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
        }

        public void ExitChannel(int channelId)
        {
            if (channelId == 0) return;
            using (HelpContext db = new HelpContext())
            {
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                User user = con.User;
                if (user != null)
                {
                    Channel channel = db.Channels.Find(channelId);
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
                            Question question = user.GetQuestion(channel);
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
                            foreach (Connection connection in channel.Users.SelectMany(us => us.Connections))
                            {
                                Clients.Client(connection.ConnectionId).RemoveUser(user.Id, channelId);
                            }
                        }
                    }
                    else
                    {
                        Clients.Caller.Alert("Kanalen blev ikke funder", "Kan ikke findes", "error");
                    }
                }
                db.SaveChanges();
            }


        }

        public void CreateNewChannel(string channelName)
        {
            using (HelpContext db = new HelpContext())
            {
                //var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                User user = con.User;
                if (user == null) return;
                if (user.AreAdministratorIn.Count > 4)
                {
                    Clients.Caller.Alert("Du kan ikke oprette mere end 5 kanaler af gangen", "Maks oprettede kanaler nået", "error");
                    return;
                }
                Channel channel = new Channel(user, channelName);
                channel.AddUser(user);
                user.Channel = channel;
                db.Users.AddOrUpdate(user);
                db.Channels.Add(channel);
                db.SaveChanges();
                SimpleChannel sc = channel.ToSimpleChannel();
                sc.IsAdmin = true;
                foreach (Connection connection in user.Connections)
                {
                    Clients.Client(connection.ConnectionId).AppendChannel(sc);
                }
            }
        }

        public void SetUsername(string name)
        {
            using (HelpContext db = new HelpContext())
            {
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                User user = con.User;
                if (user == null)
                {
                    user = new User
                    {
                        Name = name,
                    };
                    user.Connections.Add(new Connection() { ConnectionId = Context.ConnectionId });
                    db.Users.Add(user);
                }
                else
                {
                    user.Name = name;
                    foreach (Connection connection in user.Connections)
                    {
                        Clients.Client(connection.ConnectionId).UpdateUsername(name);
                    }
                    foreach (Channel channel in user.ChannelsIn)
                    {
                        Clients.Clients(
                            channel.Users.SelectMany(u => u.Connections).Select(c => c.ConnectionId).ToList())
                            .UpdateOtherUsername(name, user.Id, channel.Id);
                    }
                    foreach (Connection connection in user.Connections)
                    {
                        Clients.Client(connection.ConnectionId).SendUserId(user.Id);
                    }
                }
                db.SaveChanges();
            }
        }

        public void RequestHelp(string question, int channelid)
        {
            if (String.IsNullOrWhiteSpace(question)) question = "";
            using (HelpContext db = new HelpContext())
            {
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                User userFromDb = con.User;
                if (userFromDb == null)
                {
                    return;
                }
                Channel channel = db.Channels.Find(channelid);
                if (userFromDb.AreUserQuestioning(channel))
                {
                    Clients.Caller.SetQuestionState(true, channelid);
                    return;
                }
                Question q = userFromDb.RequestHelp(channel, question);
                if (q == null) return;
                db.Questions.Add(q);
                db.SaveChanges();
                SimpleUser su = new SimpleUser(userFromDb.Id, userFromDb.Name);
                SimpleQuestion sq = new SimpleQuestion(q.Id, q.Text, su);
                foreach (Connection connection in channel.Users.SelectMany(user => user.Connections))
                {
                    Clients.Client(connection.ConnectionId).AddQuestion(sq, channel.Id);
                }
                foreach (Connection connection in userFromDb.Connections)
                {
                    Clients.Client(connection.ConnectionId).SetQuestionState(true, channelid);
                }
            }

        }

        public void RemoveUserFromChannel(int id, int channelId)
        {
            if (id == 0) return;
            using (HelpContext db = new HelpContext())
            {
                User user = db.Users.Find(id);
                User callingUser = db.Connections.Find(Context.ConnectionId).User;
                if (callingUser == null) return;
                Channel channel = db.Channels.Find(channelId);
                if (channel == null) return;
                if (user == null)
                {
                    Clients.Caller.RemoveUser(id, channel.Id);
                    return;
                }
                if (callingUser.Id == user.Id)
                {
                    Clients.Caller.Alert("Du har lige forsøgt at smide dig selv ud af kanalen...", "Really?!", "warning");
                    return;
                }
                if (channel.IsUserAdministrator(callingUser))
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
            using (HelpContext db = new HelpContext())
            {
                User user = db.Users.Find(id);
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
            using (HelpContext db = new HelpContext())
            {
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                User user = con.User;
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
            using (HelpContext db = new HelpContext())
            {
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                User user = con.User;
                if (user == null || user.Channel == null) return;
                bool question = user.AreUserQuestioning(user.Channel);
                foreach (Connection connection in user.Connections)
                {
                    Clients.Client(connection.ConnectionId).SetChannel(user.Channel.Id, question);
                }
            }
        }

        public void CreateNewUser(string username, string email, string pw)
        {
            Debug.WriteLine("Here");
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(email) || String.IsNullOrWhiteSpace(pw))
            {
                Clients.Caller.Alert("En af værdier er ikke blevet sat.", "Fejl under oprettelse af bruger", "error");
                return;
            }
            using (HelpContext db = new HelpContext())
            {
                User user = db.Users.SingleOrDefault(u => u.EmailAddress.Equals(email));
                if (user != null)
                {
                    Clients.Caller.Alert("Emailadressen er allerede i brug.", "Fejl under oprettelse af bruger", "error");
                    return;
                }
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                user = con.User;
                if (user == null) return;
                if (!String.IsNullOrWhiteSpace(user.Pw) || !String.IsNullOrWhiteSpace(user.EmailAddress))
                {
                    Clients.Caller.Alert("Du er allerede logget ind.", "Fejl under oprettelse af bruger", "error");
                    return;
                }
                string pass = PasswordHash.CreateHash(pw);
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
                Clients.Caller.Alert("Ikke alt info er indtastet", "Fejl i indtasting", "error");
                return;
            }
            using (HelpContext db = new HelpContext())
            {
                //var currentUser = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                User currentUser = con.User;
                if (currentUser == null || !String.IsNullOrWhiteSpace(currentUser.EmailAddress) || !String.IsNullOrWhiteSpace(currentUser.Pw))
                {
                    Clients.Caller.Alert("Du er allerede logget ind", "Allerede logget ind", "error");
                    return;
                }
                User user = db.Users.SingleOrDefault(u => u.EmailAddress.Equals(email));
                if (user == null)
                {
                    Clients.Caller.Alert("Forkert mail eller kodeord", "Fejl under login", "error");
                    return;
                }
                bool success = PasswordHash.ValidatePassword(password, user.Pw);
                if (success)
                {
                    // Channels the klientuser is in
                    List<Channel> oldChannels =
                        currentUser.ChannelsIn.Where(channel => !user.ChannelsIn.Contains(channel)).ToList();
                    foreach (Channel channel in oldChannels)
                    {
                        user.ChannelsIn.Add(channel);
                        SimpleChannel sc = channel.ToSimpleChannel();
                        foreach (Connection conn in user.Connections)
                        {
                            Clients.Client(conn.ConnectionId).AppendChannel(sc);
                        }
                    }
                    Connection connection =
                        db.Connections.SingleOrDefault(conn => conn.ConnectionId.Equals(Context.ConnectionId));
                    
                    foreach (Channel channel in user.ChannelsIn.Where(ch => !currentUser.ChannelsIn.Contains(ch)))
                    {
                        SimpleChannel sc = channel.ToSimpleChannel();
                        sc.IsAdmin = channel.IsUserAdministrator(user);
                        Clients.Caller.AppendChannel(sc);
                    }
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
                    db.Users.Remove(currentUser);

                    db.SaveChanges();
                    Clients.Caller.LoginSuccess();
                    if (String.IsNullOrWhiteSpace(currentUser.Name) || !currentUser.Name.Equals(user.Name))
                    {
                        Clients.Caller.UpdateUsername(user.Name);
                    }
                    Clients.Caller.SendUserId(user.Id);
                }
                else
                {
                    Clients.Caller.Alert("Forkert mail eller kodeord", "Fejl under login", "error");
                }
            }
        }

        private void ExitChannel(Channel channel, User user)
        {
            using (HelpContext db = new HelpContext())
            {
                user = db.Users.Find(user.Id);
                if (user == null) return;
                channel = db.Channels.Find(channel.Id);
                if (channel == null) return;
                foreach (Connection con in channel.Users.SelectMany(u => u.Connections))
                {
                    Clients.Client(con.ConnectionId).RemoveUser(user.Id, channel.Id);
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
            using (HelpContext db = new HelpContext())
            {
                //var user = db.Users.SingleOrDefault(u => u.ConnectionId.Equals(Context.ConnectionId));
                Connection con = db.Connections.Find(Context.ConnectionId);
                if (con == null) return;
                User user = con.User;
                if (user == null)
                {
                    Clients.Caller.UserLoggedOut();
                    return;
                }
                user.Connections.Remove(con);
                db.Connections.Remove(con);
                User newUser = new User()
                {
                    Name = user.Name,
                    Ip = user.Ip
                };
                newUser.Connections.Add(new Connection() { ConnectionId = Context.ConnectionId });
                db.Users.Add(newUser);
                db.SaveChanges();
                Clients.Caller.UserLoggedOut();
                Clients.Caller.SendUserId(newUser.Id);
            }
        }
    }
}