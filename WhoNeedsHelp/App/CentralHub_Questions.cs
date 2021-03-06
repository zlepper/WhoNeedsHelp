﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoNeedsHelp.DB;
using WhoNeedsHelp.Models;
using WhoNeedsHelp.Server.Chat;
using WhoNeedsHelp.Simples;

namespace WhoNeedsHelp.App
{
    public partial class CentralHub
    {
        public void ChangeQuestion(string question, int channelId)
        {
            User user = DB.Connections.Find(Context.ConnectionId).User;
            if (user == null) return;
            Channel channel = DB.Channels.Find(channelId);
            if (channel == null) return;
            Question q = user.UpdateQuestion(channel, question);
            if (q == null) return;
            foreach (Connection connection in channel.Users.SelectMany(use => use.Connections))
            {
                Clients.Client(connection.ConnectionId).UpdateQuestion(string.IsNullOrWhiteSpace(q.Text) ? "" : question, q.Id, channelId);
            }
            DB.SaveChanges();

        }

        public void EditOwnQuestion(int channelId)
        {
            User user = DB.Connections.Find(Context.ConnectionId).User;
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

        public void RemoveOwnQuestion(int channelId)
        {
            Channel channel = DB.Channels.Find(channelId);
            if (channel == null) return;
            User user = DB.Connections.Find(Context.ConnectionId).User;
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
                DB.Questions.Remove(question);
                DB.SaveChanges();
            }
            else
            {
                Clients.Caller.SetQuestionState(false, channelId);
            }
        }

        public void RemoveQuestion(int questionId)
        {
            if (questionId == 0)
            {
                return;
            }
            Question q = DB.Questions.Include(qu => qu.User).Include(qu => qu.Channel).SingleOrDefault(qu => qu.Id.Equals(questionId));
            if (q == null)
            {
                Clients.Caller.RemoveQuestion(questionId);
                return;
            }
            User callingUser = DB.Connections.Find(Context.ConnectionId).User;
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
            DB.Questions.Remove(q);
            DB.SaveChanges();
        }

        public void RequestHelp(string question, int channelid)
        {
            if (string.IsNullOrWhiteSpace(question)) question = "";
            Connection con = DB.Connections.Find(Context.ConnectionId);
            if (con == null) return;
            User userFromDb = con.User;
            if (userFromDb == null)
            {
                return;
            }
            Channel channel = DB.Channels.Find(channelid);
            if (userFromDb.AreUserQuestioning(channel))
            {
                Clients.Caller.SetQuestionState(true, channelid);
                return;
            }
            Question q = userFromDb.RequestHelp(channel, question);
            if (q == null) return;
            DB.Questions.Add(q);
            DB.SaveChanges();
            SimpleQuestion sq = q.ToSimpleQuestion();
            foreach (Connection connection in channel.Users.SelectMany(user => user.Connections))
            {
                Clients.Client(connection.ConnectionId).AddQuestion(sq, channel.Id);
            }
            foreach (Connection connection in userFromDb.Connections)
            {
                Clients.Client(connection.ConnectionId).SetQuestionState(true, channelid);
            }
        }

        public void SyncChannels(Dictionary<int, int[]> chs)
        {
            foreach (KeyValuePair<int, int[]> c in chs)
            {
                Channel channel = DB.Channels.Find(c.Key);
                if (channel == null) continue;
                List<Question> l = channel.Questions.ToList();
                int[] questions = new int[l.Count];
                for (int i = 0; i < questions.Length; i++)
                {
                    questions[i] = l[i].Id;
                }
                if (!questions.SequenceEqual(c.Value))
                {
                    Clients.Caller.AppendChannel(channel.ToSimpleChannel());
                }
            }
        }
    }
}
