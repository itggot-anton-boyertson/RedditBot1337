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
    /// Run bot
    /// </summary>
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientid"> Bot id, as seen on "https://www.reddit.com/prefs/apps/" </param>
        /// <param name="clientsecret"> Bot id, as seen on "https://www.reddit.com/prefs/apps/" </param>
        /// <param name="redditusername"> Reddit username for login and authentication </param>
        /// <param name="redditpassword"> Reddit password for login and authentication </param>
        /// <param name="clientversion"> Client Version, personal to create an unique identity </param>
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

            /// Infinite loop
            /// check if _handler.Run takes more than if 10 seconds to execute. 
            /// If so, sleep for 10000 millisecond (10 seconds) minus time of execution
            /// If not, run again
            while (true)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                _handler.Run();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                var timeToSleep = 10000 - elapsedMs;

                if (timeToSleep > 0)
                {
                    System.Threading.Thread.Sleep((int)timeToSleep);
                }

            }

        }

        /// <summary>
        /// Configuring the client
        /// </summary>
        private void SetBasicAuthenticationHeader()
        {
            var AuthenticationArray = Encoding.ASCII.GetBytes($"{_clientid}:{ _clientsecret}");
            var EncodedAuthenticationString = Convert.ToBase64String(AuthenticationArray);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("basic", EncodedAuthenticationString);
        }

        /// <summary>
        /// Creating unique User Agent
        /// </summary>
        private void SetCustomUserAgent()
        {
            _client.DefaultRequestHeaders.Add("user-agent", $"changemeclient / v{_clientversion} by { _redditusername}");
        }

        /// <summary>
        /// Use reddit username and password to login to reddit
        /// (Also check if there is token, else exit)
        /// </summary>
        /// <example>
        /// 
        /// var redditBot = new RedditBot(clientId, clientSecret, redditUsername, redditPassword, clientVersion);
        /// redditBot.Authenticate
        /// 
        /// </example>
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
