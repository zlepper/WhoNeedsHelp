﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoNeedsHelp.Server.Chat;

namespace WhoNeedsHelp.App
{
    public partial class CentralHub
    {
        public void LoginOrCreateUserWithApi(string username, string userid, string password)
        {
            // Validate the data
            if (string.IsNullOrWhiteSpace(username)) return;
            if (string.IsNullOrWhiteSpace(password)) return;
            if (string.IsNullOrWhiteSpace(userid)) return;

            // Open the database connection
            using (HelpContext db = new HelpContext())
            {
                // Find the connecting user as we need to delete this
                var userConnection = db.Connections.Find(Context.ConnectionId).User;
                db.Users.Remove(userConnection);
                db.SaveChanges();
                // Get the user from the database
                var user = db.Users.FirstOrDefault(u => u.VirtualId.Equals(userid));

                // Check if the user has connected before
                if (user == null)
                {
                    // This is the first time this user has connected
                    user = new User()
                    {
                        Name = username,
                        Pw = PasswordHash.CreateHash(password),
                        VirtualId = userid
                    };
                    user.Connections.Add(new Connection() {ConnectionId = Context.ConnectionId});

                    // Save the user to the DB
                    db.Users.Add(user);
                    db.SaveChanges();
                    // Inform the client that it can proceed
                    Clients.Caller.SendUserId(user.Id);
                }
                else
                {
                    // This is not the first time the user has connected

                    // Validate the users password token
                    bool success = PasswordHash.ValidatePassword(password, user.Pw);
                    if (success)
                    {
                        // Save this connection so we can find it later
                        user.Connections.Add(new Connection() {ConnectionId = Context.ConnectionId});
                        db.SaveChanges();
                        // Inform the client that it can proceed
                        Clients.Caller.SendUserId(user.Id);

                        if (!user.Name.Equals(username))
                        {
                            SetUsername(username);
                        }
                    }
                    else
                    {
                        // Somebody send an invalid user password in the connection url
                        Clients.Caller.Alert("Connection refused", "", "error");
                    }
                }
                
                db.SaveChanges();
            }
        }

        public void JoinOrCreateChannelWithApi(string channelname, string channelid, string admintoken)
        {
            // Validate the data received
            if (string.IsNullOrWhiteSpace(channelname)) return;
            if (string.IsNullOrWhiteSpace(channelid)) return;

            // Connect to the database
            using (HelpContext db = new HelpContext())
            {
                // Find the connecting user
                var user = db.Connections.Find(Context.ConnectionId).User;
                if (user == null) return;

                // Find the channel the user wants to connect to
                var channel = db.Channels.FirstOrDefault(c => c.VirtualId.Equals(channelid));

                // Check if the channel has already been created
                if (channel == null)
                {
                    // The channel does not exist

                    // Only people with a admintoken should be able to create channels with the API
                    if (string.IsNullOrWhiteSpace(admintoken))
                    {
                        Clients.Caller.Alert("Du skal være lærer for at gøre dette. ", "Manglende rettighed", "error");
                    }
                    else
                    {
                        // Create the new channel and add the user as admin/teacher
                        channel = new Channel(user, channelname)
                        {
                            AdminHash = PasswordHash.CreateHash(admintoken),
                            VirtualId = channelid
                        };
                        channel.AddAdministrator(user);

                        // Add the channel to the database
                        db.Channels.Add(channel);
                        db.SaveChanges();

                        // Send the channel to the connecting user
                        var sch = channel.ToSimpleChannel();
                        sch.IsAdmin = channel.IsUserAdministrator(user);
                        foreach (Connection con in user.Connections)
                        {
                            Clients.Client(con.ConnectionId).AppendChannel(sch);
                        }
                    }
                }
                else
                {
                    // The channel does exist

                    // If the user is already in the channel, then no need to do anything. 
                    if (user.ChannelsIn.Contains(channel))
                    {
                        var sch = channel.ToSimpleChannel();
                        sch.IsAdmin = channel.IsUserAdministrator(user);
                        Clients.Caller.AppendChannel(sch);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(admintoken))
                        {
                            // It's a student that is trying to connect
                            channel.AddUser(user);
                            db.SaveChanges();
                            var sch = channel.ToSimpleChannel();
                            sch.IsAdmin = channel.IsUserAdministrator(user);
                            var su = user.ToSimpleUser();
                            foreach (Connection con in user.Connections)
                            {
                                Clients.Client(con.ConnectionId).AppendChannel(sch);
                            }
                            foreach (Connection connection in channel.Users.SelectMany(u => u.Connections))
                            {
                                Clients.Client(connection.ConnectionId).AppendUser(su, channel.Id);
                            }
                        }
                        else
                        {
                            // It's a teacher that is trying to connect
                            // Check that the teacher is allowed to administer this channel
                            if (PasswordHash.ValidatePassword(admintoken, channel.AdminHash))
                            {
                                channel.AddUser(user);
                                channel.AddAdministrator(user);
                                var sch = channel.ToSimpleChannel();
                                sch.IsAdmin = channel.IsUserAdministrator(user);
                                foreach (Connection con in user.Connections)
                                {
                                    Clients.Client(con.ConnectionId).AppendChannel(sch);
                                }
                                var su = user.ToSimpleUser();
                                foreach (Connection connection in channel.Users.SelectMany(u => u.Connections))
                                {
                                    Clients.Client(connection.ConnectionId).AppendUser(su, channel.Id);
                                }
                            }
                            else
                            {
                                Clients.Caller.Alert("Invalid channeltoken", "", "error");
                            }
                        }
                    }
                }
                db.SaveChanges();
                if (channel != null)
                {
                    Clients.Caller.SetChannel(channel.Id);
                }
            }
        }
    }
}