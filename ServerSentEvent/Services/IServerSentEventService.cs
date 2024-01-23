using Microsoft.AspNetCore.Http;
using ServerSentEvent.Entities;
using System.Threading.Tasks;

namespace ServerSentEvent.Services
{
    public interface IServerSentEventService
    {
        Task AddAsync(HttpContext context);
        Task SendMessageAsync(ServerSentEventMessage message);
    }
}
