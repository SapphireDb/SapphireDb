using System;
using System.Collections.Generic;
using System.Linq;
using SapphireDb.Models;

namespace SapphireDb.Helper
{
    static class MessageTopicHelper
    {
        public static readonly Dictionary<string, Tuple<Func<HttpInformation, bool>, Func<HttpInformation, bool>>>
            RegisteredTopicAuthFunctions
                = new Dictionary<string, Tuple<Func<HttpInformation, bool>, Func<HttpInformation, bool>>>(StringComparer
                    .InvariantCultureIgnoreCase);

        public static bool IsAllowedForSubscribe(string topic, HttpInformation httpInformation)
        {
            return RegisteredTopicAuthFunctions
                .Where(authFunction => authFunction.Value.Item1 != null &&
                                       (topic.MatchesGlobPattern(authFunction.Key) ||
                                        authFunction.Key.MatchesGlobPattern(topic)))
                .All(authFunction => authFunction.Value.Item1(httpInformation));
        }

        public static bool IsAllowedForPublish(string topic, HttpInformation httpInformation)
        {
            return RegisteredTopicAuthFunctions
                .Where(authFunction => authFunction.Value.Item2 != null &&
                                       topic.MatchesGlobPattern(authFunction.Key))
                .All(authFunction => authFunction.Value.Item2(httpInformation));
        }
    }
}