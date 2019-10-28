using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
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
            string chromedriverPath = @"E:\Workspace\PizzaBot\PizzaBot\PizzaBot.SeleniumHelper\bin\Debug\netcoreapp3.0";
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

        public string CompleteOrder(string phoneNumber)
        {
            driver.Navigate().GoToUrl("https://www.andys.md/ru/catalog/8");

            FindElementsByXpath(".//*[@class='bag__tobag']")[0].Click();          
            FindElementsByXpath(".//button[@class='button button_ord']")[0].Click();            
            FindElementsByXpath(".//input[@class='input-text-plc__input']")[0].SendKeys("Studenților");            
            FindElementsByXpath(".//input[@class='input-text-plc__input']")[1].SendKeys("9/11");            
            FindElementsByXpath(".//input[@class='input-text-plc__input']")[2].SendKeys("-");            
            FindElementsByXpath(".//input[@class='input-text-plc__input']")[6].SendKeys(phoneNumber);            
            FindElementsByXpath(".//button[@class='cash button button_met ']")[0].Click();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            //FindElementByXpath(".//button[@class='button button_ord']").Click();
            return "1234123123";
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

        public IList<IWebElement> FindElementsByXpath(string xpath)
        {
            var elements = driver.FindElements(By.XPath(xpath));
            return elements;
        }

        public IWebElement FindElementByXpath(string xpath)
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

    }

    public class Pizza
    {
        public string Name { get; set; }
        public string ImageLink { get; set; }
    }
}
