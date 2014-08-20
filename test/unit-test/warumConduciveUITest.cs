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
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Linq;

namespace unit_test
{
    /// <summary>
    /// Tests the UI
    /// </summary>
    public class warumConduciveUITest
    {
        [Trait("unit-test", "LoadApp")]
        [Fact]
        public async Task LoadAppHomePage()
        {
            IWebDriver driver = this.LoadSplunkHomePageAndSignIn();

            try
            {
                //the warum-pas link
                //var applink = driver.FindElement(By.CssSelector(".app-title.group.app-warum_conducive_web.draghandle"));

                var ele1 = driver.FindElement(By.CssSelector(".app-wrapper.appName-warum_conducive_web"));
                var ele2 = ele1.FindElement(By.CssSelector(".slideNavPlaceHolder.group.app-slidenav"));
                ele2.FindElement(By.LinkText("Summary")).Click();
                IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                Assert.Equal("Summary", driver.Title);
            }
            catch (Exception e)
            {

            }
            finally
            {
                driver.Quit();
            }
        }

        [Trait("unit-test", "Senario2Test1")]
        [Fact]
        public void Senario2Test1()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            IWebDriver driver = this.LoadSplunkHomePageAndSignIn();
            try
            {
                driver.Navigate().GoToUrl("http://localhost:8000/dj/en-us/warum_conducive_web/Summary/");
                driver.FindElement(By.ClassName("time-label")).Click();

                //open "Date Range"
                IReadOnlyCollection<IWebElement> searchCriterias = driver.FindElements(By.ClassName("accordion-toggle"));
                searchCriterias.ElementAt(3).Click();

                // choose "Between" and filled up the earliest/latest inputs
                var e3 = driver.FindElement(By.CssSelector(".accordion-group.active"));
                var e4 = e3.FindElement(By.ClassName("accordion-body"));
                var e5 = e4.FindElement(By.ClassName("timerangepicker-range-type"));
                var e6 = e5.FindElement(By.ClassName("dropdown-toggle"));
                e6.Click();
                var e7 = driver.FindElement(By.CssSelector(".dropdown-menu.dropdown-menu-selectable.dropdown-menu-narrow.open"));
                var e8 = e7.FindElements(By.ClassName("link-label"));
                e8[0].Click();

                var earliestDate = e4.FindElement(By.ClassName("timerangepicker-earliest-date"));
                this.SendInput("08/11/2014", earliestDate);
                var latestDate = e4.FindElement(By.ClassName("timerangepicker-latest-date"));
                this.SendInput("08/11/2014", latestDate);

                //click on apply button
                var e11 = e4.FindElement(By.ClassName("apply"));
                e11.Click();

                //hover on the side color
                var trendPanel = driver.FindElement(By.Id("trend-chart-div"));

                //click on the center panel color block
                var svg = driver.FindElement(By.XPath("//*[name()='svg']"));
                var allActionBars = svg.FindElement(By.ClassName("highcharts-series-group")).FindElements(By.ClassName("highcharts-series"));

                IReadOnlyCollection<IWebElement> topUsers = null;
                for (int i = 0; i < allActionBars.Count; i++)
                {
                    var u = allActionBars[i].FindElement(By.ClassName("highcharts-tracker"));
                    u.Click();
                    var userTable = driver.FindElement(By.Id("user-table"));
                    topUsers = userTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
                    this.VerifyTopReturnsSortedCorrectly(topUsers.Select(a => a.Text));

                    var documentTable = driver.FindElement(By.Id("document-table"));
                    var topDocuments = documentTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
                    this.VerifyTopReturnsSortedCorrectly(topDocuments.Select(a => a.Text));

                }

                this.TestClickOnTopUser(driver, topUsers);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                driver.Quit();
            }

            Console.WriteLine("total spend " + watch.Elapsed.TotalSeconds);
        }

        private void TestClickOnTopUser(IWebDriver driver, IReadOnlyCollection<IWebElement> tops)
        {
            Random rand = new Random();
            int index = rand.Next(0, tops.Count);
            tops.ElementAt(index).FindElements(By.ClassName("numeric"))[0].Click();

            //verify user-event-document page
            var topEventsTable = driver.FindElement(By.Id("user-table"));
            var topEvents = topEventsTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
            this.VerifyTopReturnsSortedCorrectly(topEvents.Select(a => a.Text));

        }

        private void SendInput(string str, IWebElement element)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            do
            {
                element.Clear();
            }
            while (watch.Elapsed.TotalSeconds < 10 && !(string.IsNullOrEmpty(element.GetAttribute("value"))));

            element.SendKeys(str);
            element.SendKeys(Keys.Enter);
        }

        private void VerifyTopReturnsSortedCorrectly(IEnumerable<string> inputs)
        {
            var current = int.MaxValue;
            foreach (string str in inputs)
            {
                var next = int.Parse(str.Split(' ')[1]);
                Assert.True(current >= next, string.Format("sorting is wrong at element {0}", str));
                current = next;
            }
        }

        private IWebDriver LoadSplunkHomePageAndSignIn()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            IWebDriver driver = new FirefoxDriver();

            // set the timeout after page load to 30seconds
            driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 30));
            Console.WriteLine("load browers taks" + watch.Elapsed.TotalSeconds);
            watch.Reset();

            driver.Navigate().GoToUrl("http://localhost:8000");
            Console.WriteLine("load page takes " + watch.Elapsed.TotalSeconds);
            watch.Reset();
            Assert.Equal(driver.Title, "Login - Splunk");

            // login 
            driver.FindElement(By.Id("username")).SendKeys("admin");
            driver.FindElement(By.Id("password")).SendKeys("changeme");
            driver.FindElement(By.ClassName("splButton-primary")).Submit();

            if (!driver.Title.Contains("Home | Splunk"))
            {
                driver.FindElement(By.CssSelector(".spl-change-password-skip")).Submit();
            }

            Console.WriteLine("login takes " + watch.Elapsed.TotalSeconds);
            return driver;
        }
    }
}

