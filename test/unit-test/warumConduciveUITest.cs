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
using System.IO;

namespace unit_test
{
    public class MyFixture : IDisposable
    {
        public IWebDriver Driver
        {
            get;
            internal set;
        }

        public List<string> Logs
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
            this.Logs = new List<string>();
            Logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, ============================== New Test Run Start ==============================", System.DateTime.Now));
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

            Logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, ============================== End Test Run ==============================", System.DateTime.Now));
            string userHomePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            Console.WriteLine(userHomePath);
            StreamWriter logFile = new StreamWriter(userHomePath + "\\" + "WarumTestPerfLog.txt", true);
            Logs.ForEach(a => logFile.WriteLine(a));
            logFile.Dispose();
            logFile.Close();
        }
    }

    /// <summary>
    /// Tests the UI
    /// </summary>
    public class WarumConduciveUITest : IUseFixture<MyFixture>, IDisposable
    {
        private static IWebDriver driver = null;
        private static string splunkServerUrl = "http://localhost:8000/";
        private static string conduciveAppUrl = splunkServerUrl + "dj/en-us/warum_conducive_web/Summary/";
        private static string splunkHomeUrl = splunkServerUrl + "en-US/app/launcher/home";
        private static bool firstTestRun = false;
        private static List<string> logs = null;
        private static Stopwatch watch = new Stopwatch();
        private const int timeoutThreshold = 90; // seconds

        public void SetFixture(MyFixture data)
        {
            if (!firstTestRun)
            {
                driver = data.Driver;
                logs = data.Logs;
                this.LoadSplunkHomePageAndSignIn();
                firstTestRun = true;
                watch.Start();
            }
        }

        public void Dispose()
        {
        }

        [Trait("unit-test", "LoadAppHomePage")]
        [Fact]
        public void LoadAppHomePage()
        {
            driver.Navigate().GoToUrl(splunkHomeUrl);
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            watch.Restart();
            bool loaded = false;
            Exception ex = null;
            do
            {
                try
                {
                    var ele1 = driver.FindElement(By.CssSelector(".app-wrapper.appName-warum_conducive_web"));
                    var ele2 = ele1.FindElement(By.CssSelector(".slideNavPlaceHolder.group.app-slidenav"));
                    ele2.FindElement(By.LinkText("Summary")).Click();
                    wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                    loaded = true;
                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, 1. Load App page = {1} ", DateTime.Now, watch.Elapsed.TotalSeconds));
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, 1. Load App page = {2} , !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10));
                if (ex != null)
                {
                    throw ex;
                }
            }

            Assert.Equal("Summary", driver.Title);
        }

        [Trait("unit-test", "LoadSummaryPage")]
        [Fact]
        public void LoadSummaryPage()
        {
            var svg = GetSvgInSummaryPage();
            this.VerifyClickOnTrendChartLegendItem(svg);
        }

        [Trait("unit-test", "ClickOnTrendChart")]
        [Fact]
        public void ClickOnTrendChart()
        {
            var svg = GetSvgInSummaryPage();
            this.VerifyClickOnTrendChart(svg);
        }

        [Trait("unit-test", "UserDetails")]
        [Fact]
        public void UserDetails()
        {
            var svg = GetSvgInSummaryPage();
            this.TestClickOnTopUsers();
        }

        [Trait("unit-test", "DocumentDetails")]
        [Fact]
        public void DocumentDetails()
        {
            var svg = GetSvgInSummaryPage();
            this.TestClickOnTopDocuments();
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

        private IWebElement GetSvgInSummaryPage()
        {
            driver.Navigate().GoToUrl(conduciveAppUrl);
            this.ChangeTimeRange();

            //click on the center panel color block
            watch.Restart();
            bool loaded = false;
            IWebElement svg = null;
            Exception ex = null;
            do
            {
                try
                {
                    svg = driver.FindElement(By.XPath("//*[name()='svg']"));
                    loaded = true;
                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, 2. Load Summary page Center Chart = {1}", DateTime.Now, watch.Elapsed.TotalSeconds));
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, 2. Load Summary page Center Chart = {2} , !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10));
                if (ex != null)
                {
                    throw ex;
                }
            }

            return svg;
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
            watch.Restart();
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

                watch.Restart();

                //verify top user table result returned
                Task task1 = Task.Run(() =>
                {
                    Exception ex = null;
                    bool loaded = false;
                    do
                    {
                        try
                        {
                            var userTable = driver.FindElement(By.Id("user-table"));
                            var x = userTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
                            if (x.Count > 0)
                            {
                                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, 5. Load TopUser after click lengend ({2}) = {1}", DateTime.Now, watch.Elapsed.TotalSeconds,
                                    highchartsLengendItems[i].FindElement(By.TagName("text")).FindElement(By.TagName("tspan")).Text));
                                loaded = true;
                            }
                        }
                        catch (Exception e) { ex = e; }
                    } while (!loaded && watch.Elapsed.TotalSeconds > timeoutThreshold);

                    if (!loaded)
                    {
                        logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, 5. Load TopUser after click lengend ({1}) = {2} , !!!Exception: takes more than {3} seconds",
                            DateTime.Now,
                            highchartsLengendItems[i].FindElement(By.TagName("text")).FindElement(By.TagName("tspan")).Text,
                            timeoutThreshold + 10,
                            timeoutThreshold));
                        if (ex != null)
                        {
                            throw ex;
                        }
                    }
                });

                //verify top document table result returned
                Task task2 = Task.Run(() =>
                {
                    Exception ex = null;
                    bool loaded = false;
                    do
                    {
                        try
                        {
                            var userTable = driver.FindElement(By.Id("document-table"));
                            var x = userTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
                            if (x.Count > 0)
                            {
                                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, 6. Load TopDocument after click lengend ({2}) = {1}", DateTime.Now, watch.Elapsed.TotalSeconds,
                                    highchartsLengendItems[i].FindElement(By.TagName("text")).FindElement(By.TagName("tspan")).Text));
                                loaded = true;
                            }
                        }
                        catch (Exception e) { ex = e; }
                    } while (!loaded && watch.Elapsed.TotalSeconds > timeoutThreshold);

                    if (!loaded)
                    {
                        logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, 6. Load TopDocument after click lengend ({1}) = {2} , !!!Exception: takes more than {3} seconds",
                            DateTime.Now,
                            highchartsLengendItems[i].FindElement(By.TagName("text")).FindElement(By.TagName("tspan")).Text,
                            timeoutThreshold + 10,
                            timeoutThreshold));

                        if (ex != null)
                        {
                            throw ex;
                        }
                    }

                });

                task1.Wait();
                task2.Wait();
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
                    Console.WriteLine(e);
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
                {
                    Console.WriteLine(e);
                }
            }

            return ret;
        }

        private void TestClickOnTopUsers()
        {
            bool loaded = false;
            IReadOnlyCollection<IWebElement> topEvents = null;
            Exception ex = null;
            watch.Restart();
            do
            {
                try
                {
                    var userTable = driver.FindElement(By.Id("user-table"));
                    IReadOnlyCollection<IWebElement> tops = userTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));

                    Random rand = new Random();
                    int index = rand.Next(0, tops.Count);
                    Console.WriteLine(string.Format("Debug info: index={0}, tops.count={1}", index, tops.Count));
                    tops.ElementAt(index).FindElements(By.ClassName("numeric"))[0].Click();
                    //verify user-event-document page
                    var topEventsTable = driver.FindElement(By.Id("events-table"));
                    topEvents = topEventsTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, 4. Load User Details page = {1}", DateTime.Now, watch.Elapsed.TotalSeconds));
                    loaded = true;
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds > timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, 4. Load User Details page = {1}, !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10));
                if (ex != null)
                {
                    throw ex;
                }
            }

            this.VerifyTopReturnsSortedCorrectly(topEvents.Select(a => a.Text));
          
        }

        private void TestClickOnTopDocuments()
        {
            var userTable = driver.FindElement(By.Id("document-table"));

            bool loaded = false;
            IReadOnlyCollection<IWebElement> topEvents = null;
            Exception ex = null;
            watch.Restart();

            do
            {
                try
                {
                    IReadOnlyCollection<IWebElement> tops = userTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
                    Random rand = new Random();
                    int index = rand.Next(0, tops.Count);
                    Console.WriteLine(string.Format("Debug info: index={0}, tops.count={1}", index, tops.Count));
                    tops.ElementAt(index).FindElements(By.ClassName("numeric"))[0].Click();

                    //verify user-event-document page
                    var topEventsTable = driver.FindElement(By.Id("events-table"));
                    topEvents = topEventsTable.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, 3. Load Document Details page = {1}", DateTime.Now, watch.Elapsed.TotalSeconds));
                    loaded = true;
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds > timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, 3. Load Document Details page = {2}, !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10));
                if (ex != null)
                {
                    throw ex;
                }
            }
            
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
            watch.Restart();

            // set the timeout after page load to 30seconds
            driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 30));
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            Console.WriteLine("load browers taks" + watch.Elapsed.TotalSeconds);

            watch.Restart();
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