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

    /// <summary>
    /// Make requests towards reddit regarding messages
    /// </summary>
    class MessageHandler
    {

        private TokenBucket _tb;
        private HttpClient _client;
        public MessageHandler(HttpClient client, TokenBucket tb)
        {
            _client = client;
            _tb = tb;
        }

        /// <summary>
        /// Send Request
        /// (If there is token.
        /// else sleep)
        /// </summary>
        /// <param name="method"> The url the get request is sent to </param>
        /// <returns> HttpResponseMessage </returns>
        public async Task<HttpResponseMessage> GetRequestAsync(string method)
        {
            Console.WriteLine(method);
            Console.WriteLine(_client);
            if (_tb.RequestIsAllowed(1))
            {
                return await _client.GetAsync(method);
            }
            else
            {
                System.Threading.Thread.Sleep(60000);
                return await GetRequestAsync(method);
            }
        }

        /// <summary>
        /// Send Request
        /// (If there is token.
        /// else sleep)
        /// </summary>
        /// <param name="method"> The url the post request is sent to </param>
        /// <param name="data"> The content sent to the url </param>
        /// <returns> HttpResponseMessage </returns>
        public async Task<HttpResponseMessage> PostRequestAsync(string method, FormUrlEncodedContent data)
        {
            Console.WriteLine(method);
            Console.WriteLine(_client);
            if (_tb.RequestIsAllowed(1))
            {
                return await _client.PostAsync(method, data);
            }
            else
            {
                System.Threading.Thread.Sleep(60000);
                return await PostRequestAsync(method, data);
            }
        }

        /// <summary>
        /// Send Request
        /// (If there is token.
        /// else sleep)
        /// </summary>
        /// <param name="method"> The url the post request is sent to </param>
        /// <returns> HttpResponseMessage </returns>
        public async Task<HttpResponseMessage> PostRequestAsync(string method)
        {
            Console.WriteLine(method);
            Console.WriteLine(_client);
            if (_tb.RequestIsAllowed(1))
            {
                var formdata = new Dictionary<string, string> { };
                var encodedformdata = new FormUrlEncodedContent(formdata);

                return await _client.PostAsync(method, encodedformdata);
            }
            else
            {
                System.Threading.Thread.Sleep(60000);
                return await PostRequestAsync(method);
            }
        }


        /// <summary>
        /// Get token (permission) to access reddit
        /// </summary>
        /// <param name="client"> HttpClient used to handle Http request </param>
        /// <param name="response"> Response from Http request </param>
        private void TokenUsage(HttpClient client, HttpResponseMessage response)
        {
            var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var accessToken = JObject.Parse(responseData).SelectToken("access_token").ToString();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);

            client.GetAsync("https://oauth.reddit.com/api/v1/me").GetAwaiter().GetResult();
            responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine(responseData);
        }

        /// <summary>
        /// Get "id" to all unread messages
        /// Respond to message by sending "bacon"
        /// Empty inbox by deleting all unread messages
        /// </summary>
        /// <example>
        /// 
        /// var messageHandler = new MessageHandler(Client, TokenBucket);
        /// messageHandler.Run();
        ///  
        /// </example>
        public void Run()
        {
            var response = GetRequestAsync("https://oauth.reddit.com/message/unread.json").GetAwaiter().GetResult();
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
                var sendComment = PostRequestAsync(authurl, encodedFormData).GetAwaiter().GetResult();
                Console.WriteLine(sendComment);

            }

            var deleteMessages = "https://oauth.reddit.com/api/read_all_messages.json";
            PostRequestAsync(deleteMessages).GetAwaiter().GetResult();
        }

    }
}
