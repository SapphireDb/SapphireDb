using System;
using System.Collections.Generic;
using System.Linq;
using SapphireDb.Connection;

namespace SapphireDb.Helper
{
    static class MessageTopicHelper
    {
        public static readonly Dictionary<string, Tuple<Func<IConnectionInformation, bool>, Func<IConnectionInformation, bool>>>
            RegisteredTopicAuthFunctions
                = new Dictionary<string, Tuple<Func<IConnectionInformation, bool>, Func<IConnectionInformation, bool>>>(StringComparer
                    .InvariantCultureIgnoreCase);

        public static bool IsAllowedForSubscribe(string topic, IConnectionInformation connectionInformation)
        {
            return RegisteredTopicAuthFunctions
                .Where(authFunction => authFunction.Value.Item1 != null &&
                                       (topic.MatchesGlobPattern(authFunction.Key) ||
                                        authFunction.Key.MatchesGlobPattern(topic)))
                .All(authFunction => authFunction.Value.Item1(connectionInformation));
        }

        public static bool IsAllowedForPublish(string topic, IConnectionInformation connectionInformation)
        {
            return RegisteredTopicAuthFunctions
                .Where(authFunction => authFunction.Value.Item2 != null &&
                                       topic.MatchesGlobPattern(authFunction.Key))
                .All(authFunction => authFunction.Value.Item2(connectionInformation));
        }
    }
}