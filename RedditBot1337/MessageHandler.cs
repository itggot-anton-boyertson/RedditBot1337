using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditBot1337
{
    class MessageHandler
    {
        private TokenBucket _tb;

        public async Task<HttpResponseMessage> GetRequestAsync(HttpClient client, string method)
        {
            Console.WriteLine(method);
            Console.WriteLine(client);
            if (_tb.RequestIsAllowed(1))
            {
                return await client.GetAsync(method);
            }
            else
            {
                System.Threading.Thread.Sleep(60000);
                return await GetRequestAsync(client, method);
            }
        }

        public async Task<HttpResponseMessage> PostRequestAsync(HttpClient client, string method, FormUrlEncodedContent data)
        {
            Console.WriteLine(method);
            Console.WriteLine(client);
            if (_tb.RequestIsAllowed(1))
            {
                return await client.PostAsync(method, data);
            }
            else
            {
                System.Threading.Thread.Sleep(60000);
                return await PostRequestAsync(client, method, data);
            }
        }

        public async Task<HttpResponseMessage> PostRequestAsync(HttpClient client, string method)
        {
            Console.WriteLine(method);
            Console.WriteLine(client);
            if (_tb.RequestIsAllowed(1))
            {
                var formdata = new Dictionary<string, string> { };
                var encodedformdata = new FormUrlEncodedContent(formdata);

                return await client.PostAsync(method, encodedformdata);
            }
            else
            {
                System.Threading.Thread.Sleep(60000);
                return await PostRequestAsync(client, method);
            }
        }

        private void TokenUsage(HttpClient client, HttpResponseMessage response)
        {
            var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var accessToken = JObject.Parse(responseData).SelectToken("access_token").ToString();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);

            client.GetAsync("https://oauth.reddit.com/api/v1/me").GetAwaiter().GetResult();
            responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine(responseData);
        }

        public void Run(HttpClient client)
        {
            var response = GetRequestAsync(client, "https://oauth.reddit.com/message/unread.json").GetAwaiter().GetResult();
            var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine(responseData);


            var obj = JObject.Parse(responseData);

            foreach (var commentData in obj["data"]["children"])
            {
                var commentName = commentData["data"]["name"].Value<string>();
                Console.WriteLine(commentName);

                var formData = new Dictionary<string, string>

                {
                    { "grant_type", "json" },
                    { "text", "bacon" },
                    { "thing_id", commentName }
                };

                var encodedFormData = new FormUrlEncodedContent(formData);

                var authurl = "https://oauth.reddit.com/api/comment.json";
                var sendComment = PostRequestAsync(client, authurl, encodedFormData).GetAwaiter().GetResult();
                Console.WriteLine(sendComment);

            }

            var deleteMessages = "https://oauth.reddit.com/api/read_all_messages.json";
            var deleteMessagesData = PostRequestAsync(client, deleteMessages).GetAwaiter().GetResult();

        }
    }
}
