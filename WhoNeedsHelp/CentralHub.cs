﻿using System;
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
            Channel c = Users[Context.ConnectionId].CurrentChannel;
            var messages = from message in c.ChatMessages.Values
                where message.MessageId == messageId
                select message;
            var messagesList = messages.ToList();
            foreach (User user in c.GetActiveUsers())
            {
                foreach (ChatMessage message in messagesList)
                {
                    Clients.Client(user.ConnectionId).RemoveChatMessage(message.MessageId);
                    c.ChatMessages.Remove(message.MessageId);
                }
            }
            
        }


        private void Chat(string message)
        {
            User u = Users[Context.ConnectionId];
            if (u.CurrentChannel != null)
            {
                if (String.IsNullOrWhiteSpace(message)) return;
                Channel c = u.CurrentChannel;
                ChatMessage chatMessage = c.AddChatMessage(u, message);
                if (chatMessage != null)
                {
                    foreach (User user in c.GetActiveUsers())
                    {
                        Clients.Client(user.ConnectionId)
                            .SendChatMessage(chatMessage.Text, u.Name,
                                chatMessage.GetMessageId(), chatMessage.Author == user, c.AppendMessageToLast(chatMessage),
                                chatMessage.Author == user || c.Administrator == user);
                    }
                }
            }
        }

        private void ChangeQuestion(string question)
        {
            User u = Users[Context.ConnectionId];
            u.UpdateQuestion(u.CurrentChannel, question);
            string questionId = u.ConnectionId + "-" + u.CurrentChannel.ChannelId;
            foreach (User user in u.CurrentChannel.GetActiveUsers())
            {
                Clients.Client(user.ConnectionId).UpdateQuestion(String.IsNullOrWhiteSpace(question) ? "" : question, questionId);
            }
        }

        private void RemoveQuestion(string questionId)
        {
            if (String.IsNullOrWhiteSpace(questionId))
            {
                User u = Users[Context.ConnectionId];
                Channel c = u.CurrentChannel;
                questionId = u.ConnectionId + "-" + c.ChannelId;
                u.RemoveQuestion();
                c.UsersRequestingHelp.Remove(u);
                foreach (User user in c.GetActiveUsers())
                {
                    Clients.Client(user.ConnectionId).RemoveQuestion(questionId);
                }
            }
            else
            {
                Channel c = Users[Context.ConnectionId].CurrentChannel;
                string userId = questionId.Substring(0, 36);
                Debug.WriteLine(questionId);
                Debug.WriteLine(userId);
                User u = Users[userId];
                u.RemoveQuestion(c);
                c.UsersRequestingHelp.Remove(u);
                foreach (User user in c.GetActiveUsers())
                {
                    Clients.Client(user.ConnectionId).RemoveQuestion(questionId);
                }
                Clients.Client(u.ConnectionId).SetLayout(1);
            }
        }

        private void RemoveQuestion(User u, Channel c)
        {
            if (u == null || c == null)
            {
                return;
            }
            string questionId = u.ConnectionId + "-" + c.ChannelId;
            u.RemoveQuestion(c);
            c.UsersRequestingHelp.Remove(u);
            foreach (User user in c.GetActiveUsers())
            {
                Clients.Client(user.ConnectionId).RemoveQuestion(questionId);
            }
        }

        private void ChangeToChannel(string channelId)
        {
            User u = Users[Context.ConnectionId];
            int activeUsers;
            if (u.CurrentChannel != null)
            {
                activeUsers = u.CurrentChannel.GetActiveUserCount()-1;
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
                    authorList.Add(chatMessage.Author.Name);
                    messageIdsList.Add(chatMessage.GetMessageId());
                    senderList.Add(chatMessage.Author == u);
                    appendToLastList.Add(c.AppendMessageToLast(index, chatMessage.Author));
                    canEditList.Add(chatMessage.Author == u || c.Administrator == u);
                }
                Clients.Caller.SendChatMessages(textList.ToArray(), authorList.ToArray(), messageIdsList.ToArray(), senderList.ToArray(), appendToLastList.ToArray(), canEditList.ToArray());
            }
        }

        private void JoinChannel(string channelId)
        {
            Channels[channelId].AddUser(Users[Context.ConnectionId]);
            Clients.Caller.AppendChannel(Channels[channelId].ChannelName, channelId);
        }

        private void SearchForChannel(string parameter)
        {
            parameter = parameter.ToLower();
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
                    if (c.UsersRequestingHelp.Contains(u))
                    {
                        c.UsersRequestingHelp.Remove(u);
                    }
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
                case 2:
                    // Request a version number from the server
                    Clients.Caller.CheckVersion(1);
                    break;
            }
        }

        private void GetQuestion()
        {
            string question = Users[Context.ConnectionId].GetQuestion(Users[Context.ConnectionId].CurrentChannel);
            Debug.WriteLine(question);
            Clients.Caller.SendQuestion(String.IsNullOrWhiteSpace(question) ? "" : question);
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
            if (Users.ContainsKey(Context.ConnectionId))
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