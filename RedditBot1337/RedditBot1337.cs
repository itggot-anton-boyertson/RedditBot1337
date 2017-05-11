﻿using System;
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
        private MessageHandler _handler;
        private HttpClient _client;
        // private MessageHandler _messageHandler;

        //constructor
        public RedditBot(string clientid, string clientsecret, string redditusername, string redditpassword, string clientversion)
        {
            _clientid = clientid;
            _clientsecret = clientsecret;
            _redditusername = redditusername;
            _redditpassword = redditpassword;
            _clientversion = clientversion;

            _tb = new TokenBucket(60, 60);

            _client = new HttpClient();
            SetBasicAuthenticationHeader();
            SetCustomUserAgent();
            Authenticate();

            _handler = new MessageHandler(_client, _tb);
            _handler.Run();
        }

        private void SetBasicAuthenticationHeader()
        {
            var AuthenticationArray = Encoding.ASCII.GetBytes($"{_clientid}:{ _clientsecret}");
            var EncodedAuthenticationString = Convert.ToBase64String(AuthenticationArray);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("basic", EncodedAuthenticationString);
        }

        private void SetCustomUserAgent()
        {
            _client.DefaultRequestHeaders.Add("user-agent", $"changemeclient / v{_clientversion} by { _redditusername}");
        }

        private void Authenticate()
        {
            var formData = new Dictionary<string, string>

                {
                { "grant_type", "password" },
                { "username", _redditusername },
                { "password", _redditpassword }
                };

            var encodedFormData = new FormUrlEncodedContent(formData);



            var authurl = "https://www.reddit.com/api/v1/access_token";

            if (_tb.RequestIsAllowed(1))
            {
                var response = _client.PostAsync(authurl, encodedFormData).GetAwaiter().GetResult();
                Console.WriteLine(response);
                var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var accessToken = JObject.Parse(responseData).SelectToken("access_token").ToString();

                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);

            }
            else
            {
                System.Environment.Exit(-1);
            }
            

        }

        

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
