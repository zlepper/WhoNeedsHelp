using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace WhoNeedsHelp.UITests.Channel
{
    [TestFixture]
    public class Creation : WhoNeedsHelpBase
    {
        [TestCase(Browser.Chrome, OS.Windows)]
        public void CanCreateChannel(Browser browser, OS os)
        {
            Setup(browser, os);
            LoadWebserver();

        }
    }
}
