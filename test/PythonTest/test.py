from selenium import webdriver
import sys
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.support.wait import WebDriverWait
from selenium.webdriver.common.action_chains import ActionChains
import time
import threading
import datetime
from datetime import timedelta
from time import sleep
from multiprocessing.pool import ThreadPool

##############################################
# Test Summary
##############################################

def Summary():
    driver.get(warumHomeUrl)
    WebDriverWait(driver,30).until(readystate_complete)
    ChangeTimeRange()
    VerifySummaryPageElements()
    #VerifyClickOnTrendChartLegendItem()
    Verify_ClickFilteredUserOrDocument("user");
    Verify_ClickFilteredUserOrDocument("document");


##############################################
# Test UserDetails
##############################################
def UserDetails():
    driver.get(userActivityUrl);
    WebDriverWait(driver,30).until(readystate_complete)

    userInput = StartWaitElementAppearTask(driver, By.CSS_SELECTOR,".splunk-textinput.splunk-view").get()
    userInput=userInput.find_element(By.TAG_NAME,"input")
    #this.SendInput("rblack", userInput);
    ChangeTimeRange();
    Verify_UserOrDocument_Details_Page("panel2");

##############################################
# Test DocumentAccess
##############################################
def DocumentAccess():
    VerifyOffhourDocumentAccessPage()
    VerifyTerminatedEmployeePage()


##############################################
# Verify_UserOrDocument_Details_Page
##############################################
def VerifyOffhourDocumentAccessPage():
    driver.get(offhourDocUrl);
    WebDriverWait(driver,30).until(readystate_complete)
    ChangeTimeRange();
    StartWaitElementAppearTask(driver,By.XPATH,"//*[name()='svg']", "Load Suspicous-doc-access page Center Chart").get()

##############################################
# Verify_UserOrDocument_Details_Page
##############################################
def VerifyTerminatedEmployeePage():
    driver.get(terminatedEmployeeDocUrl);
    WebDriverWait(driver,30).until(readystate_complete)

    ChangeTimeRange();
    StartWaitElementAppearTask(driver,By.XPATH,"//*[name()='svg']", "Load Terminated-employee page Center Chart").get();



##############################################
# Verify_UserOrDocument_Details_Page
##############################################
def Verify_UserOrDocument_Details_Page(userOrDocument):
  
    ActionTask(VerifyUserPageZoomChart,"Verify UserPage ZoomChart");
    ActionTask(VerifyUserPageTrendChart,"Verify UserPage TrendChart");
 
    VerifyClickOnTopEventOnUserOrDocumentDetailPage()


##############################################
# Verify_UserOrDocument_Details_Page
##############################################
def VerifyClickOnTopEventOnUserOrDocumentDetailPage():

    #var table = StartWaitElementAppearTask(By.Id("events-table")).Result;
    table = StartWaitElementAppearTask(driver, By.ID,"panel2").get();
    tops = StartWaitElementsAppearTask(table, By.CLASS_NAME,"shared-resultstable-resultstablerow").get();
    index = 0;

    #this used to render to the same page, now load a new page
    tops[index].find_element_by_class_name("numeric").click()

    #var rawTableDiv = StartWaitElementAppearTask(By.Id("raw_container")).Result;
    #StartWaitElementsAppearTask(rawTableDiv, By.ClassName("shared-resultstable-resultstablerow"), string.Format("Load top event on {0}-detail page", msg)).Wait();

##############################################
# VerifyUserPageTrendChart
##############################################
def VerifyUserPageTrendChart():
    trendChart = StartWaitElementAppearTask(driver, By.ID,"trend_chart").get();
    trendchartTask = StartWaitElementsAppearTask(trendChart, By.CLASS_NAME,"highcharts-axis-labels", "load userdetailspage trendchart")
    trends=trendchartTask.get()

    startdate="{0} {1}{2}".format(searchRangeStartTime.strftime("%b"),searchRangeStartTime.day,searchRangeStartTime.year)
    enddate="{0} {1}".format(searchRangeEndTime.strftime("%b"),searchRangeEndTime.day)

    assert startdate, trends[0].text
    assert enddate, trends[0].text 
  

