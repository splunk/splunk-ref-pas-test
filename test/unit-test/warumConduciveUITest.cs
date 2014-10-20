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

        [Trait("unit-test", "Summary")]
        [Fact]
        public void Summary()
        {
            this.LoadSubItemPageFromSplunkhomePage("Summary", "Summary");
            this.ChangeTimeRangeNew();

            this.VerifySummaryPageElements();

            this.VerifyClickOnTrendChartLegendItem();

            //click a user/doc drilldown and select included
            this.Verify_ClickFilteredUserOrDocument("user");
            this.Verify_ClickFilteredUserOrDocument("document");
        }

        [Trait("unit-test", "UserDetails")]
        [Fact]
        public void UserDetails()
        {
            this.LoadSubItemPageFromSplunkhomePage("User Details", "User Details");

            var userInput = StartWaitElementAppearTask(By.Id("userInput")).Result.FindElement(By.Id("userInput-input"));
            this.SendInput("rblack", userInput);
            this.ChangeTimeRangeNew();
            this.Verify_UserOrDocument_Details_Page("user");
        }


        [Trait("unit-test", "DocumentDetails")]
        [Fact]
        public void DocumentDetails()
        {
            this.LoadSubItemPageFromSplunkhomePage("Document Details", "Document Details");

            var userInput = StartWaitElementAppearTask(By.Id("userInput")).Result.FindElement(By.Id("userInput-input"));
            this.SendInput("*4799649936474595.dmgr", userInput);
            this.ChangeTimeRangeNew();

            this.Verify_UserOrDocument_Details_Page("document");
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
            this.VerifyAnomalousActivityPage();
            this.VerifySuspiciousDocumentAccessPage();
            this.VerifyTerminatedEmployeePage();
        }

        /// <summary>
        /// Create a task to wait a web elment to show up, use this function if expect the item take some time to show up in the page
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="byMethod"></param>
        /// <param name="logMsg"></param>
        /// <returns></returns>
        private Task<IWebElement> StartWaitElementAppearTask(IWebElement parentElement, By byMethod, string logMsg)
        {
            watch.Restart();
            bool loaded = false;
            Exception ex = null;

            if (parentElement == null)
            {
                parentElement = driver.FindElement(By.TagName("html"));
            }

            return Task.Factory.StartNew(() =>
            {
                IWebElement result = null;
                do
                {
                    try
                    {
                        result = parentElement.FindElement(byMethod);
                        Assert.NotNull(result);

                        if (string.IsNullOrEmpty(logMsg))
                        {
                            logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, {2}, {1}", DateTime.Now, watch.Elapsed.TotalSeconds, logMsg));
                        }

                        loaded = true;
                    }
                    catch (Exception e) { ex = e; }
                } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

                if (!loaded)
                {
                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss},{3}, {2}, !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10, logMsg));
                    if (ex != null)
                    {
                        throw new Exception(string.Format("Can't find {0} by {1} for {2}", parentElement.ToString(), byMethod.ToString(), logMsg), ex);
                    }
                }

                return result;
            });
        }

        /// <summary>
        /// same as StartWaitElementAppearTask except return a collections of IWebElement
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="byMethod"></param>
        /// <param name="logMsg"></param>
        /// <returns></returns>
        private Task<IReadOnlyCollection<IWebElement>> StartWaitElementsAppearTask(IWebElement parentElement, By byMethod, string logMsg)
        {
            watch.Restart();
            bool loaded = false;
            Exception ex = null;
            IReadOnlyCollection<IWebElement> result = null;

            if (parentElement == null)
            {
                parentElement = driver.FindElement(By.TagName("html"));
            }

            return Task.Factory.StartNew(() =>
            {
                do
                {
                    try
                    {
                        result = parentElement.FindElements(byMethod).ToList();
                        Assert.True(result.Count > 0);

                        if (string.IsNullOrEmpty(logMsg))
                        {
                            logs.Add(string.Format("{0:MM/dd/yy H:mm:ss}, {2}, {1}", DateTime.Now, watch.Elapsed.TotalSeconds, logMsg));
                        }

                        loaded = true;
                    }
                    catch (Exception e) { ex = e; }
                } while (!loaded && watch.Elapsed.TotalSeconds < timeoutThreshold);

                if (!loaded)
                {
                    logs.Add(string.Format("{0:MM/dd/yy H:mm:ss},{3}, {2}, !!!Exception: takes more than {1} seconds", DateTime.Now, timeoutThreshold, timeoutThreshold + 10, logMsg));
                    if (ex != null)
                    {
                        throw new Exception(string.Format("Can't find the group of {0} by {1} for {2}", parentElement.ToString(), byMethod.ToString(), logMsg), ex);
                    }
                }

                return result;
            });
        }

        private Task<IReadOnlyCollection<IWebElement>> StartWaitElementsAppearTask(By byMethod)
        {
            var parentElement = driver.FindElement(By.TagName("html"));
            return this.StartWaitElementsAppearTask(parentElement, byMethod);
        }

        private Task<IReadOnlyCollection<IWebElement>> StartWaitElementsAppearTask(By byMethod, string logMsg)
        {
            var parentElement = driver.FindElement(By.TagName("html"));
            return this.StartWaitElementsAppearTask(parentElement, byMethod, logMsg);
        }

        private Task<IReadOnlyCollection<IWebElement>> StartWaitElementsAppearTask(IWebElement parentElement, By byMethod)
        {
            return this.StartWaitElementsAppearTask(parentElement, byMethod, string.Empty);
        }

        private Task<IWebElement> StartWaitElementAppearTask(By byMethod)
        {
            var parentElement = driver.FindElement(By.TagName("html"));
            return this.StartWaitElementAppearTask(parentElement, byMethod);
        }

        private Task<IWebElement> StartWaitElementAppearTask(By byMethod, string logMsg)
        {
            var parentElement = driver.FindElement(By.TagName("html"));
            return this.StartWaitElementAppearTask(parentElement, byMethod, logMsg);
        }

        private Task<IWebElement> StartWaitElementAppearTask(IWebElement parentElement, By byMethod)
        {
            return this.StartWaitElementAppearTask(parentElement, byMethod, string.Empty);
        }

        private void VerifySummaryPageElements()
        {
            //wait central chart to show up
            var svgTask = StartWaitElementAppearTask(By.XPath("//*[name()='svg']"), "load Summary page central chart");

            //wait Policy violation show up
            var policyViolation = StartWaitElementAppearTask(By.Id("single-value")).Result;
            var policyViolationValueTask = StartWaitElementAppearTask(policyViolation, By.ClassName("single-result"), "summary page policy violation");

            //wait top-users table show up
            var userTable = StartWaitElementAppearTask(By.Id("user-table")).Result;
            var topUsersTask = StartWaitElementsAppearTask(userTable, By.ClassName("shared-resultstable-resultstablerow"), "load summary page top-user table");

            //wait top-documents table show up
            var documentTable = StartWaitElementAppearTask(By.Id("document-table")).Result;
            var topDocumentsTask = StartWaitElementsAppearTask(documentTable, By.ClassName("shared-resultstable-resultstablerow"), "load summary page top-documents table");

            policyViolationValueTask.Wait();
            topUsersTask.Wait();
            svgTask.Wait();
            topDocumentsTask.Wait();
        }

        private void Verify_ClickFilteredUserOrDocument(string userOrDoc)
        {
            //click on one of the top-users, should direct to another summary page with filtered user
            var userTable = StartWaitElementAppearTask(By.Id(string.Format("{0}-table", userOrDoc))).Result;
            SelectDrillDown(userTable);

            //verify the summary page with filtered user
            var filteredUser = StartWaitElementAppearTask(By.ClassName("filter_tags"), string.Format("load summary page with filtered {0}", userOrDoc)).Result;
            filteredUser = StartWaitElementAppearTask(filteredUser, By.CssSelector(".tag.label.filter")).Result;

            Assert.True(filteredUser.Text.ToUpper().Contains(string.Format("FILTER {0} IS", userOrDoc == "user" ? userOrDoc.ToUpper() : "OBJECT")));
            this.VerifySummaryPageElements();

            //remove filtered user
            this.RemoveUserOrDocFilter();
            this.VerifySummaryPageElements();
        }

        private void RemoveUserOrDocFilter()
        {
            var filter = driver.FindElement(By.CssSelector(".tag.label.filter"));
            var ele = filter.FindElement(By.TagName("span"));
            ele.Click();
        }

        private void VerifyTerminatedEmployeePage()
        {
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            StartWaitElementAppearTask(By.LinkText("Suspicous Document Access")).Result.Click();
            StartWaitElementAppearTask(By.LinkText("Terminated Employees")).Result.Click();
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            this.ChangeTimeRange();
            StartWaitElementAppearTask(By.XPath("//*[name()='svg']"), "Load Terminated-employee page Center Chart").Wait();
        }

        private void VerifySuspiciousDocumentAccessPage()
        {
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            StartWaitElementAppearTask(By.LinkText("Suspicous Document Access")).Result.Click();
            StartWaitElementAppearTask(By.LinkText("Off Hours")).Result.Click();
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            this.ChangeTimeRange();
            StartWaitElementAppearTask(By.XPath("//*[name()='svg']"), "Load Suspicous-doc-access page Center Chart").Wait();
        }

        private void VerifyAnomalousActivityPage()
        {
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            StartWaitElementAppearTask(By.LinkText("Anomalous Activity")).Result.Click();
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            var multplier = StartWaitElementAppearTask(By.Id("multiplier_input-input")).Result;
            this.SendInput("0.05", multplier);
            this.ChangeTimeRange();

            var table = StartWaitElementAppearTask(By.Id("main-table")).Result;
            var rows = StartWaitElementsAppearTask(table, By.ClassName("shared-resultstable-resultstablerow"), "Load Anomalous-doc-access page").Result;
        }

        private void LoadDocumentAccessPage()
        {
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            if (driver.Url != splunkHomeUrl)
            {
                driver.Navigate().GoToUrl(splunkHomeUrl);
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            }

            StartWaitElementAppearTask(By.LinkText("Suspicous Document Access")).Result.Click();
        }

        private void LoadSubItemPageFromSplunkhomePage(string linkText, string subItemPageTitle)
        {
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            if (driver.Url != splunkHomeUrl)
            {
                driver.Navigate().GoToUrl(splunkHomeUrl);
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            }

            StartWaitElementAppearTask(By.LinkText(linkText)).Result.Click();
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            Assert.Equal(subItemPageTitle, driver.Title);
        }

        private void ChangeTimeRange()
        {
            StartWaitElementAppearTask(By.ClassName("time-label")).Result.Click();
            StartWaitElementsAppearTask(By.ClassName("accordion-toggle")).Result.ElementAt(3).Click();

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
            var timepicker = StartWaitElementAppearTask(By.Id("timepicker")).Result;
            StartWaitElementAppearTask(timepicker, By.ClassName("time-label")).Result.Click();

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

        private void VerifyClickOnTrendChartLegendItem()
        {
            var svg = driver.FindElement(By.XPath("//*[name()='svg']"));

            var highchartsLengendItems = svg.FindElements(By.ClassName("highcharts-legend-item"));

            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));

            for (int i = 0; i < highchartsLengendItems.Count; i++)
            {
                var element = highchartsLengendItems[i].FindElement(By.TagName("rect"));
                element.Click();

                watch.Restart();
                this.VerifySummaryPageElements();
            }
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

        /// <summary>
        /// Verify user(document)_details page
        /// </summary>
        /// <param name="userOrDocument">specify "user" or "document"</param>
        private void Verify_UserOrDocument_Details_Page(string userOrDocument)
        {
            IWebElement webpage = driver.FindElement(By.TagName("html"));
            IWebElement zoomChart = StartWaitElementAppearTask(webpage, By.Id("zoom-chart-div")).Result;
            var zoomchartTask = StartWaitElementsAppearTask(zoomChart, By.ClassName("highcharts-axis-labels"), string.Format("load {0}-details page zoom chart", userOrDocument));
            IWebElement trendChart = StartWaitElementAppearTask(webpage, By.Id("trend-chart-div")).Result;
            var trendchartTask = StartWaitElementsAppearTask(trendChart, By.ClassName("highcharts-axis-labels"), string.Format("load {0}-details page zoom chart", userOrDocument));

            Assert.Contains("Thu Aug 72014", zoomchartTask.Result.ElementAt(0).Text);
            Assert.Contains("Mon Aug 11", zoomchartTask.Result.ElementAt(0).Text);
            Assert.Contains("Thu Aug 72014", trendchartTask.Result.ElementAt(0).Text);
            Assert.Contains("Mon Aug 11", zoomchartTask.Result.ElementAt(0).Text);

            this.VerifyZoomChart();
            this.VerifyClickOnTopEventOnUserOrDocumentDetailPage(userOrDocument);
        }

        private void VerifyZoomChart()
        {
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));
            //the zoomchart series
            var e = StartWaitElementAppearTask(By.Id("zoom-chart-div")).Result;
            var e1 = StartWaitElementAppearTask(e, By.ClassName("highcharts-series")).Result;
            var zoomChartSeries = StartWaitElementsAppearTask(e1, By.TagName("path")).Result.ElementAt(1);
            Actions builder = new Actions(driver);

            //right click zoomchart so that the zoom elements show up, click on some non-affective item to make the right click popup window disappear
            builder.ContextClick(zoomChartSeries)
                .SendKeys(Keys.ArrowDown)
                .SendKeys(Keys.ArrowDown)
                .SendKeys(Keys.ArrowDown)
                .SendKeys(Keys.ArrowDown)
                .SendKeys(Keys.ArrowDown)
                .SendKeys(Keys.ArrowDown)
                .SendKeys(Keys.ArrowDown)
                .SendKeys(Keys.Return)
                .Build()
                .Perform();

            // the data point on the zoom chart
            var e0 = StartWaitElementAppearTask(e, By.ClassName("highcharts-series-group")).Result;
            var e4 = StartWaitElementAppearTask(e0, By.CssSelector(".highcharts-markers.highcharts-tracker")).Result;
            var d = e4.FindElement(By.TagName("path")).GetAttribute("d");
            var moveTo = GetSvgLineCoordinates(d).ElementAt(3);

            builder.DragAndDropToOffset(zoomChartSeries, moveTo.Item1, moveTo.Item2).Perform();
         
            //verify the "reset" shows up when the swim windows is selected.
            StartWaitElementAppearTask(By.ClassName("icon-minus-circle"),"zoom window shown").Wait();
        }

        private IEnumerable<Tuple<int, int>> GetSvgLineCoordinates(string d)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();

            //d example: M 406 30.799999999999997 L 418 30.799999999999997 418 42.8 406 42.8 Z
            d = d.Replace("M", "");
            d = d.Replace("L", "");
            d = d.Replace("Z", "");
            d = d.TrimEnd(' ').TrimStart(' ');
            d = d.Replace("  ", " ");

            string[] values = d.Split(' ');
            int index = 0;
            while (index < values.Length)
            {
                float v1 = 0, v2 = 0;
                if (values[index] == string.Empty)
                {
                    index++;
                }

                v1 = float.Parse(values[index++]);
                v2 = float.Parse(values[index++]);
                result.Add(new Tuple<int, int>((int)v1, (int)v2));
            }

            return result;
        }

        private void VerifyClickOnTopEventOnUserOrDocumentDetailPage(string msg)
        {
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));

            var table = StartWaitElementAppearTask(By.Id("events-table")).Result;
            IReadOnlyCollection<IWebElement> tops = StartWaitElementsAppearTask(table, By.ClassName("shared-resultstable-resultstablerow")).Result;
            Random rand = new Random();
            int index = rand.Next(0, tops.Count);
            tops.ElementAt(index).FindElement(By.ClassName("numeric")).Click();

            var rawTableDiv = StartWaitElementAppearTask(By.Id("raw_container")).Result;
            StartWaitElementsAppearTask(rawTableDiv, By.ClassName("shared-resultstable-resultstablerow"), string.Format("Load top event on {0}-detail page", msg)).Wait();
        }

        private void SelectDrillDown(IWebElement table)
        {
            var topUsers = StartWaitElementsAppearTask(table, By.ClassName("shared-resultstable-resultstablerow")).Result;

            //click on one of the top-users
            Random rand = new Random();
            int index = rand.Next(0, topUsers.Count);

            //click "include" dropdown menu
            topUsers.ElementAt(index).FindElement(By.ClassName("numeric")).Click();
            var dropdownmenu = StartWaitElementsAppearTask(By.CssSelector(".dropdown-menu.dropdown-context")).Result;
            foreach (var x in dropdownmenu)
            {
                if (x.Displayed == true)
                {
                    x.FindElements(By.ClassName("context-event"))[0].Click();
                    break;
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
            this.LoadSubItemPageFromSplunkhomePage("Setup", "warum_conducive_web Setup");
            var saveButton = driver.FindElement(By.CssSelector(".btn.btn-primary"));
            this.SubmitSetupPage(saveButton);
        }

        private void SubmitSetupPage(IWebElement saveButton)
        {
            IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            saveButton.Click();
            wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

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
            driver.Manage().Window.Maximize();

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