using System;
using Microsoft.AspNetCore.Http;

namespace SapphireDb.Helper;

public static class SapphireAuthenticationHelper
{
    public static void CheckAndSetToken(HttpRequest request, Action<string> tokenSetter)
    {
        var accessToken = request.Query["access_token"];

        // If the request is for our hub...
        var path = request.Path;
        if (!string.IsNullOrEmpty(accessToken) &&
            (path.StartsWithSegments("/sapphire/hub")))
        {
            // Read the token out of the query string
            tokenSetter.Invoke(accessToken);
        }
    }
}