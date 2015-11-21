using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using WhoNeedsHelp.UITests.Constants;

namespace WhoNeedsHelp.UITests
{
    public abstract class WhoNeedsHelpBase : TestBase
    {
        public void LoadWebserver()
        {
            driver.Navigate().GoToUrl(Program.Url);
            //Sleep(10000);
            //var ele = driver.FindElement(By.Id(Frontpage.LoadingAnimation));
            //Console.WriteLine(ele);
            WaitForElementToDisappearById(Frontpage.LoadingAnimation);
        }
    }
}
