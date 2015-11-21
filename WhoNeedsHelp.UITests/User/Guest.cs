using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using WhoNeedsHelp.UITests.Constants;

namespace WhoNeedsHelp.UITests.User
{
    [TestFixture]
    public class Guest : WhoNeedsHelpBase
    {
        [TestCase(Browser.Chrome, OS.Windows)]
        public void CanLoginToUser(Browser browser, OS os)
        {
            Setup(browser, os);
            LoadWebserver();
            Sleep(2000);
            TypeIntoInputById(Frontpage.NameInput, "TestUser");
            ClickButtonById("selectUsernameButton");
            Sleep(2000);
            WaitForElementByCss(".helplist");
            Assert.That(driver.FindElement(By.ClassName("helplist")).Displayed, Is.True);
        }
    }
}
