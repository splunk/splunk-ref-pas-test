using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace unit_test
{
    public class MyFixture : IDisposable
    {
        public IWebDriver Driver
        {
            get;
            internal set;
        }

        /// <summary>
        /// This should only be run once before all tests running
        /// </summary>
        public MyFixture()
        {
            this.Driver = new FirefoxDriver();
        }

        /// <summary>
        /// This should only be run once after all tests running
        /// </summary>
        public void Dispose()
        {
            if (this.Driver != null)
            {
                this.Driver.Dispose();
            }
        }
    }

    /// <summary>
    /// Tests the UI
    /// </summary>
    public class WarumConduciveUITest : IUseFixture<MyFixture>, IDisposable
    {
        private static IWebDriver driver = null;
        private static string splunkServerUrl = "http://10.80.9.38:8000/";
        private static string conduciveAppUrl = splunkServerUrl + "dj/en-us/warum_conducive_web/Summary/";
        private static string splunkHomeUrl = splunkServerUrl + "en-US/app/launcher/home";
        private static bool firstTestRun = false;

        public void SetFixture(MyFixture data)
        {
            if (!firstTestRun)
            {
                driver = data.Driver;
                this.LoadSplunkHomePageAndSignIn();
                firstTestRun = true;
            }
        }

        public void Dispose()
        {
        }

        [Trait("unit-test", "LoadApp")]
        [Fact]
        public void LoadAppHomePage()
        {
            driver.Navigate().GoToUrl(splunkHomeUrl);
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            var ele1 = driver.FindElement(By.CssSelector(".app-wrapper.appName-warum_conducive_web"));
            var ele2 = ele1.FindElement(By.CssSelector(".slideNavPlaceHolder.group.app-slidenav"));
            ele2.FindElement(By.LinkText("Summary")).Click();
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            Assert.Equal("Summary", driver.Title);
        }

        [Trait("unit-test", "Senario2Test1")]
        [Fact]
        public void LoadSummaryPage()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            driver.Navigate().GoToUrl(conduciveAppUrl);

            this.ChangeTimeRange();

            //click on the center panel color block
            var svg = driver.FindElement(By.XPath("//*[name()='svg']"));

            //wait for data page to finish loading
            System.Threading.Thread.Sleep(5000);

            this.VerifyClickOnTrendChartLegendItem(svg);

            //this.TestClickOnTopUser();

            Console.WriteLine("total spend " + watch.Elapsed.TotalSeconds);
        }

        [Trait("unit-test", "ClickOnTrendChart")]
        [Fact]
        public void ClickOnTrendChart()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            driver.Navigate().GoToUrl(conduciveAppUrl);
            this.ChangeTimeRange();

            //click on the center panel color block
            var svg = driver.FindElement(By.XPath("//*[name()='svg']"));
            while (svg == null)
            {
                System.Threading.Thread.Sleep(100);
            }

            this.VerifyClickOnTrendChart(svg);

            Console.WriteLine("total spend " + watch.Elapsed.TotalSeconds);
        }

        [Trait("unit-test", "UserDetails")]
        [Fact]
        public void UserDetails()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            driver.Navigate().GoToUrl(conduciveAppUrl);

            this.ChangeTimeRange();

            //click on the center panel color block
            var svg = driver.FindElement(By.XPath("//*[name()='svg']"));
            System.Threading.Thread.Sleep(5000);

            this.TestClickOnTopUsers();

            Console.WriteLine("total spend " + watch.Elapsed.TotalSeconds);
        }

        [Trait("unit-test", "UserDetails")]
        [Fact]
        public void DocumentDetails()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            driver.Navigate().GoToUrl(conduciveAppUrl);

            this.ChangeTimeRange();

            //click on the center panel color block
            var svg = driver.FindElement(By.XPath("//*[name()='svg']"));
            System.Threading.Thread.Sleep(5000);

            this.TestClickOnTopDocuments();

            Console.WriteLine("total spend " + watch.Elapsed.TotalSeconds);
        }

        [Trait("unit-test", "ClickonSwimline")]
        [Fact]
        public void ClickonSwimline()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            try
            {
                driver.Navigate().GoToUrl("http://nvd3.org/examples/multiBar.html");
                var svg = driver.FindElement(By.TagName("svg"));
                var switches = svg.FindElements(By.ClassName("nv-series"));
                foreach (var swi in switches)
                {
                    swi.Click();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            Console.WriteLine("total spend " + watch.Elapsed.TotalSeconds);
        }

        private void ChangeTimeRange()
        {
            //open "time Range"
            driver.FindElement(By.ClassName("time-label")).Click();
            IReadOnlyCollection<IWebElement> searchCriterias = driver.FindElements(By.ClassName("accordion-toggle"));
            searchCriterias.ElementAt(3).Click();
            Console.WriteLine("Open time selection dialog succeed");

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
            this.SendInput("08/07/2014", earliestDate);
            var latestDate = e4.FindElement(By.ClassName("timerangepicker-latest-date"));
            this.SendInput("08/11/2014", latestDate);

            //click on apply button
            var e11 = e4.FindElement(By.ClassName("apply"));
            e11.Click();
            Console.WriteLine("Change time selection succeed");
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

        private void VerifyTopUserTable()
        {
            var userTable = driver.FindElement(By.Id("user-table"));
            IReadOnlyCollection<IWebElement> topUsers = userTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
            this.VerifyTopReturnsSortedCorrectly(topUsers.Select(a => a.Text));
            //Console.WriteLine("Verify top user table succeed");
        }

        private void VerifyTopDocumentTable()
        {
            var documentTable = driver.FindElement(By.Id("document-table"));
            var topDocuments = documentTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
            this.VerifyTopReturnsSortedCorrectly(topDocuments.Select(a => a.Text));
            //Console.WriteLine("Verify top document table succeed");
        }

        private void VerifyClickOnTrendChart(IWebElement svg)
        {
            var allActionBars = svg.FindElements(By.ClassName("highcharts-series"));

            var barElementCoordinates = new List<Tuple<int, int>>();
            IWebElement barUnit = null;
            for (int i = 0; i < allActionBars.Count; i++)
            {
                Console.WriteLine("Verify allActionBars[{0}]", i);
                barUnit = allActionBars[i].FindElement(By.ClassName("highcharts-tracker"));
                barElementCoordinates = this.TryClickOnEachBarElement(allActionBars[i]);
            }

            Actions action = new Actions(driver);
            action.MoveToElement(barUnit, barElementCoordinates[0].Item1, barElementCoordinates[0].Item2).Click().Perform();
            Console.WriteLine("Click on the the central chart element succeed");
        }

        private void VerifyClickOnTrendChartLegendItem(IWebElement svg)
        {
            var highchartsLengendItems = svg.FindElements(By.ClassName("highcharts-legend-item"));
            Console.WriteLine("Load the central chart lengend succeed");

            for (int i = 0; i < highchartsLengendItems.Count; i++)
            {
                var element = highchartsLengendItems[i].FindElement(By.TagName("rect"));
                element.Click();
                this.VerifyTopUserTable();
                this.VerifyTopDocumentTable();
            }

            Console.WriteLine("Verify ClickOnTrendChartLegendItem succeed");
        }

        private List<Tuple<int, int>> TryClickOnEachBarElement(IWebElement element)
        {
            List<Tuple<int, int>> ret = new List<Tuple<int, int>>();
            var u = element.FindElement(By.ClassName("highcharts-tracker"));

            Actions action = new Actions(driver);
            var path = u.GetAttribute("d");

            List<string> coordinate = this.BarCoordinate(path);
            foreach (string str in coordinate)
            {
                string str1 = str.Replace("M", "").Replace("L", "").TrimStart(' ');
                try
                {
                    int x = int.Parse(str1.Split(' ')[0]);
                    int y = int.Parse(str1.Split(' ')[1]);

                    action.MoveToElement(u, x, y).Perform();
                    ret.Add(new Tuple<int, int>(x, y));
                    //action.MoveToElement(u, x, y).Click().Perform();
                    //action.MoveToElement(u, x, y).Click().Build().Perform();
                }
                catch (Exception e)
                {

                }
            }

            return ret;
        }

        private List<string> BarCoordinate(string path)
        {
            List<string> ret = new List<string>();
            var barColumns = path.Split('Z');
            foreach (string str in barColumns)
            {
                string str1 = str;
                if (str.StartsWith("M"))
                {
                    str1 = str.Replace("M", "L");
                }

                var fourlines = str.Replace("M ", "").Split('L');
                try
                {
                    string line1 = fourlines[0].Split(' ')[2];
                    string line2 = fourlines[1].Split(' ')[2];
                    string line3 = fourlines[2].Split(' ')[2];
                    string line4 = fourlines[3].Split(' ')[2];
                    if (line1 == line4)
                    {
                        break;
                    }
                    else
                    {
                        ret.Add(str);
                    }
                }
                catch (Exception e)
                { }
            }

            return ret;
        }

        private void TestClickOnTopUsers()
        {
            var userTable = driver.FindElement(By.Id("user-table"));
            IReadOnlyCollection<IWebElement> tops = userTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));

            Random rand = new Random();
            int index = rand.Next(0, tops.Count);
            tops.ElementAt(index).FindElements(By.ClassName("numeric"))[0].Click();
            System.Threading.Thread.Sleep(8000);

            //verify user-event-document page
            var topEventsTable = driver.FindElement(By.Id("events-table"));
            var topEvents = topEventsTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
            this.VerifyTopReturnsSortedCorrectly(topEvents.Select(a => a.Text));
        }

        private void TestClickOnTopDocuments()
        {
            var userTable = driver.FindElement(By.Id("document-table"));
            IReadOnlyCollection<IWebElement> tops = userTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));

            Random rand = new Random();
            int index = rand.Next(0, tops.Count);
            tops.ElementAt(index).FindElements(By.ClassName("numeric"))[0].Click();
            System.Threading.Thread.Sleep(8000);

            //verify user-event-document page
            var topEventsTable = driver.FindElement(By.Id("events-table"));
            var topEvents = topEventsTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
            this.VerifyTopReturnsSortedCorrectly(topEvents.Select(a => a.Text));
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

        private void LoadSplunkHomePageAndSignIn()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // set the timeout after page load to 30seconds
            driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 30));
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            Console.WriteLine("load browers taks" + watch.Elapsed.TotalSeconds);

            watch.Reset();
            driver.Navigate().GoToUrl(splunkHomeUrl);
            Console.WriteLine("load page takes " + watch.Elapsed.TotalSeconds);
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
        }
    }
}