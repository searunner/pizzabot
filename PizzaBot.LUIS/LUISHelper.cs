using JsonFormatterPlus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PizzaBot.LUIS
{
    public class LUISHelper
    {
        // NOTE: Replace this example LUIS application ID with the ID of your LUIS application.
        static string appID = "844971e8-f5cf-43fc-ace2-87dd75ebcb57";

        // NOTE: Replace this example LUIS application version number with the version number of your LUIS application.
        static string appVersion = "0.1";
        static string authoringKey = "caa91b28026245a0a27eb0f5ab2a6737";

        // NOTE: Replace this example LUIS authoring key with a valid key.
        static string pizzaEndPoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/844971e8-f5cf-43fc-ace2-87dd75ebcb57?verbose=true&timezoneOffset=0&subscription-key=caa91b28026245a0a27eb0f5ab2a6737&q=";
             

        async static Task<HttpResponseMessage> SendGet(string uri)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", authoringKey);
                return await client.SendAsync(request);
            }
        }

        async static Task<HttpResponseMessage> SendPost(string uri, string requestBody)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);

                if (!String.IsNullOrEmpty(requestBody))
                {
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "text/json");
                }

                request.Headers.Add("Ocp-Apim-Subscription-Key", authoringKey);
                return await client.SendAsync(request);
            }
        }

        public async static Task<Rootobject> AddUtterances(string utterance)
        {
            string uri = pizzaEndPoint + utterance;
            //string requestBody = File.ReadAllText(input_file);

            var response = await SendGet(uri);
            var result = await response.Content.ReadAsStringAsync();

            var responceObj = JsonConvert.DeserializeObject<Rootobject>(result);

            Console.WriteLine("Added utterances.");
            Console.WriteLine(JsonFormatter.Format(result));

            return responceObj;
        }

        async static Task Status()
        {
            var response = await SendGet(pizzaEndPoint + "/train");
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Requested training status.");
            Console.WriteLine(JsonFormatter.Format(result));
        }

        async static Task Train()
        {
            string uri = pizzaEndPoint + "/train";

            var response = await SendPost(uri, null);
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Sent training request.");
            Console.WriteLine(JsonFormatter.Format(result));

        }
    }
    public class Rootobject
    {
        public string query { get; set; }
        public Topscoringintent topScoringIntent { get; set; }
        public Intent[] intents { get; set; }
        public Entity[] entities { get; set; }
    }

    public class Topscoringintent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public float score { get; set; }
        public string role { get; set; }
    }
}
