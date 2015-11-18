using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoNeedsHelp.DB;
using WhoNeedsHelp.Models;
using WhoNeedsHelp.Server.Chat;
using WhoNeedsHelp.Server.Mail;
using WhoNeedsHelp.Simples;

namespace WhoNeedsHelp.App
{
    public partial class CentralHub
    {
        public void SetUsername(string name)
        {
            User user = DB.GetUserByConnection(Context.ConnectionId);
            if (user == null)
            {
                user = new User
                {
                    Name = name,
                };
                user.Connections.Add(new Connection() { ConnectionId = Context.ConnectionId });
                DB.Users.Add(user);
            }
            else
            {
                user.Name = name;
                Clients.Clients(user.Connections.Select(c => c.ConnectionId).ToList()).UpdateUsername(name);

                foreach (Channel channel in user.ChannelsIn)
                {
                    Clients.Clients(
                        channel.Users.SelectMany(u => u.Connections).Select(c => c.ConnectionId).ToList())
                        .UpdateOtherUsername(name, user.Id, channel.Id);
                }
                
                Clients.Clients(user.Connections.Select(c => c.ConnectionId).ToList()).SendUserId(user.Id);
            }
            DB.SaveChanges();
        }

        public void CreateNewUser(string username, string email, string pw, bool stayLoggedIn)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pw))
            {
                Clients.Caller.Alert("En af værdier er ikke blevet sat.");
                return;
            }
            User user = DB.Users.SingleOrDefault(u => u.EmailAddress.Equals(email));
            if (user != null)
            {
                Clients.Caller.Alert("Emailadressen er allerede i brug.");
                return;
            }
            user = DB.GetUserByConnection(Context.ConnectionId);
            if (user == null) return;
            if (!string.IsNullOrWhiteSpace(user.Pw) || !string.IsNullOrWhiteSpace(user.EmailAddress))
            {
                Clients.Caller.Alert("Du er allerede logget ind.");
                return;
            }
            string pass = PasswordHash.CreateHash(pw);
            user.Pw = pass;
            user.EmailAddress = email;

            Guid key = Guid.NewGuid();
            Clients.Caller.SendReloginData(key.ToString(), user.Id, stayLoggedIn);
            user.GenerateLoginToken(key);

            user.LastLogin = DateTime.Now;
            DB.SaveChanges();
            Clients.Caller.UserCreationSuccess();
        }

        public void LoginWithToken(int userId, string tokenKey, bool longer)
        {
            User user;
            if (userId == -1)
            {
                user = DB.Connections.Find(Context.ConnectionId).User;
                var key = Guid.NewGuid();
                user.GenerateLoginToken(key);
                Clients.Caller.SendReloginData(key.ToString(), user.Id, longer);
                return;
            }
            user = DB.Users.Find(userId);
            LoginToken lt = null;
            if ((lt = user.CheckLoginToken(tokenKey)) != null)
            {
                Clients.Caller.SendUserId(user.Id);
                Clients.Caller.UpdateUsername(user.Name);
                if (!string.IsNullOrWhiteSpace(user.EmailAddress))
                    Clients.Caller.LoginSuccess();
                foreach (Channel channel in user.ChannelsIn)
                {
                    var sc = channel.ToSimpleChannel();
                    sc.IsAdmin = channel.IsUserAdministrator(user);
                    Clients.Caller.AppendChannel(sc);
                }
                Guid newKey = Guid.NewGuid();
                Clients.Caller.SendReloginData(newKey.ToString(), user.Id, longer);
                user.GenerateLoginToken(newKey);
                DB.LoginTokens.Remove(lt);
                user.LastLogin = DateTime.Now;
                Connection connection =
                    DB.Connections.SingleOrDefault(conn => conn.ConnectionId.Equals(Context.ConnectionId));
                var u = connection?.User;
                u?.Connections.Remove(connection);
                user.Connections.Add(connection);
                DB.Users.Remove(u);
                DB.SaveChanges();
            }
            else
            {
                Clients.Clients(user.Connections.Select(c => c.ConnectionId).ToList()).UserLoggedOut();
                DB.LoginTokens.RemoveRange(user.LoginTokens);
                Clients.Caller.TokenLoginFailed();
            }
            DB.SaveChanges();
        }

        public void LoginUser(string email, string password, bool stayLoggedIn)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Clients.Caller.Alert("Ikke alt info er indtastet");
                return;
            }
            User currentUser = DB.GetUserByConnection(Context.ConnectionId);
            if (currentUser == null || !string.IsNullOrWhiteSpace(currentUser.EmailAddress) || !string.IsNullOrWhiteSpace(currentUser.Pw))
            {
                Clients.Caller.Alert("Du er allerede logget ind");
                return;
            }
            User user = DB.Users.SingleOrDefault(u => u.EmailAddress.Equals(email));
            if (user == null)
            {
                Clients.Caller.Alert("Forkert mail eller kodeord");
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
                    DB.Connections.SingleOrDefault(conn => conn.ConnectionId.Equals(Context.ConnectionId));

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
                DB.Users.Remove(currentUser);
                Guid key = Guid.NewGuid();
                Clients.Caller.SendReloginData(key.ToString(), user.Id, stayLoggedIn);

                user.GenerateLoginToken(key);
                user.LastLogin = DateTime.Now;
                DB.SaveChanges();
                Clients.Caller.LoginSuccess();
                if (string.IsNullOrWhiteSpace(currentUser.Name) || !currentUser.Name.Equals(user.Name))
                {
                    Clients.Caller.UpdateUsername(user.Name);
                }
                Clients.Caller.SendUserId(user.Id);
            }
            else
            {
                Clients.Caller.Alert("Forkert mail eller kodeord");
            }

        }

        public void LogoutUser(string key)
        {
            Connection con = DB.Connections.Find(Context.ConnectionId);
            if (con == null) return;
            User user = con.User;
            if (user == null)
            {
                Clients.Caller.UserLoggedOut();
                return;
            }
            user.Connections.Remove(con);
            DB.Connections.Remove(con);
            User newUser = new User()
            {
                Name = user.Name,
                Ip = user.Ip
            };
            newUser.Connections.Add(new Connection() { ConnectionId = Context.ConnectionId });
            DB.Users.Add(newUser);
            if (!string.IsNullOrWhiteSpace(key))
            {
                LoginToken lt = user.CheckLoginToken(key);
                if (lt != null)
                {
                    DB.LoginTokens.Remove(lt);
                }
            }
            DB.SaveChanges();
            Clients.Caller.UserLoggedOut();
            Clients.Caller.SendUserId(newUser.Id);
        }

        public void RequestPasswordReset(string email)
        {
            UserMail um = new UserMail();
            Clients.Caller.PasswordResetRequestResult(um.SendPasswordRecovery(email));
        }

        public void ResetPassword(string key, string password, string email)
        {
            key = key.Trim();
            var user = DB.Users.SingleOrDefault(u => u.EmailAddress.Equals(email));
            if (user == null)
            {
                Clients.Caller.PasswordResetResult(false);
                return;
            }
            if (user.CanPasswordBeReset(key))
            {
                user.Pw = PasswordHash.CreateHash(password);
                Clients.Caller.PasswordResetResult(true);
                user.ResetExpiresAt = DateTime.Now;
                user.ResetKey = null;
                DB.SaveChanges();
                LoginUser(email, password, false);
            }
            else
            {
                Clients.Caller.PasswordResetResult(false);
            }
        }

        public void ChangePassword(string oldpass, string newpass)
        {
            var con = DB.Connections.SingleOrDefault(c => c.ConnectionId.Equals(Context.ConnectionId));
            if (con == null) return;
            var user = con.User;
            if (user == null) return;
            if (string.IsNullOrWhiteSpace(user.EmailAddress))
            {
                Clients.Caller.UserLoggedOut();
                return;
            }
            if (PasswordHash.ValidatePassword(oldpass, user.Pw))
            {
                user.Pw = PasswordHash.CreateHash(newpass);
                DB.SaveChanges();
                Clients.Caller.PasswordChanged(true);
                return;
            }
            Clients.Caller.PasswordChanged(false);


        }

        public void LogoutAll()
        {
            var con = DB.Connections.SingleOrDefault(c => c.ConnectionId.Equals(Context.ConnectionId));
            if (con == null) return;
            var user = con.User;
            if (user == null) return;
            foreach (Connection connection in user.Connections.Where(c => !c.ConnectionId.Equals(Context.ConnectionId)))
            {
                Clients.Client(connection.ConnectionId).UserLoggedOut();
            }
            DB.LoginTokens.RemoveRange(user.LoginTokens);
            Clients.Caller.AllUsersLoggedOut();
            DB.SaveChanges();
        }
        private void ExitChannel(Channel channel, User user)
        {
            user = DB.Users.Find(user.Id);
            if (user == null) return;
            channel = DB.Channels.Find(channel.Id);
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
            DB.SaveChanges();
        }
    }
}
