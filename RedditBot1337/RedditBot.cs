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
        private MessageHandler _handler;
        private HttpClient _client;

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
            //loop

            //kör bara varje 10 sekund, så om det går  fortare än 10 sekunder ska den sova
            //annars så ska den köra direkt
            while (true)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                _handler.Run();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                var timeToSleep = 10000 - elapsedMs;

                if (timeToSleep > 0)
                {
                    System.Threading.Thread.Sleep( (int) timeToSleep);
                    Console.WriteLine("Snark");
                }

            }
            
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
    }
}
