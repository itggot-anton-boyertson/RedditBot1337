using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace RedditBot1337
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientId = ConfigurationManager.AppSettings["clientId"];
            var clientSecret = ConfigurationManager.AppSettings["clientSecret"];
            var redditUsername = ConfigurationManager.AppSettings["redditUsername"];
            var redditPassword = ConfigurationManager.AppSettings["redditPassword"];
            var clientVersion = ConfigurationManager.AppSettings["clientVersion"];

            var minBot = new RedditBot1337(clientId, clientSecret, redditUsername, redditPassword, clientVersion);
            //minBot.Run();
            

        }
    }
}
