using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
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
        private static string conduciveSummaryUrl = splunkServerUrl + "dj/en-us/warum_conducive_web/Summary/";
        private static string conduciveSuspicousDocAccUrl = splunkServerUrl + "dj/en-us/warum_conducive_web/suspicious_document_access/";
        private static string conduciveTerminatedEmployeeUrl = splunkServerUrl + "dj/en-us/warum_conducive_web/terminated_employees/";
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

        [Trait("unit-test", "LoadSummaryPage")]
        [Fact]
        public void Summary()
        {
            this.LoadSubitemPageFromSplunkhomePage("Summary", "Summary");
            this.ChangeTimeRangeNew();
            var svg = GetSvgInSummaryPage();
            //this.VerifyClickOnTrendChartLegendItem(svg);

            this.TestClickOnTopUserOrDocuments("user-table");
            this.RemoveUserFilter();

            this.TestClickOnTopUserOrDocuments("document-table");
            this.RemoveUserFilter();
        }


        [Trait("unit-test", "UserDetails")]
        [Fact]
        public void UserDetails()
        {
            this.LoadSubitemPageFromSplunkhomePage("User Details", "User Details");

            //wait let the data load for no search criteria
            //System.Threading.Thread.Sleep(5000);

            var userInput = driver.FindElement(By.Id("userInput")).FindElement(By.Id("userInput-input"));
            this.SendInput("rblack", userInput);
            this.ChangeTimeRangeNew();

            this.Verify_UserOrDocument_Details_Page("user");
        }

        //[Trait("unit-test", "ClickOnTrendChart")]
        //[Fact]
        //public void ClickOnTrendChart()
        //{
        //    //driver.Navigate().GoToUrl(conduciveSummaryUrl);
        //    //this.ChangeTimeRangeNew();
        //    //var svg = GetSvgInSummaryPage();
        //    //this.VerifyClickOnTrendChart(svg);
        //}

        [Trait("unit-test", "DocumentAccess")]
        [Fact]
        public void DocumentAccess()
        {
            this.LoadDocumentAccessPage();
            System.Threading.Thread.Sleep(1000);

            this.VerifyAnomalousActivityPage();
            System.Threading.Thread.Sleep(1000);

            this.VerifySuspiciousDocumentAccessPage();
            System.Threading.Thread.Sleep(1000);

            this.VerifyTerminatedEmployeePage();
        }

        private void RemoveUserFilter()
        {
            var filter = driver.FindElement(By.CssSelector(".tag.label.filter"));
            var ele = filter.FindElement(By.TagName("span"));
            ele.Click();
            this.VerifySummaryPage();
        }

        private void VerifyTerminatedEmployeePage()
        {
            driver.FindElement(By.LinkText("Suspicous Document Access")).Click();
            driver.FindElement(By.LinkText("Terminated Employees")).Click();

            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            this.ChangeTimeRange();

            //click on the center panel color block
            watch.Restart();
            bool loaded = false;
            Exception ex = null;
            do
            {
                try
                {
                    //svg = driver.FindElement(By.XPath("//*[name()='svg']"));
                    System.Threading.Thread.Sleep(3000);
                    loaded = true;
                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load Terminated-employee page Center Chart, {1}", DateTime.Now, watch.Elapsed.TotalSeconds));
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load Terminated-employee page Center Chart, {2}, !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10));
                if (ex != null)
                {
                    throw ex;
                }
            }
        }

        private void VerifySuspiciousDocumentAccessPage()
        {
            driver.FindElement(By.LinkText("Suspicous Document Access")).Click();
            driver.FindElement(By.LinkText("Off Hours")).Click();
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            this.ChangeTimeRange();

            //click on the center panel color block
            watch.Restart();
            bool loaded = false;
            Exception ex = null;
            do
            {
                try
                {
                    driver.FindElement(By.XPath("//*[name()='svg']"));
                    loaded = true;
                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load Suspicous-doc-access page Center Chart, {1}", DateTime.Now, watch.Elapsed.TotalSeconds));
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load Suspicous-doc-access page Center Chart, {2}, !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10));
                if (ex != null)
                {
                    throw ex;
                }
            }
        }

        private void VerifyAnomalousActivityPage()
        {
            var multplier = driver.FindElement(By.Id("multiplier_input-input"));
            this.SendInput("0.05", multplier);
            this.ChangeTimeRange();

            watch.Restart();
            bool loaded = false;
            Exception ex = null;

            do
            {
                try
                {
                    //check result show up
                    var table = driver.FindElement(By.Id("main-table"));
                    var rows = table.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
                    Assert.True(rows.Count > 0);

                    loaded = true;
                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load Anomalous-doc-access page, {1} ", DateTime.Now, watch.Elapsed.TotalSeconds));
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load Anomalous-doc-access page, {2} , !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10));
                if (ex != null)
                {
                    throw ex;
                }
            }
        }

        private void LoadDocumentAccessPage()
        {
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            if (driver.Url != splunkHomeUrl)
            {
                driver.Navigate().GoToUrl(splunkHomeUrl);
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            }

            watch.Restart();
            bool loaded = false;
            Exception ex = null;

            do
            {
                try
                {
                    driver.FindElement(By.LinkText("Suspicous Document Access")).Click();
                    driver.FindElement(By.LinkText("Anomalous Activity")).Click();
                    wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                    loaded = true;
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);
        }

        private void LoadSubitemPageFromSplunkhomePage(string linkText, string subItemPageTitle)
        {
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            if (driver.Url != splunkHomeUrl)
            {
                driver.Navigate().GoToUrl(splunkHomeUrl);
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            }

            watch.Restart();
            bool loaded = false;
            Exception ex = null;

            do
            {
                try
                {
                    driver.FindElement(By.LinkText(linkText)).Click();
                    wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                    loaded = true;
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            Assert.Equal(subItemPageTitle, driver.Title);
        }

        private IWebElement GetSvgInSummaryPage()
        {
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
                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load Summary page Center Chart, {1}", DateTime.Now, watch.Elapsed.TotalSeconds));
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load Summary page Center Chart, {2} , !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10));
                if (ex != null)
                {
                    throw ex;
                }
            }

            return svg;
        }

        private void ChangeTimeRange()
        {
            bool loaded = false;
            watch.Restart();
            Exception ex = null;
            do
            {
                try
                {
                    //open "time Range"
                    driver.FindElement(By.ClassName("time-label")).Click();
                    IReadOnlyCollection<IWebElement> searchCriterias = driver.FindElements(By.ClassName("accordion-toggle"));
                    searchCriterias.ElementAt(3).Click();
                    loaded = true;
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                if (ex != null)
                {
                    throw ex;
                }
            }

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

        private void ChangeTimeRangeNew()
        {
            bool loaded = false;
            watch.Restart();
            Exception ex = null;
            do
            {
                try
                {
                    //open "time picker"
                    var timepicker = driver.FindElement(By.Id("timepicker"));
                    var timelabel = timepicker.FindElement(By.ClassName("time-label"));
                    timelabel.Click();
                    loaded = true;
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                if (ex != null)
                {
                    throw ex;
                }
            }

            //open "Date Range" dropdown option
            var timepickerview = driver.FindElement(By.CssSelector(".accordion.view-new-time-range-picker-dialog.shared-timerangepicker-dialog"));
            var timepicerviewGroups = timepickerview.FindElements(By.ClassName("accordion-group"));
            // select the "Date Range" dropdown option
            timepicerviewGroups[2].FindElement(By.ClassName("accordion-toggle")).Click();

            //change the date in the "Date Range" option
            var dateRange = driver.FindElement(By.CssSelector(".accordion-inner.shared-timerangepicker-dialog-daterange"));
            var dateRangeBtn = dateRange.FindElement(By.CssSelector(".dropdown-toggle.btn"));
            //open "Before between after" button
            dateRangeBtn.FindElement(By.ClassName("link-label")).Click();

            var e7 = driver.FindElement(By.CssSelector(".dropdown-menu.dropdown-menu-selectable.dropdown-menu-narrow.open"));
            var e8 = e7.FindElements(By.ClassName("link-label"));
            //select the "between" buttion
            e8[0].Click();

            //change the date value
            var e3 = driver.FindElement(By.CssSelector(".accordion-group.active"));
            var e4 = e3.FindElement(By.ClassName("accordion-body"));
            var earliestDate = e4.FindElement(By.CssSelector(".timerangepicker-earliest-date.hasDatepicker"));
            this.SendInput("08/07/2014", earliestDate);
            var latestDate = e4.FindElement(By.CssSelector(".timerangepicker-latest-date.hasDatepicker"));
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
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));

            for (int i = 0; i < highchartsLengendItems.Count; i++)
            {
                var element = highchartsLengendItems[i].FindElement(By.TagName("rect"));
                element.Click();

                watch.Restart();
                this.VerifySummaryPage();
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

        private void VerifySummaryPage()
        {
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));
            bool loaded = false;
            Exception ex = null;
            watch.Restart();

            do
            {
                try
                {
                    Task task1 = Task.Factory.StartNew(() => this.VerifyTopTableLoaded("user-table"));
                    Task task2 = Task.Factory.StartNew(() => this.VerifyTopTableLoaded("document-table"));

                    task1.Wait();
                    task2.Wait();

                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load Summary page, {1}", DateTime.Now, watch.Elapsed.TotalSeconds));
                    loaded = true;
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load Summary page, {2}, !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10));
                if (ex != null)
                {
                    throw ex;
                }
            }

            //this.Verify_UserOrDocument_Details_Page("Document");
            //this.VerifyTopReturnsSortedCorrectly(topEvents.Select(a => a.Text));
        }

        private void VerifyTopTableLoaded(string tableName)
        {
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));
            bool loaded = false;
            Exception ex = null;
            watch.Restart();

            do
            {
                try
                {
                    var table = driver.FindElement(By.Id(tableName));
                    IReadOnlyCollection<IWebElement> tops = table.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
                    Assert.True(tops.Count > 0);

                    Random rand = new Random();
                    int index = rand.Next(0, tops.Count);
                    //click "include" dropdown menu
                    Assert.True(tops.ElementAt(index).FindElement(By.ClassName("numeric")) != null);

                    //this will fail as results keep updating
                    this.VerifyTopReturnsSortedCorrectly(tops.Select(a => a.Text));

                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load {2} page, {1}", DateTime.Now, watch.Elapsed.TotalSeconds, tableName));
                    loaded = true;
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load {2} page, {2}, !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10, tableName));
                if (ex != null)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Verify user(document)_details page
        /// </summary>
        /// <param name="msg">specify "user" or "document"</param>
        private void Verify_UserOrDocument_Details_Page(string msg)
        {
            bool loaded = false;
            Exception ex = null;
            watch.Restart();

            do
            {
                try
                {
                    IWebElement x_axis;

                    //find the SVG of "zoom-chart-div"
                    IWebElement zoomChart = driver.FindElement(By.Id("zoom-chart-div"));
                    x_axis = zoomChart.FindElements(By.ClassName("highcharts-axis-labels"))[0];
                    Assert.Contains("Thu Aug 72014", x_axis.Text);
                    Assert.Contains("Mon Aug 11", x_axis.Text);


                    //find the SVG of "trend-chart-div"
                    IWebElement trendChart = driver.FindElement(By.Id("trend-chart-div"));
                    x_axis = zoomChart.FindElements(By.ClassName("highcharts-axis-labels"))[0];
                    Assert.Contains("Thu Aug 72014", x_axis.Text);
                    Assert.Contains("Mon Aug 11", x_axis.Text);

                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load {2} Details page trendChart and zoomChart, {1}", DateTime.Now, watch.Elapsed.TotalSeconds, msg));
                    loaded = true;
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load {2} Details page trendChart and zoomChart, {1}, !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10, msg));
                if (ex != null)
                {
                    throw ex;
                }
            }

            this.VerifyClickOnTopEventOnUserOrDocumentDetailPage("user");
        }

        private void VerifyClickOnTopEventOnUserOrDocumentDetailPage(string msg)
        {
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));
            bool loaded = false;
            Exception ex = null;

            //click on on top events
            do
            {
                try
                {
                    var table = driver.FindElement(By.Id("events-table"));
                    IReadOnlyCollection<IWebElement> tops = table.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
                    Random rand = new Random();
                    int index = rand.Next(0, tops.Count);
                    tops.ElementAt(index).FindElement(By.ClassName("numeric")).Click();
                    loaded = true;
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            // verify the top event raw table loaded
            loaded = false;
            ex = null;
            watch.Restart();
            do
            {
                try
                {
                    //click "include" dropdown menu
                    var rawTableDiv = driver.FindElement(By.Id("raw_container"));
                    var results = rawTableDiv.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
                    Assert.True(results.Count > 0);

                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load top event on {2}-detail page, {1}", DateTime.Now, watch.Elapsed.TotalSeconds, msg));
                    loaded = true;
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load top event on {3}-detail page, {2}, !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10, msg));
                if (ex != null)
                {
                    throw ex;
                }
            }
        }

        private void TestClickOnTopUserOrDocuments(string userOrDocument)
        {
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));
            bool loaded = false;
            Exception ex = null;
            watch.Restart();

            do
            {
                try
                {
                    this.VerifyTopTableLoaded(userOrDocument);
                    var table = driver.FindElement(By.Id(userOrDocument));
                    IReadOnlyCollection<IWebElement> tops = table.FindElements(By.ClassName("shared-resultstable-resultstablerow"));
                    Random rand = new Random();
                    int index = rand.Next(0, tops.Count);

                    //click "include" dropdown menu
                    tops.ElementAt(index).FindElement(By.ClassName("numeric")).Click();
                    var dropdownmenu = driver.FindElements(By.CssSelector(".dropdown-menu.dropdown-context"));
                    foreach (var x in dropdownmenu)
                    {
                        if (x.Displayed == true)
                        {
                            x.FindElements(By.ClassName("context-event"))[0].Click();
                            break;
                        }
                    }

                    this.GetSvgInSummaryPage();
                    this.VerifySummaryPage();

                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load {2} Details page, {1}", DateTime.Now, watch.Elapsed.TotalSeconds, userOrDocument));
                    loaded = true;
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Load {3} Details page, {2}, !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10, userOrDocument));
                if (ex != null)
                {
                    throw ex;
                }
            }
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

        private void RunAppSetup()
        {
            this.LoadSubitemPageFromSplunkhomePage("Setup", "warum_conducive_web Setup");
            var saveButton = driver.FindElement(By.CssSelector(".btn.btn-primary"));
            this.SubmitSetupPage(saveButton);
        }

        private void SubmitSetupPage(IWebElement saveButton)
        {
            watch.Restart();
            bool loaded = false;
            Exception ex = null;
            do
            {
                try
                {
                    IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                    saveButton.Click();
                    wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                    loaded = true;
                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Submit App setup page, {1} ", DateTime.Now, watch.Elapsed.TotalSeconds));
                }
                catch (Exception e) { ex = e; }
            } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

            if (!loaded)
            {
                logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, Submit App setup page, {2} , !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10));
                if (ex != null)
                {
                    throw ex;
                }
            }

            Assert.Equal("Summary", driver.Title);
        }

        private void LoadSplunkHomePageAndSignIn()
        {
            watch.Restart();

            // set the timeout after page load to 30seconds
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

            Console.WriteLine("now Need to run setup page");

            this.RunAppSetup();
        }
    }
}