using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChatBotMVC.Controllers
{
    public class ChatController : Controller
    {
        private readonly string key = "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";
        private readonly string url = "https://api.openai.com/v1/chat/completions";

        public ActionResult Index()
        {
            if (Session["Content"] == null)
                FeedContent();
            return View();
        }

        public async Task<ActionResult> GenerateResponse(string input)
        {
            string response = string.Empty;
            var content = Session["Content"].ToString();
            var messages = new List<dynamic>
             {
                 new {role = "system",content = content},
                 new {role = "assistant",content = "Welcome to MRCC group LMS information\n A wide gamut of learning solutions in eLearning, LMS, ILT, VILT, and Corporate Training. MRCC provides Digital publishing services across various disciplines such as Business Publishing, World Languages, Careers, Workforce Readiness, Nursing, IT Labs among many others. \nHow can I help you?"}
             };

            // Capture the users messages and add to
            // messages list for submitting to the chat API
            messages.Add(new { role = "user", content = input });

            // Create the request for the API sending the
            // latest collection of chat messages
            var request = new
            {
                messages,
                model = "gpt-3.5-turbo-16k",
                max_tokens = 10000,
            };
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
            var requestJson = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
            var httpResponseMessage = await httpClient.PostAsync(url, requestContent);
            var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeAnonymousType(jsonString, new
            {
                choices = new[] { new { message = new { role = string.Empty, content = string.Empty } } },
                error = new { message = string.Empty }
            });


            if (!string.IsNullOrEmpty(responseObject?.error?.message))  // Check for errors
            {
                //AnsiConsole.MarkupLine($"[bold red]{Markup.Escape(responseObject?.error.message)}[/]");
            }
            else  // Add the message object to the message collection
            {
                var messageObject = responseObject?.choices[0].message;
                messages.Add(messageObject);
                response = messageObject.content;
                //AnsiConsole.MarkupLine($"[purple]MACHINE:[/] [blue]{Markup.Escape(messageObject.content)}[/]");
            }

            // Deserialize the API response to a dynamic object
            dynamic apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);

            // Access the 'usage' field to get the token count
            int tokenCount = apiResponse.usage.total_tokens;
            //Console.WriteLine($"Tokens used in this API call: {tokenCount}");
            return Json(new { res = response }, JsonRequestBehavior.AllowGet);
        }

        public void FeedContent()
        {
            string filename = @"D:\\Manualv1.txt";
            string content = string.Empty;
            if (System.IO.File.Exists(filename))
                content = System.IO.File.ReadAllText(filename);

            content = "You are ChatGPT, a large language \" + \"model trained by OpenAI. Please answer based on below information " + content + "Answer as concisely as possible";
            Session["Content"] = content;
        }
    }
}