##############################################
# VerifyUserPageZoomChart
##############################################
def VerifyUserPageZoomChart():
    zoomChart = StartWaitElementAppearTask(driver, By.ID,"zoom_chart").get()
    zoomchartTask = StartWaitElementsAppearTask(zoomChart, By.CLASS_NAME,"highcharts-axis-labels", "load userdetailspage zoomchart")
    zooms=zoomchartTask.get()

    startdate="{0} {1}{2}".format(searchRangeStartTime.strftime("%b"),searchRangeStartTime.day,searchRangeStartTime.year)
    enddate="{0} {1}".format(searchRangeEndTime.strftime("%b"),searchRangeEndTime.day)

    assert startdate in zooms[0].text
    assert enddate in zooms[0].text

    #zoom out the zoomchart series
    highchartsGroup = StartWaitElementAppearTask(zoomChart, By.CLASS_NAME,"highcharts-series-group").get()
    highcharts = StartWaitElementAppearTask(highchartsGroup, By.CLASS_NAME,"highcharts-series").get()
    path=StartWaitElementsAppearTask(highcharts, By.TAG_NAME,"path").get()
    path=path[0]
    d = path.get_attribute("d");
    moveFrom = GetSvgLineCoordinates(d)[1];
    moveTo = GetSvgLineCoordinates(d)[3];
    ActionChains(driver).move_to_element_with_offset(path, moveFrom[0], moveFrom[1]).perform()
    ActionChains(driver).drag_and_drop_by_offset(path, moveTo[0]-moveFrom[0], 0).perform();

    #verify the "reset" shows up when the swim windows is selected.
    driver.find_element_by_class_name("icon-minus-circle")

##############################################
# GetSvgLineCoordinates
##############################################
def GetSvgLineCoordinates(d):
        
    result = []

    #d example: M 406 30.799999999999997 L 418 30.799999999999997 418 42.8 406 42.8 Z
    d = d.replace("M", "");
    d = d.replace("L", "");
    d = d.replace("Z", "");
    d = d.strip()
    d = d.replace("  ", " ");

    values = d.split(' ');
    index = 0;
    while index < len(values):    
        if not values[index]: 
            index=index+1

        v1 = float(values[index])
        index=index+1
        v2 = float(values[index])
        index=index+1
        tuple=(int(v1),int(v2))
        result.append(tuple);    

    return result;

##############################################
# ActionTask
##############################################
def TryAction(action):
    #print "start to TryTryAction({0})".format(action.func_name)
    stoptime=time.time()+30
    finished=False
    ex=None
    while time.time()<stoptime and not finished:    
        try:
            action()
            finished=True
        except:
            print "????{0} next try".format(action.func_name)
            ex=sys.exc_info()

    if not finished:
        print "!!!!!!!!!==================={0}() throw exception {1}".format(action,ex)
        raise Exception(ex)
    
    #print "finish to TryTryAction({0})".format(action.func_name)

def ActionTask(action,logMsg=None):
    print "start to ActionTask({0})".format(action.func_name)
    
    pool=ThreadPool(processes=1)
    starttime=time.time()
    task=pool.apply_async(TryAction(action))
    task.wait()
    stoptime=time.time()
    
    if logMsg:
        logstr="{0}, {1}, {2}".format(time.time(),action.func_name, stoptime-starttime)
        logs.append(logstr)
   
    print "finish to ActionTask({0})".format(action.func_name)

###############################################
## FunctionTask
###############################################
#def TryFunction(func):
#    print "start to TryFunction({0})".format(func.func_name)
#    stoptime=time.time()+30
#    finished=False
#    ex=None
#    while time.time()<stoptime and not finished:    
#        try:
#            myresult=func()
#            finished=True
#        except:
#            print "{0} next try, took {1}".format(func.func_name,time.time())
#            sleep(1)
#            ex=sys.exc_info()

#    if not finished:
#        print "{0}() throw exception {1}".format(func,ex)
#        raise Exception(ex)
    
#    print "finish to TryFunction({0})".format(func.func_name)
#    return myresult

#def FunctionTask(func,logMsg=None):
#    print "start to FunctionTask({0})".format(func.func_name)
#    mypool=ThreadPool(processes=1)
#    starttime=time.time();
#    try:
#        mynewtask= mypool.apply_async(TryFunction(func))
#        mynewresult=mynewtask.get()
        
#        logstr="{0}, {1}, {2}".format(time.time(),func.func_name, time.time()-starttime)
#        logs.append(logstr)
#    except:
#        logstr="!!! except {0}, {1}, {2}".format(time.time(),func.func_name, time.time()-starttime)
#        logs.append(logstr)
#        raise Exception(sys.exc_info())

#    print "finish to FunctionTask({0})".format(func.func_name)
#    return myresult

