using MySql.Data.MySqlClient;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckUNLOCKSWDL2.Lib
{
    internal class SeleniumHelper
    {

        ChromeOptions options = new ChromeOptions();
        ChromeDriverService cds = ChromeDriverService.CreateDefaultService();
        IWebDriver driver ;
        public string Path;
        Logger Logger = new Logger();
        public SeleniumHelper(string _path)
        {
            this.Path = _path;
            cds.HideCommandPromptWindow = true;
            options.AddArguments("--headless");
        }
        public void DestroyChrome()
        {
            if(driver != null)
            {
                Logger.WriteLog(Path, "EXIT CHROME");
                driver.Close();
                driver.Dispose();
            }
           
        }
        string username = "superadmin";
        public bool CheckSerialNumberWeb(string DUT_SN, string url)
        {
            using (driver = new ChromeDriver(cds, options, TimeSpan.FromSeconds(60)))
            {
                bool isUrlLoaded = false;
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl(url);
                //WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                //wait.Until(drv => drv.Url.Contains(url));
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                
                
                for (int i = 0; i < 10; i++) // Tối đa 10 lần thử, mỗi lần cách nhau 1 giây
                {
                    Thread.Sleep(1000);

                    if (driver.Url.Contains(url))
                    {
                        isUrlLoaded = true;
                        break;
                    }
                }
                if (isUrlLoaded == false)
                {
                    Logger.WriteLog(Path, " Can not connect websupper ");
                }
                string password_Default = "adminadminadmina";
                Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-pre-login/nav/pv-logout/div/div[2]", "", "Click");
                IWebElement usernameELement = driver.FindElement(By.XPath("/html/body/app-root/app-login/div/pv-card/form/div[2]/pv-form-field[1]/div/pv-inputbox/div/div/input"));
                usernameELement.SendKeys(username);
                IWebElement passwordElement = driver.FindElement(By.XPath("/html/body/app-root/app-login/div/pv-card/form/div[2]/pv-form-field[2]/div/pv-inputbox/div/div/input"));
                passwordElement.SendKeys(password_Default);
                IWebElement buttonLogin = driver.FindElement(By.XPath("/html/body/app-root/app-login/div/pv-card/form/div[3]/pv-button/button"));
                buttonLogin.Click();

                bool isLogin;
                isLogin = driver.FindElements(By.XPath("/html/body/app-root/app-main/div/div/app-overview/div[1]/div[1]/pv-card/div/div/pinch-zoom/ul/li/ul/li/a/div")).Count > 0;
                if (isLogin)
                {
                    IWebElement SN_element = driver.FindElement(By.XPath(""));
                    return false;
                }
                return true;
            }
        }
        public string GetWebSuperPassword(string serialNumber)
        {
            
            string webSuperPassword = string.Empty;
            ConnectionDB connectDB = new ConnectionDB(Path);

            try
            {

                if (connectDB.Open())
                {

                    string query = string.Format(@"SELECT vn_g67_customer_data.WebSuperPassword 
                                           FROM vn_g67_customer_data 
                                           WHERE vn_g67_customer_data.SerialNumber = '{0}';", serialNumber);
                    MySqlDataReader reader = connectDB.ExecuteReader(query);
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            webSuperPassword = reader["WebSuperPassword"]?.ToString();
                            if (string.IsNullOrWhiteSpace(webSuperPassword))
                            {
                                webSuperPassword = "null";
                            }
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Path,"Exeption : " + ex.Message);
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                connectDB.Close();
            }

            return webSuperPassword;
        }


        private void Process_Kill_new(string process)
        {
            try
            {
                Process[] proc = Process.GetProcesses();
                foreach (Process item in proc)
                {
                    if (item.ProcessName.Contains(process))
                    {
                        item.Kill();

                    }
                }

            }
            catch (Exception ep)
            {

            }
        }
        //public bool CheckSeriaNumberWeb(IWebDriver driver)
        //{

        //}
        public bool CheckWebStatus(string url, string DUT_SN)
        {
            //DelayMs(50000);
            //Process_Kill_new("chrome");                      
            Logger.WriteLog(Path, "- USING SELENIUM CHECKING WEB SUPER -");
            
            string password_Default = "adminadminadmina";
            

            using (driver = new ChromeDriver(cds, options, TimeSpan.FromSeconds(60)))
            {
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl(url);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(50);
                bool isOnLine = false;
                isOnLine = driver.FindElements(By.XPath("/html/body/app-root/app-pre-login/div/div/router-outlet/div/div/div/div[3]/pv-card/div/pv-list[1]/div/div/div[2]/pv-text/div")).Count > 0;

                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(1000);
                    if (isOnLine)
                    {
                        break;
                    }
                }
                Logger.WriteLog(Path, "=============================LOGIN WEB DEFAULT PASSWORD=========================");
                Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-pre-login/nav/pv-logout/div/div[2]", "", "Click");
                IWebElement usernameELement = driver.FindElement(By.XPath("/html/body/app-root/app-login/div/pv-card/form/div[2]/pv-form-field[1]/div/pv-inputbox/div/div/input"));
                usernameELement.SendKeys(username);
                Logger.WriteLog(Path, "username is: " + username);
                IWebElement passwordElement = driver.FindElement(By.XPath("/html/body/app-root/app-login/div/pv-card/form/div[2]/pv-form-field[2]/div/pv-inputbox/div/div/input"));
                passwordElement.SendKeys(password_Default);
                Logger.WriteLog(Path, "password is: adminadminadmina " );
                IWebElement buttonLogin = driver.FindElement(By.XPath("/html/body/app-root/app-login/div/pv-card/form/div[3]/pv-button/button"));
                buttonLogin.Click();               
                bool isLogin;
                isLogin = driver.FindElements(By.XPath("/html/body/app-root/app-main/div/div/app-overview/div[1]/div[1]/pv-card/div/div/pinch-zoom/ul/li/ul/li/a/div")).Count > 0;
                if (isLogin==true)
                {
                    Logger.WriteLog(Path, "Loggin password defalt success");
                    Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-main/app-side-nav/nav/div[1]/pv-side-nav/nav/ul/li[7]/div", "", "Click");
                    Logger.WriteLog(Path, "CLICK ACCESS CONTROLL WEB SUPER");
                    Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-main/app-side-nav/nav/div[1]/pv-side-nav/nav/ul/li[7]/ul/li[4]", "", "Click");
                    IWebElement input_ssh_status = driver.FindElement(By.XPath("/html/body/app-root/app-main/div/div/app-access-control/div/form/pv-card[2]/div[1]/div[2]/pv-form-field[2]/div/span/pv-select/div/input"));
                    string ssh_value = input_ssh_status.GetAttribute("value");
                    Logger.WriteLog(Path, "GET Websuper SSH status is: " + ssh_value);
                    if (ssh_value.Contains("Allow"))
                    {
                        Logger.WriteLog(Path, "SSH VALUE:   ALLOW");
                        return true;
                    }
                    else
                    {
                       
                        Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-main/div/div/app-access-control/div/form/pv-card[2]/div[1]/div[2]/pv-form-field[2]/div/span/pv-select/div/pv-vector", "", "Click");
                        Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-main/div/div/app-access-control/div/form/pv-card[2]/div[1]/div[2]/pv-form-field[2]/div/span/pv-select/div/div/ul/li[1]", "", "Click");
                        Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-main/div/div/app-access-control/div/form/pv-card[2]/div[2]/pv-button/button", "", "Click");
                        Logger.WriteLog(Path, "SSH VALUE:   ALLOW");
                        return true;
                    }
                  
                }
                else
                {
                    //login mật khẩu superapassword
                    string websuperPassword = String.Empty;
                    websuperPassword = GetWebSuperPassword(DUT_SN);
                    driver.Navigate().Refresh();
                    //driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                    Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-pre-login/nav/pv-logout/div/div[2]", "", "Click");
                    usernameELement = driver.FindElement(By.XPath("/html/body/app-root/app-login/div/pv-card/form/div[2]/pv-form-field[1]/div/pv-inputbox/div/div/input"));
                    usernameELement.SendKeys(username);
                    passwordElement = driver.FindElement(By.XPath("/html/body/app-root/app-login/div/pv-card/form/div[2]/pv-form-field[2]/div/pv-inputbox/div/div/input"));
                    passwordElement.SendKeys(websuperPassword);
                    Logger.WriteLog(Path, "Password super : " + websuperPassword);
                    buttonLogin = driver.FindElement(By.XPath("/html/body/app-root/app-login/div/pv-card/form/div[3]/pv-button/button"));
                    buttonLogin.Click();
                    Logger.WriteLog(Path, "Loggin super password success");
                    //isLogin = driver.FindElements(By.XPath("/html/body/app-root/app-main/div/div/app-overview/div[1]/div[1]/pv-card/div/div/pinch-zoom/ul/li/ul/li/a/div")).Count > 0;
                    Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-main/app-side-nav/nav/div[1]/pv-side-nav/nav/ul/li[7]/div", "", "Click");
                    Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-main/app-side-nav/nav/div[1]/pv-side-nav/nav/ul/li[7]/ul/li[4]", "", "Click");
                    IWebElement input_ssh_status = driver.FindElement(By.XPath("/html/body/app-root/app-main/div/div/app-access-control/div/form/pv-card[2]/div[1]/div[2]/pv-form-field[2]/div/span/pv-select/div/input"));
                    string ssh_value = input_ssh_status.GetAttribute("value");
                    Logger.WriteLog(Path, "GET Websuper SSH status is: " + ssh_value);
                    if (ssh_value.Contains("Deny"))
                    {
                        Logger.WriteLog(Path, "SSH VALUE: DENY ");
                        return false;
                    }
                    else
                    {
                        Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-main/div/div/app-access-control/div/form/pv-card[2]/div[1]/div[2]/pv-form-field[2]/div/span/pv-select/div/pv-vector", "", "Click");
                        Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-main/div/div/app-access-control/div/form/pv-card[2]/div[1]/div[2]/pv-form-field[2]/div/span/pv-select/div/div/ul/li[2]", "", "Click");
                        Chrome_FindElement_Xpath(driver, "/html/body/app-root/app-main/div/div/app-access-control/div/form/pv-card[2]/div[2]/pv-button/button", "", "Click");
                        Logger.WriteLog(Path, "SSH VALUE: DENY ");
                        return false;
                    }

                }
            }
        }


        public void DelayMs(int ms)
        {
            int index = ms / 100 + 1;
            for (int i = 0; i < index; i++)
            {
                Thread.Sleep(100);
                Application.DoEvents();
            }
        }
        private bool Chrome_FindElement_Xpath(IWebDriver driver, string Locator, string SendKeys, string mode)
        {
            string Text_result;
            try
            {
                switch (mode)
                {
                    case "SendKeys":
                        driver.FindElement(By.XPath(Locator)).SendKeys(SendKeys);
                        return true;
                    case "Click":
                        driver.FindElement(By.XPath(Locator)).Click();
                        return true;
                    case "GetText":
                        Text_result = "";
                        Text_result = driver.FindElement(By.XPath(Locator)).Text;
                        if (Text_result == "" || Text_result == "N/A")
                        {
                            DelayMs(2000);
                            Text_result = driver.FindElement(By.XPath(Locator)).Text;
                            DelayMs(1000);
                            if (Text_result == "") return false;
                        }
                        return true;
                    case "Refresh":
                        driver.Navigate().Refresh();
                        return true;
                    default:
                        return false;

                }
            }
            catch
            {
                for (int i = 0; i < 5;)
                {
                    try
                    {
                        DelayMs(2000);
                        switch (mode)
                        {
                            case "SendKeys":
                                driver.FindElement(By.XPath(Locator)).SendKeys(SendKeys);
                                return true;
                            case "Click":
                                driver.FindElement(By.XPath(Locator)).Click();
                                return true;
                            case "GetText":
                                Text_result = "";
                                Text_result = driver.FindElement(By.XPath(Locator)).Text;
                                if (Text_result == "")
                                {
                                    DelayMs(2000);
                                    Text_result = driver.FindElement(By.XPath(Locator)).Text;
                                    DelayMs(1000);
                                    if (Text_result == "") return false;
                                }
                                return true;
                            case "Refresh":
                                driver.Navigate().Refresh();
                                return true;
                            default:
                                return false;

                        }
                    }
                    catch { }
                    i++;
                }
            }
            return false;
        }

    }
}
