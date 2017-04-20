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
    class RedditBot1337
    {

        private string _clientId;
        private string _clientSecret;
        private string _redditUsername;
        private string _redditPassword;
        private string _clientVersion;
        private TokenBucket _tb;

        public async Task<HttpResponseMessage> GetRequestAsync(HttpClient client, string method)
        {
            Console.WriteLine(method);
            Console.WriteLine(client);
            //Console.ReadKey();
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
            //Console.ReadKey();
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
            //Console.ReadKey();
            if (_tb.RequestIsAllowed(1))
            {
                var formData = new Dictionary<string, string>{ };
                var encodedFormData = new FormUrlEncodedContent(formData);

                return await client.PostAsync(method, encodedFormData);
            }
            else
            {
                System.Threading.Thread.Sleep(60000);
                return await PostRequestAsync(client, method);
            }
        }

        //Constructor
        public RedditBot1337(string clientId, string clientSecret, string redditUsername, string redditPassword, string clientVersion)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redditUsername = redditUsername;
            _redditPassword = redditPassword;
            _clientVersion = clientVersion;

            _tb = new TokenBucket(60, 60);

            using (var client = new HttpClient())
            {
                setBasicAuthenticationHeader(client);
                setCustomUserAgent(client);
                createFormData(client);
                var response = createFormData(client);
                tokenUsage(client, response);
                run(client);
            }

        }

        private void setBasicAuthenticationHeader(HttpClient client)
        {
            var authenticationArray = Encoding.ASCII.GetBytes($"{_clientId}:{ _clientSecret}");
            var encodedAuthenticationString = Convert.ToBase64String(authenticationArray);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedAuthenticationString);
        }

        private void setCustomUserAgent(HttpClient client)
        {
            client.DefaultRequestHeaders.Add("User-Agent", $"ChangeMeClient / v{_clientVersion} by { _redditUsername}");
        }

        private HttpResponseMessage createFormData(HttpClient client)
        {
            var formData = new Dictionary<string, string>

                {
                { "grant_type", "password" },
                { "username", _redditUsername },
                { "password", _redditPassword }
                };

            var encodedFormData = new FormUrlEncodedContent(formData);

            var authUrl = "https://www.reddit.com/api/v1/access_token";
            var response = client.PostAsync(authUrl, encodedFormData).GetAwaiter().GetResult();

            // Response Code
            Console.WriteLine(response.StatusCode);
            // Actual Token
            
            return response;
        }

        private void tokenUsage(HttpClient client, HttpResponseMessage response)
        {
            var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var accessToken = JObject.Parse(responseData).SelectToken("access_token").ToString();

            // Update AuthorizationHeader

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);

           client.GetAsync("https://oauth.reddit.com/api/v1/me").GetAwaiter().GetResult();
            responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine(responseData);
        }

        public void run(HttpClient client)
        {
            Console.WriteLine("wooot");
            var response = GetRequestAsync(client, "https://oauth.reddit.com/message/unread.json").GetAwaiter().GetResult();
            var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine(responseData);
            Console.ReadKey();
            

            var obj = JObject.Parse(responseData);
            
            foreach (var commentData in obj["data"]["children"])
            {
                var commentName = commentData["data"]["name"].Value<string>();
                Console.WriteLine(commentName);
                //Console.ReadKey();

                var formData = new Dictionary<string, string>

                {
                    { "grant_type", "json" },
                    { "text", "Bacon" },
                    { "thing_id", commentName }
                };

                var encodedFormData = new FormUrlEncodedContent(formData);

                var authUrl = "https://oauth.reddit.com/api/comment.json";
                var sendComment = PostRequestAsync(client, authUrl, encodedFormData).GetAwaiter().GetResult();
                Console.WriteLine(sendComment);
                Console.ReadKey();
                            
            }

            var deleteMessages = "https://oauth.reddit.com/api/read_all_messages.json";
            var sdeleteMessagesData = PostRequestAsync(client, deleteMessages).GetAwaiter().GetResult();

        }

    }
}