##############################################
# VerifySummaryPageElements
##############################################
def VerifyDonutChart():
    policyViolation=StartWaitElementAppearTask(driver,By.ID,"policy_violations_panel").get()
    print("policyViolation ok")
    StartWaitElementAppearTask(policyViolation,By.CLASS_NAME,"donut_series").get() 
    donuts = StartWaitElementsAppearTask(driver,By.CLASS_NAME,"donut", "summary page policy violation donutschart").get()
    assert len(donuts)>0
    print("donuts ok")
    
    #valueInDonutUnicode1=StartWaitElementAppearTask(donuts[0], By.CLASS_NAME,"centerText").get()
    ActionTask(lambda: StartWaitElementAppearTask(donuts[0], By.CLASS_NAME,"centerText").get().text.replace("",""))
    valueInDonutUnicode1=StartWaitElementAppearTask(donuts[0], By.CLASS_NAME,"centerText").get().text
    print("valueInDonutUnicode1 ok")
    valueInDonut=float(valueInDonutUnicode1)
    assert valueInDonut>0
    
def VerifyTrendchart():
    trendchartRow=StartWaitElementAppearTask(driver,By.ID,"row3").get()
    print "trendchartRow ok"
    svgTask = StartWaitElementAppearTask(trendchartRow, By.TAG_NAME,"svg", "load Summary page central chart").get()
    print "svgTask ok"  

##############################################
# VerifySummaryPageElements
##############################################
def VerifySummaryPageElements():
    print "call VerifySummaryPageElements"
    
    ##wait trendchart showup
    #ActionTask(VerifyTrendchart, "Verify Trendchart")


    #wait donut chart to show up
    ActionTask(VerifyDonutChart,"Verify DonutChart")
        
    #wait top - userstable show up
    userPanel = StartWaitElementAppearTask(driver, By.ID,"panel3").get()
    topUsersTask = StartWaitElementAppearTask(userPanel, By.CLASS_NAME,"shared-resultstable-resultstablerow","load summary page top-user table").get()

    #wait top-documents table show up
    documentPanel = StartWaitElementAppearTask(driver, By.ID,"panel4").get()
    topDocumentsTask = StartWaitElementAppearTask(documentPanel, By.CLASS_NAME,"shared-resultstable-resultstablerow", "load summary page top-documents table").get()

    
##############################################
# LoadSplunkHomePageAndSignIn
##############################################
def LoadSplunkHomePageAndSignIn():
    driver.get(splunkServerUrl)
    WebDriverWait(driver,30).until(readystate_complete)

    print(driver.title)
    driver.maximize_window()
    driver.find_element_by_id('username').send_keys('admin')
    driver.find_element_by_id('password').send_keys('changeme')
    driver.find_element_by_class_name("splButton-primary").submit()
    #try:    
    #    driver.find_element_by_css_selector('.spl-change-password-skip').submit()
    #except:
    #    print sys.exc_info()[0]        

    #if not "Home | Splunk" in driver.title: raise Exception("wrong")
    return

##############################################
# readystate_complete: wait for page loaded
##############################################
def readystate_complete(d):
    return d.execute_script("return document.readyState") =="complete"

##############################################
# LoadSubItemPageFromSplunkhomePage
##############################################
def LoadSubItemPageFromSplunkhomePage(linkText,subItemPageTitle):
    if driver.Url != splunkHomeUrl:
        driver.get(splunkHomeUrl)

    result=StartWaitElementAppearTask(By.LinkText,linkText)
    result.click()


##############################################
# StartWaitElementAppearTask
##############################################
def WaitElementAppear(parentElement,byMethod, str,logMsg):
    if parentElement is None:
        parentElement=driver.find_element_by_tag_name('html')

    loaded=False
    stopTime=time.time()+timeoutThreshold
    startTime=time.time()
    ex=None
    result=None
    while not loaded and time.time()<stopTime:
        try:
            result=parentElement.find_element(byMethod,str)
            assert result!=None
            assert result.tag_name!=None
            loaded=True
            if logMsg:
                logstr="{0}, {1}, {2}".format(time.time(), time.time()-startTime, logMsg)
                logs.append(logstr)

        except: 
            ex=sys.exc_info()[0]

    if loaded!=True:
        print "log failed info"
        if not logMsg:
            logMsg=byMethod
            
        logstr="{0:MM/dd/yy H:mm:ss},{3}, {2}, !!!Exception: takes more than {1} seconds".format(time.time(), timeoutThreshold,timeoutThreshold + 10, logMsg)
        logs.append(logstr)

        if ex!=None:
            if isinstance(parentElement,webdriver.Firefox):
                parentName=parentElement.name
            else: 
                parentName=parentElement.tag_name
                
            print "Can't find tagName '{0}' by '{1}' for '{2}'".format(parentName,byMethod,str,logMsg)
            raise Exception("Can't find tagName '{0}' by '{1}' for '{2}'".format(parentName,byMethod,str,logMsg))               
    
    return result


