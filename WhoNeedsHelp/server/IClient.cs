using System.Collections.Generic;
using WhoNeedsHelp.server;
using WhoNeedsHelp.Simples;

namespace WhoNeedsHelp
{
    public interface IClient
    {
        void AppendChannel(SimpleChannel simpleChannel);
        void AppendChannel2(SimpleChannel simpleChannel);
        //void AddQuestions(string[] usernames, string[] questions, string[] questionIds, bool admin = false);
        //void AddQuestion(string username, string question, string questionId, bool admin = false);
        void AddQuestions(List<SimpleQuestion> questions, int channelid);
        void AddQuestion(SimpleQuestion question, int channelid);
        void RemoveQuestion(int questionId);
        void ErrorChannelAlreadyMade();
        void Log(string text);
        void ExitChannel(int channelId);
        void SetChannel(int channelId, bool areUserQuestioning);
        void UpdateChannelCount(int activeUsers, int connectedUsers, int channelId);
        void SendQuestion(string question);
        void UpdateQuestion(string questionText, int questionId);
        void ReloadPage();
        void SendChatMessage(string text, string author, string messageId, bool sender, bool appendToLast, bool canEdit);
        void SendChatMessages(string[] text, string[] author, string[] messageIds, bool[] sender, bool[] appendToLast, bool[] canEdit);
        void CheckVersion(int version);
        void RemoveChatMessage(string messageId);
        void IpDiscover(string[] channelIds, string[] channelNames);
        void ClearChat();
        void LoginSuccess();
        void LoginFailed();
        void ShowChannels(string[] channelId, string[] channelName);
        void UserCreationFailed(string errorMessage);
        void UserCreationSuccess();
        void UserLoggedOut();
        void UpdateUsername(string name);
        void UpdateQuestionAuthorName(string name, string questionId);
        void UpdateChatMessageAuthorName(string name, string[] chatMessageIds);
        void ErrorChat(string errorMessage);
        void AppendUsers(string[] toArray, int[] ids, bool admin);
        void AppendUser(string name, int id, bool admin);
        void RemoveUser(int id);
        void Alert(string message, string title, string type);
        void SetQuestionState(bool hasQuestion, int channelid);
    }
}