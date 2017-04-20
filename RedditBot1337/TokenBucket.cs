using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditBot1337
{
    class TokenBucket
    {
        public int currentTokens, capacity, interval;
        public DateTime lastRefreshed;

        public TokenBucket(int capacity, int intervalInSeconds)
        {
            this.currentTokens = capacity;
            this.capacity = capacity;
            this.interval = intervalInSeconds;
            this.lastRefreshed = DateTime.Now;
        }

        public bool RequestIsAllowed(int tokens)
        {
            Refill();
            if(currentTokens >= tokens)
            {
                currentTokens = currentTokens - tokens;
                return true;
            }
            return false;
        }

        public bool Refill()
        {
            if(DateTime.Now.Subtract(lastRefreshed).TotalSeconds >= interval) {
                currentTokens = capacity;
                lastRefreshed = DateTime.Now;
                return true;
            }
            return false; 
        }

        
    }
}
