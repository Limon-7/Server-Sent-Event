using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ServerSentEvent.Services;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ServerSentEvent.Middlewares
{
    public static class ServerSentEventMiddlewareMapper
    {
        public static IApplicationBuilder MapServerSentEventMiddleware(this IApplicationBuilder app, PathString path)
        {
            return app.Map(path, (app) => app.UseMiddleware<ServerSentEventMiddleware>());
        }
    }
    public class ServerSentEventMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IServerSentEventService service;

        public ServerSentEventMiddleware(RequestDelegate next,
            IServerSentEventService service)
        {
            this.next = next;
            this.service = service;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            await service.AddAsync(context);
        }
    }
}
