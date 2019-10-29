using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace PizzaBot.SeleniumHelper
{
    public class AndysPizzaSite
    {
        const string DOMAIN = "https://www.andys.md";
        readonly IWebDriver driver;
        public AndysPizzaSite()
        {
            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("headless");
            string chromedriverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace("TelegramService", "SeleniumHelper");
            driver = new ChromeDriver(chromedriverPath, options, TimeSpan.FromMinutes(10));
        }

        public List<Pizza> GetPizzas()
        {
            driver.Navigate().GoToUrl("https://www.andys.md/ru/catalog/8");

            var pizzaElements = driver.FindElements(By.XPath("//*[@class='product']")).ToList();

            var pizzaList = pizzaElements.Select(x => new Pizza
            {
                Name = x.FindElement(By.XPath(".//*[@class='product__name']")).Text,
                ImageLink = x.FindElement(By.XPath(".//*[@class='product__img show_order_win']//img")).GetAttribute("src")
            }).ToList();

            //driver.Quit();

            return pizzaList;
        }

        public void CompleteOrder(string phoneNumber, string userName)
        {
            driver.Navigate().GoToUrl("https://www.andys.md/ru/catalog/8");

            FindElementsByXpath(".//*[@class='bag__tobag']")[0].Click();          
            FindElementsByXpath(".//button[@class='button button_ord']")[0].Click();
            
            FindElementsByXpath(".//input[@class='input-text-plc__input']")[0].SendKeys(userName);            
            FindElementsByXpath(".//input[@class='input-text-plc__input']")[1].SendKeys(GetSettingFromConfiguration("AddressStreet"));            
            FindElementsByXpath(".//input[@class='input-text-plc__input']")[2].SendKeys(GetSettingFromConfiguration("AddressHouse"));            
            FindElementsByXpath(".//input[@class='input-text-plc__input']")[3].SendKeys(GetSettingFromConfiguration("AddressAppartment"));            
            
            FindElementsByXpath(".//input[@class='input-text-plc__input']")[7].SendKeys(phoneNumber);            
            FindElementsByXpath(".//button[@class='cash button button_met ']")[0].Click();
            Thread.Sleep(TimeSpan.FromSeconds(1));            
            
            //driver.Quit();
        }

        public void AddPizzaToCard(string pizzaName)
        {
            try
            {
                var xpath = $".//div[@class='product__name' and text()='{pizzaName}']/parent::div/following-sibling::*";
                FindElementByXpath(xpath).Click();
                Thread.Sleep(TimeSpan.FromSeconds(1));
                FindElementsByXpath("//*[@class='button button_add add_to_cart']")[0].Click();
            }
            catch (Exception)
            {
                driver.Quit();
            }
        }

        public string SendOrderToAndys()
        {
            FindElementsByXpath(".//button[@class='button button_ord']")[0].Click();
            Thread.Sleep(TimeSpan.FromMilliseconds(500));
            FindElementsByXpath(".//button[@class='button button_ord']")[0].Click();
            Thread.Sleep(TimeSpan.FromMilliseconds(500));

            var orderInfo = GetOrderInfo();            

            return orderInfo;
        }

        public string GetOrderInfo()
        {
            var elements = FindElementsByXpath("//div[@class = 'check__info-prop']");

            var completeInfo = new StringBuilder(); 
            
            var orderNumber = elements[0].FindElements(By.XPath(".//div"))[1].Text;
            var amount = elements[4].FindElements(By.XPath(".//div"))[1].Text;
            var eta = elements[5].FindElements(By.XPath(".//div"))[1].Text;
            
            completeInfo
                .AppendLine(orderNumber)
                .AppendLine(amount)
                .AppendLine(eta);
            

            return completeInfo.ToString();
        }

        private IList<IWebElement> FindElementsByXpath(string xpath)
        {
            var elements = driver.FindElements(By.XPath(xpath));
            return elements;
        }

        private IWebElement FindElementByXpath(string xpath)
        {
            try
            {
                var element = driver.FindElement(By.XPath(xpath));
                return element;
            }
            catch (Exception)
            {
                driver.Quit();
                return null;
            }

        }

        private string GetSettingFromConfiguration(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

    }

    public class Pizza
    {
        public string Name { get; set; }
        public string ImageLink { get; set; }
    }
}
