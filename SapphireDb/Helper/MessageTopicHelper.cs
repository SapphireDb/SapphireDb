using System;
using System.Collections.Generic;
using SapphireDb.Models;

namespace SapphireDb.Helper
{
    static class MessageTopicHelper
    {
        public static readonly Dictionary<string, Tuple<Func<HttpInformation, bool>, Func<HttpInformation, bool>>> RegisteredTopicAuthFunctions
            = new Dictionary<string, Tuple<Func<HttpInformation, bool>, Func<HttpInformation, bool>>>(StringComparer.InvariantCultureIgnoreCase);

        public static bool IsAllowedForSubscribe(string topic, HttpInformation httpInformation)
        {
            if (RegisteredTopicAuthFunctions.TryGetValue(topic, out var topicAuthFunction) && topicAuthFunction.Item1 != null)
            {
                return topicAuthFunction.Item1(httpInformation);
            }

            return true;
        }

        public static bool IsAllowedForPublish(string topic, HttpInformation httpInformation)
        {
            if (RegisteredTopicAuthFunctions.TryGetValue(topic, out var topicAuthFunction) && topicAuthFunction.Item2 != null)
            {
                return topicAuthFunction.Item2(httpInformation);
            }

            return true;
        }
    }
}