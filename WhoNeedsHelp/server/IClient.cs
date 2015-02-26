namespace WhoNeedsHelp
{
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
        void SetChannel(string channel, bool areUserQuestioning);
        void UpdateChannelCount(int activeUsers, int connectedUsers, string channelId);
        void SendQuestion(string question);
        void UpdateQuestion(string question, string questionId);
        void ReloadPage();
        void SetLayout(int layout);
        void SendChatMessage(string text, string author, string messageId, bool sender, bool appendToLast, bool canEdit);
        void SendChatMessages(string[] text, string[] author, string[] messageIds, bool[] sender, bool[] appendToLast, bool[] canEdit);
        void CheckVersion(int version);
        void RemoveChatMessage(string messageId);
        void IpDiscover(string[] channelIds, string[] channelNames);
    }
}