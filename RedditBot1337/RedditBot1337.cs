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
    class RedditBot
    {

        private string _clientid;
        private string _clientsecret;
        private string _redditusername;
        private string _redditpassword;
        private string _clientversion;
        private TokenBucket _tb;

        public async Task<HttpResponseMessage> GetRequestAsync(HttpClient client, string method)
        {
            Console.WriteLine(method);
            Console.WriteLine(client);
            if (_tb.RequestIsAllowed(1) )
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
                var formdata = new Dictionary<string, string>{ };
                var encodedformdata = new FormUrlEncodedContent(formdata);

                return await client.PostAsync(method, encodedformdata);
            }
            else
            {
                System.Threading.Thread.Sleep(60000);
                return await PostRequestAsync(client, method);
            }
        }

        //constructor
        public RedditBot(string clientid, string clientsecret, string redditusername, string redditpassword, string clientversion)
        {
            _clientid = clientid;
            _clientsecret = clientsecret;
            _redditusername = redditusername;
            _redditpassword = redditpassword;
            _clientversion = clientversion;

            _tb = new TokenBucket(60, 60);

            using (var client = new HttpClient())
            {
                SetBasicAuthenticationHeader(client);
                SetCustomUserAgent(client);
                CreateFormData(client);
                var response = CreateFormData(client);
                TokenUsage(client, response);
                Run(client);
            }

        }

        private void SetBasicAuthenticationHeader(HttpClient client)
        {
            var AuthenticationArray = Encoding.ASCII.GetBytes($"{_clientid}:{ _clientsecret}");
            var EncodedAuthenticationString = Convert.ToBase64String(AuthenticationArray);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("basic", EncodedAuthenticationString);
        }

        private void SetCustomUserAgent(HttpClient client)
        {
            client.DefaultRequestHeaders.Add("user-agent", $"changemeclient / v{_clientversion} by { _redditusername}");
        }

        private HttpResponseMessage CreateFormData(HttpClient client)
        {
            var formData = new Dictionary<string, string>

                {
                { "grant_type", "password" },
                { "username", _redditusername },
                { "password", _redditpassword }
                };

            var encodedFormData = new FormUrlEncodedContent(formData);

            var authurl = "https://www.reddit.com/api/v1/access_token";
            var response = client.PostAsync(authurl, encodedFormData).GetAwaiter().GetResult();

            Console.WriteLine(response.StatusCode);
            // actual token
            
            return response;
        }

        MessageHandler

        //private void TokenUsage(HttpClient client, HttpResponseMessage response)
        //{
        //    var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        //    var accessToken = JObject.Parse(responseData).SelectToken("access_token").ToString();

        //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);

        //    client.GetAsync("https://oauth.reddit.com/api/v1/me").GetAwaiter().GetResult();
        //    responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        //    Console.WriteLine(responseData);
        //}

        //public void Run(HttpClient client)
        //{
        //    var response = GetRequestAsync(client, "https://oauth.reddit.com/message/unread.json").GetAwaiter().GetResult();
        //    var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        //    Console.WriteLine(responseData);


        //    var obj = JObject.Parse(responseData);

        //    foreach (var commentData in obj["data"]["children"])
        //    {
        //        var commentName = commentData["data"]["name"].Value<string>();
        //        Console.WriteLine(commentName);

        //        var formData = new Dictionary<string, string>

        //        {
        //            { "grant_type", "json" },
        //            { "text", "bacon" },
        //            { "thing_id", commentName }
        //        };

        //        var encodedFormData = new FormUrlEncodedContent(formData);

        //        var authurl = "https://oauth.reddit.com/api/comment.json";
        //        var sendComment = PostRequestAsync(client, authurl, encodedFormData).GetAwaiter().GetResult();
        //        Console.WriteLine(sendComment);

        //    }

        //    var deleteMessages = "https://oauth.reddit.com/api/read_all_messages.json";
        //    var deleteMessagesData = PostRequestAsync(client, deleteMessages).GetAwaiter().GetResult();

        //}

    }
}
