using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using Moq;
using NUnit.Framework;
using WhoNeedsHelp.App;
using WhoNeedsHelp.DB;
using WhoNeedsHelp.Models;
using WhoNeedsHelp.Server.Chat;

namespace WhoNeedsHelp.Tests.SignalR
{
    [TestFixture]
    public class UserManagement
    {
        [Test]
        public void SetUsernameOnExistingUser()
        {
            string username = "Zlepper";
            var usermock = new Mock<User>();
            usermock.Setup(u => u.Connections).Returns();
            var mockedIClient = new Mock<IClient>();
            var dbmock = new Mock<IHelpContext>();
            dbmock.Setup(d => d.GetUserByConnection(It.IsAny<string>())).Returns(usermock.Object);
            var db = dbmock.Object;
            var hub = new CentralHub(db);
            var clientMocks = new Mock<IHubCallerConnectionContext<IClient>>();
            clientMocks.Setup(c => c.Client(It.IsAny<string>())).Returns(mockedIClient.Object);

            var clients = clientMocks.Object;
            var context = new Mock<HubCallerContext>();
            context.Setup(c => c.ConnectionId).Returns("testConnection");

            hub.SetUsername(username);

            mockedIClient.Verify(m => m.UpdateUsername(username), Times.Once);
        }
    }
}