def StartWaitElementAppearTask(parentElement,byMethod, str,logMsg=None):
    pool=ThreadPool(processes=1)
    result=pool.apply_async(WaitElementAppear,args=(parentElement,byMethod,str,logMsg))
    return result

##############################################
# StartWaitElementsAppearTask
##############################################
def WaitElementsAppear(parentElement,byMethod, str,logMsg):
    if parentElement is None:
        parentElement=driver.find_element_by_tag_name('html')

    loaded=False
    startTime=time.time()
    stopTime=time.time()+timeoutThreshold
    ex=None
    result=None
    while not loaded and time.time()<stopTime:
        try:
            result=parentElement.find_elements(byMethod,str)
            assert len(result)>0
            assert result[0].text!=None
            loaded=True
            if logMsg:
                logstr="{0}, {1}, {2}".format(time.time(), time.time()-startTime, logMsg)
                logs.append(logstr)

        except :
            print sys.exc_info()[0]
            ex=sys.exc_info()[0]

    if loaded!=True:
        if not logMsg:
            logMsg=byMethod
            
        logstr="{0:MM/dd/yy H:mm:ss},{3}, {2}, !!!Exception: takes more than {1} seconds".format(time.time(), timeoutThreshold,timeoutThreshold + 10, logMsg)
        logs.append(logstr)
        print "log failed info"
        if ex!=None:
            raise Exception("Can't find tagName '{0}' by '{1}' for '{2}'".format(parentName,byMethod,str,logMsg))               
   
    return result

def StartWaitElementsAppearTask(parentElement,byMethod, str,logMsg=None):
    pool=ThreadPool(processes=1)
    result=pool.apply_async(WaitElementsAppear,args=(parentElement,byMethod,str,logMsg))
    return result

##############################################
# ChangeTimeRange
##############################################
def ChangeTimeRange():
    timepicker=StartWaitElementAppearTask(driver, By.CSS_SELECTOR,".controls.shared-timerangepicker").get()
    a =StartWaitElementAppearTask(timepicker,By.TAG_NAME,"a").get()
    a.click()
    items=driver.find_elements_by_class_name("accordion-toggle")
    items[3].click()
    # choose "Between" and filled up the earliest/latest inputs
    e3 = driver.find_element_by_css_selector(".accordion-group.active")
    e4 = e3.find_element_by_class_name("accordion-body")
    e5 = e4.find_element_by_class_name("timerangepicker-range-type")
    e6 = e5.find_element_by_class_name("dropdown-toggle")
    e6.click()

    e7 = driver.find_element_by_css_selector(".dropdown-menu.dropdown-menu-selectable.dropdown-menu-narrow.open")
    e8 = e7.find_elements_by_class_name("link-label")
    e8[0].click()

    earliestDate = e4.find_element_by_class_name("timerangepicker-earliest-date")
    
    startdate="{0}/{1}/{2}".format(searchRangeStartTime.month,searchRangeStartTime.day,searchRangeStartTime.year)
    enddate=searchRangeEndTime.strftime("%m/%d/%Y")
    SendInput(startdate, earliestDate)
    latestDate = e4.find_element_by_class_name("timerangepicker-latest-date")
    SendInput(enddate, latestDate)

    #click on apply button
    e11 = e4.find_element_by_class_name("apply")
    e11.click()
    print("Change time selection succeed")
    return 

##############################################
# SendInput
##############################################
def SendInput(str, element):
    watch = time.time()+20

    while time.time() < watch and element.get_attribute("value"):
        element.clear()

    element.send_keys(str)
    element.send_keys(Keys.ENTER)
    return

