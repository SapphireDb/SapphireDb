using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SapphireDb.Command.Connection;
using SapphireDb.Helper;
using SapphireDb.Models;

namespace SapphireDb.Connection;

class ApiAuthMiddleware
{
    private readonly RequestDelegate next;
    private readonly SapphireDatabaseOptions options;

    public ApiAuthMiddleware(RequestDelegate next, SapphireDatabaseOptions options)
    {
        this.next = next;
        this.options = options;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!AuthHelper.CheckApiAuth(context.Request.Query["ApiKey"], context.Request.Query["ApiSecret"], options))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(JsonHelper.Serialize(new WrongApiResponse()).ToString());
            return;
        }

        await next(context);
    }
}