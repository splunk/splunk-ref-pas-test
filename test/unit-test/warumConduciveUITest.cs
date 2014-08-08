using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using Xunit;

namespace unit_test
{
    /// <summary>
    /// Tests the UI
    /// </summary>
    public class warumConduciveUITest
    {
        [Trait("unit-test", "LoadApp")]
        [Fact]
        public async Task LoadApp()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Console.WriteLine("start..." + watch.Elapsed.TotalSeconds);

            // the same way we can setup webDriver to use other browsers
            IWebDriver driver = new FirefoxDriver();
            Console.WriteLine("intit driver takes" + watch.Elapsed.TotalSeconds);

            try
            {
                // set the timeout after page load to 30seconds
                driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 30));
                Console.WriteLine("load browers taks" + watch.Elapsed.TotalSeconds);

                driver.Navigate().GoToUrl("http://localhost:8000");
                Console.WriteLine("load page takes " + watch.Elapsed.TotalSeconds);

                Console.WriteLine("WeDoQA=" + driver.Title);

                // login 
                driver.FindElement(By.Id("username")).SendKeys("admin");
                driver.FindElement(By.Id("password")).SendKeys("changeme");
                driver.FindElement(By.ClassName("splButton-primary")).Submit();

                if (driver.FindElements(By.CssSelector(".spl-change-password-skip")).Count > 0)
                {
                    driver.FindElement(By.CssSelector(".spl-change-password-skip")).Submit();
                }

                driver.Navigate().GoToUrl("http://localhost:8000/en-us/app/warum_conducive_web/");
                Console.WriteLine("warum_conducive_web=" + driver.Title);
                Assert.Equal("warum_conducive_web Home Page", driver.Title);
            }
            finally
            {
                driver.Quit();
            }

            Console.WriteLine("total spend " + watch.Elapsed.TotalSeconds);
        }
    }
}