##############################################
# Verify_ClickFilteredUserOrDocument
##############################################
def Verify_ClickFilteredUserOrDocument(userOrDoc):
    #click on one of the top-users, should direct to another summary page with filtered user
    userTable = StartWaitElementAppearTask(driver, By.ID, "{0}_table".format(userOrDoc)).get()
    SelectDrillDown(userTable)

    #verify the summary page with filtered user
    filteredUser = StartWaitElementAppearTask(driver, By.CLASS_NAME,"filter_tags","load summary page with filtered {0}".format(userOrDoc)).get()
    filteredUser = StartWaitElementAppearTask(filteredUser, By.CSS_SELECTOR,".tag.label.filter").get();
    text=filteredUser.text
    text=text.upper()

    userOrDoc=userOrDoc.upper()
    if userOrDoc!="USER":
        userOrDoc="OBJECT"
        
    assert "FILTER {0} IS".format(userOrDoc) in text
    print "add user filter, VerifySummaryPageElements"
    VerifySummaryPageElements()

    #remove filtered user
    RemoveUserOrDocFilter()
    print "remove user filter, VerifySummaryPageElements"
    VerifySummaryPageElements()

##############################################
# SelectDrillDown
##############################################
def SelectDrillDown(table):
    topUsers = StartWaitElementsAppearTask(table, By.CLASS_NAME,"shared-resultstable-resultstablerow").get()

    #click on one of the top-users
    index = 0

    #click "include" dropdown menu
    topUsers[index].find_element(By.CLASS_NAME,"numeric").click()
    dropdownmenu = StartWaitElementsAppearTask(driver,By.CSS_SELECTOR,".dropdown-menu.dropdown-context").get()

    for x in dropdownmenu:
        if x.is_displayed():
            x.find_elements(By.CLASS_NAME,"context-event")[0].click()
            break



##############################################
# RemoveUserOrDocFilter
##############################################
def RemoveUserOrDocFilter():
    filter = driver.find_element_by_css_selector(".tag.label.filter")
    ele = filter.find_element_by_tag_name("span")
    ele.click()
    WebDriverWait(driver,30).until(readystate_complete)

##############################################
# RemoveUserOrDocFilter
##############################################
def RunAppSetup():

    driver.get(setupUrl)
    WebDriverWait(driver,30).until(readystate_complete)

    dropdown = StartWaitElementAppearTask(driver, By.ID,"departments_dropdown").get()
    select2choices = dropdown.find_element_by_class_name("select2-choices")
 
    #remove all elements
    try:
        #remove departments
        hrefs=select2choices.find_elements_by_tag_name("a")
        for i in range(0,len(hrefs)):
            href=select2choices.find_element_by_tag_name("a")
            href.click()
    except:
        print sys.exc_info()[0]
    

    # add departments
    dropdown = StartWaitElementAppearTask(driver, By.ID,"departments_dropdown").get()
    select2choices = dropdown.find_element_by_class_name("select2-choices")
    select2choices.click()
    select2choices.find_element_by_class_name("select2-input").send_keys(Keys.ENTER) 
    select2choices.click()
    select2choices.find_element_by_class_name("select2-input").send_keys(Keys.ENTER) 
    select2choices.click()
    select2choices.find_element_by_class_name("select2-input").send_keys(Keys.ENTER) 
    select2choices.click()
    select2choices.find_element_by_class_name("select2-input").send_keys(Keys.ENTER) 
    select2choices.click()
    select2choices.find_element_by_class_name("select2-input").send_keys(Keys.ENTER) 
    select2choices.click()
    select2choices.find_element_by_class_name("select2-input").send_keys(Keys.ENTER) 

    save = driver.find_element_by_id("save")
    save.click();    
    sleep(5)
    WebDriverWait(driver,30).until(readystate_complete)
    assert driver.title=="Summary | Splunk"

##############################################
# Main Func
##############################################
driver = webdriver.Firefox()
splunkServerUrl = "http://localhost:8000/"
splunkHomeUrl = splunkServerUrl + "en-US/app/launcher/home"
warumHomeUrl=splunkServerUrl+"en-US/app/warum_pas_ri/"
summaryUrl = warumHomeUrl + "summary";
offhourDocUrl = warumHomeUrl + "offhours_document_access";
terminatedEmployeeDocUrl = warumHomeUrl + "terminated_employee_document_access";
anomalousActivityUrl = warumHomeUrl + "anomalous_activity";
userActivityUrl = warumHomeUrl + "user_activity";
setupUrl = warumHomeUrl + "setup";

logs = list()
timeoutThreshold = 20 # seconds

searchRangeStartTime=datetime.date.today()-timedelta(days=5)
searchRangeEndTime=datetime.date.today()


LoadSplunkHomePageAndSignIn()
RunAppSetup()
UserDetails()
Summary()
DocumentAccess()

driver.close()

print "done"
print logs