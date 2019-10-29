using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PizzaBot.LUIS;
using PizzaBot.SeleniumHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace PizzaBot.TelegramService
{
    public class Program
    {
        static ITelegramBotClient botClient;
        AndysPizzaSite site;

        public Program()
        {
            site = new AndysPizzaSite();
            var telegramApiKey = ConfigurationManager.AppSettings["TelegramApiKey"];
            botClient = new TelegramBotClient(telegramApiKey);
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello, I'm Pizza Bot! How can I help you {me.FirstName}?");

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            Thread.Sleep(int.MaxValue);

            Console.ReadLine();
        }

        static void Main(string[] args)
        {                 
            var program = new Program();
        }

        async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {              
                if(e.Message.Text.ToLower() == "send")
                {
                    SendOrderToAndys(e.Message.Chat.Id);
                }
                else
                {
                    var responce = LUISHelper.AddUtterances(e.Message.Text).GetAwaiter().GetResult();

                    switch (responce.topScoringIntent.intent)
                    {
                        case "AddPizzaToCard":
                            AddPizzaToCard(e.Message.Chat.Id, responce.entities[0].entity);
                            break;
                        case "ExecuteOrder":
                            OrderPizza(e.Message.Chat.Id, responce.entities[0].entity, e.Message.From.FirstName);
                            break;
                        case "GetListOfPizza":
                            DisplayListOfPizza(e.Message.Chat.Id);
                            break;
                        case "Salutation":
                            Salutation(e.Message.Chat.Id, e.Message.From.FirstName);
                            break;

                        default:
                            IntentNotFound(e.Message.Chat.Id);
                            break;
                    }
                }                
            }
        }

        async void Salutation(long chatId, string firstName)
        {
            await botClient.SendTextMessageAsync(chatId, $"Hello, I'm Pizza Bot! How can I help you {firstName}?");
        }
        async void DisplayListOfPizza(long chatId)
        {
            var pizzas = site.GetPizzas().Skip(1).Take(4);
            foreach (var item in pizzas)
            {
                var newPhoto = new InputOnlineFile(item.ImageLink);
                await botClient.SendPhotoAsync(chatId, newPhoto, item.Name, Telegram.Bot.Types.Enums.ParseMode.Html, true);
            }
            await botClient.SendTextMessageAsync(chatId, $"I can offer a special list of tasty pizza, choose one please");
        }

        async void AddPizzaToCard(long chatId, string pizzaName)
        {
            var firstToUpper = pizzaName.First().ToString().ToUpper() + pizzaName.Substring(1);
            site.AddPizzaToCard(firstToUpper);

            await botClient.SendTextMessageAsync(chatId, $"Pizza {pizzaName} has been added to card. Add more pizza or complete the order.");
        }

        async void OrderPizza(long chatId, string phoneNumber, string userName)
        {
           site.CompleteOrder(phoneNumber, userName);

            await botClient.SendTextMessageAsync(chatId, "Your order is almost done, type 'Send' for confirmation");
        }

        async void SendOrderToAndys(long chatId)
        {
            var orderNumber = site.SendOrderToAndys();

            await botClient.SendTextMessageAsync(chatId, $"Order {orderNumber} is being processed, operator will call you in 5 minutes.");
        }

        async void IntentNotFound(long chatId)
        {
            await botClient.SendTextMessageAsync(chatId, "Sorry, but I couldn't understand your intention, reformulate the question please :)");
        }
    }
}
