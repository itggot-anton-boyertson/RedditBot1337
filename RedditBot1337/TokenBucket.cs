using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditBot1337
{

    /// <summary>
    /// Limit number of requests apache
    /// </summary>
    class TokenBucket
    {
        public int currentTokens, capacity, interval;
        public DateTime lastRefreshed;

        /// <summary>
        /// Control this current amount of Tokens, The total capacity, the time interval for bucket refill, time since last refill
        /// </summary>
        /// <param name="capacity"> The amount of request that can be made in intervalInSeconds(The time set för token bucket refill) </param>
        /// <param name="intervalInSeconds"> The time set för token bucket refill </param>
        public TokenBucket(int capacity, int intervalInSeconds)
        {
            this.currentTokens = capacity;
            this.capacity = capacity;
            this.interval = intervalInSeconds;
            this.lastRefreshed = DateTime.Now;
        }

        /// <summary>
        /// Check if token usage is allowed
        /// </summary>
        /// <param name="tokens"> The amount of tokens wanting to be used </param>
        /// <returns> Boolean </returns>
        /// <example>
        /// 
        /// var tokenBucket = new TokenBucket(currentTokens, capacity, interval, lastRefreshed);
        /// tokenBucket.RequestIsAllowed(tokens);
        /// 
        /// </example>
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

        /// <summary>
        /// Refill token bucket 
        /// </summary>
        /// <returns> BooLean </returns>
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
