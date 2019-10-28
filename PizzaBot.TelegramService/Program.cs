using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PizzaBot.LUIS;
using PizzaBot.SeleniumHelper;
using System;
using System.Collections.Generic;
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
            botClient = new TelegramBotClient("333986430:AAGEjLTakbjX5M-rwJ2UMJ8p1-nu6hS61i4");
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello, I'm Pizza Bot! How can I help you {me.FirstName}?");

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            Thread.Sleep(int.MaxValue);

            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            //var list = AndysPizzaSite.GetPizzas();
            //Console.WriteLine("Hello World!");           
            var program = new Program();
        }

        async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {              

                var responce = LUISHelper.AddUtterances(e.Message.Text).GetAwaiter().GetResult();

                switch (responce.topScoringIntent.intent)
                {
                    case "AddPizzaToCard": AddPizzaToCard(e.Message.Chat.Id, responce.entities[0].entity); 
                        break;
                    case "ExecuteOrder": OrderPizza(e.Message.Chat.Id, responce.entities[0].entity);
                        break;
                    case "GetListOfPizza": DisplayListOfPizza(e.Message.Chat.Id);
                        break;
                    case "Salutation":
                        Salutation(e.Message.Chat.Id, e.Message.From.FirstName);
                        break;

                    default: IntentNotFound(e.Message.Chat.Id);
                        break;
                }

                //var newPhoto = new InputOnlineFile("http://andys.md/public/menu/thumbs/version_220x310x1/bdfbc936fb2c6f3e0fd0e777dcfd2001.jpg");
                //var pizzas = new List<string> { "Funghi", "4Cheese", "Capricioasa" };
                //Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}.");
                //var pollMessage = await botClient.SendPollAsync(botClient.BotId, "What pizza would you like?", pizzas);
                //var photo = await botClient.SendPhotoAsync(e.Message.Chat.Id, newPhoto, "<a href='google.com'>4Cheese</a>", Telegram.Bot.Types.Enums.ParseMode.Html, true);
                //var articles = new List<InlineQueryResultArticle>
                //   {
                //      new InlineQueryResultArticle("1","test1", new InputTextMessageContent("Test1")),
                //      new InlineQueryResultArticle("2","test2", new InputTextMessageContent("Test2"))
                //   };
                //var rkm = new ReplyKeyboardMarkup();
                //rkm.Keyboard =
                //    new KeyboardButton[][]
                //    {
                //        new KeyboardButton[]
                //        {
                //            new KeyboardButton("item"),
                //            new KeyboardButton("item")
                //        },
                //        new KeyboardButton[]
                //        {
                //            new KeyboardButton("item")
                //        }
                //    };

                //var button = await botClient.AnswerInlineQueryAsync("1", articles);
                //await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Text", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, rkm);
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

        async void OrderPizza(long chatId, string phoneNumber)
        {
            var orderNumber  = site.CompleteOrder(phoneNumber);

            await botClient.SendTextMessageAsync(chatId, $"Order {orderNumber} successfully completed! The operator will call you soon.");
        }

        async void IntentNotFound(long chatId)
        {
            await botClient.SendTextMessageAsync(chatId, "Sorry, but I couldn't understand your intention, reformulate the question please :)");
        }
    }
}
