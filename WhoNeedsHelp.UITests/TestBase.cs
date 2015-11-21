using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using WhoNeedsHelp.UITests.Constants;

namespace WhoNeedsHelp.UITests
{
    public abstract class TestBase : IDisposable
    {
        protected IWebDriver driver;
        protected WebDriverWait wait;

        public void Setup(Browser browser, OS os)
        {
            ICapabilities capabilities;
            switch (browser)
            {
                case Browser.Safari:
                    switch (os)
                    {
                        case OS.Mac:
                            capabilities = DesiredCapabilities.Safari();
                            driver = new RemoteWebDriver(new Uri($"http://{Servers.Mac}:4444/wd/hub"), capabilities);
                            break;
                        case OS.IOS:
                            DesiredCapabilities c = new DesiredCapabilities();
                            capabilities = c;
                            c.SetCapability(MobileCapabilityType.DeviceName, "Allan Marcuslund's Ipad");
                            c.SetCapability(MobileCapabilityType.PlatformName, "IOS");
                            c.SetCapability(MobileCapabilityType.PlatformVersion, "9.0.1");
                            c.SetCapability(CapabilityType.BrowserName, "Safari");
                            driver = new RemoteWebDriver(new Uri($"http://{Servers.Mac}:4723/wd/hub"), c);
                            break;
                    }

                    break;
                case Browser.Chrome:
                    ChromeOptions options = new ChromeOptions();
                    options.AddArgument("--incognito");
                    options.AddArgument("-incognito");
                    capabilities = DesiredCapabilities.Chrome();
                    switch (os)
                    {
                        case OS.Mac:
                            driver = new RemoteWebDriver(new Uri($"http://{Servers.Mac}:9515"), capabilities);
                            break;
                        case OS.Windows:
                            driver = new RemoteWebDriver(new Uri($"http://{Servers.Windows}:9515"), capabilities);
                            break;
                        case OS.AndroidTablet:
                            capabilities = new DesiredCapabilities();
                            DesiredCapabilities c = (DesiredCapabilities) capabilities;
                            c.SetCapability(MobileCapabilityType.DeviceName, "device");
                            c.SetCapability(MobileCapabilityType.PlatformName, "Android");
                            driver = new RemoteWebDriver(new Uri($"http://{Servers.Windows}:4723/wd/hub"), capabilities);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(os), os, null);
                    }
                    break;
                case Browser.Explorer:
                    if (os != OS.Windows)
                        throw new ArgumentOutOfRangeException(nameof(os), os, "Invalid OS for Internet Explorer");
                    capabilities = DesiredCapabilities.InternetExplorer();
                    driver = new RemoteWebDriver(new Uri($"http://{Servers.Windows}:4445/wd/hub"), capabilities);
                    break;
                case Browser.Edge:
                    break;
                case Browser.Firefox:
                    capabilities = DesiredCapabilities.Firefox();
                    switch (os)
                    {
                        case OS.Windows:
                            driver = new RemoteWebDriver(new Uri($"http://{Servers.Windows}:4444/wd/hub"), capabilities);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(os), os, null);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(browser), browser, "Unknown browser");
            }
            wait = new WebDriverWait(driver, new TimeSpan(0, 1, 0));
        }


        public void ClickButtonById(string id)
        {
            driver.FindElement(By.Id(id)).Click();
        }

        public void ClickButtonByCss(string css)
        {
            driver.FindElement(By.CssSelector(css)).Click();
        }

        public void TypeIntoInputByName(string name, string text)
        {
            driver.FindElement(By.Name(name)).SendKeys(text);
        }

        public void TypeIntoInputById(string id, string text)
        {
            driver.FindElement(By.Id(id)).SendKeys(text);
        }

        public void TypeIntoInputByCss(string css, string text)
        {
            driver.FindElement(By.CssSelector(css)).SendKeys(text);
        }

        public void WaitForElementById(string id)
        {
            wait.Until(webDriver => webDriver.FindElement(By.Id(id)).Displayed);
        }

        public void WaitForElementToDisappearById(string id)
        {
            wait.Until(webDriver => !webDriver.FindElement(By.Id(id)).Displayed);
        }

        public void WaitForElementByCss(string css)
        {
            wait.Until(webDriver => webDriver.FindElement(By.CssSelector(css)).Displayed);
        }

        public void WaitForElementToDisappearByCss(string css)
        {
            wait.Until(webDriver => !webDriver.FindElement(By.CssSelector(css)).Displayed);
        }

        public void Sleep(int miliseconds)
        {
            Thread.Sleep(miliseconds);
        }

        public string GetElementTextByCss(string css)
        {
            var ele = driver.FindElement(By.CssSelector(css));
            return ele.Text;
        }

        public string GetElementTextById(string id)
        {
            return driver.FindElement(By.Id(id)).Text;
        }

        [TearDown]
        public void TearDown()
        {
            Dispose();
        }

        public void Dispose()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}
