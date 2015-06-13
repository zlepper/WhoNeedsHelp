using WhoNeedsHelp.Simples;

namespace WhoNeedsHelp.server
{
    public interface IClient
    {
        void AppendChannel(SimpleChannel simpleChannel);
        void AddQuestion(SimpleQuestion question, int channelid);
        void RemoveQuestion(int questionId);
        void ExitChannel(int channelId);
        void SetChannel(int channelId, bool areUserQuestioning);
        void SendQuestion(string question);
        void UpdateQuestion(string questionText, int questionId, int channelId);
        void SendChatMessage(SimpleChatMessage message, int channelId);
        void CheckVersion(int version);
        void RemoveChatMessage(int messageId);
        void IpDiscover(string[] channelIds, string[] channelNames);
        void ClearChat(int channelId);
        void LoginSuccess();
        void ShowChannels(string[] channelId, string[] channelName);
        void UserCreationSuccess();
        void UserLoggedOut();
        void UpdateUsername(string name);
        void AppendUser(SimpleUser user, int channelId);
        void RemoveUser(int id, int channelid);
        void Alert(string message, string title, string type);
        void SetQuestionState(bool hasQuestion, int channelid);
        void SendUserId(int id);
        void UpdateOtherUsername(string name, int userid, int channelid);
        void SetAdminState(int id, bool isAdmin);
        void ClearChannels();
        void SendReloginData(string loginKey, int userId);
        void TokenLoginFailed();
    }
}