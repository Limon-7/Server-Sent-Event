using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerSentEvent.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ServerSentEvent.Services
{
    public record ServerSentEventClient(HttpResponse Response, CancellationTokenSource Cancel);

    public class ServerSentEventService : IServerSentEventService
    {
        private readonly ConcurrentDictionary<string, ServerSentEventClient> clients = new();
        public ServerSentEventService(IHostApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStopping.Register(OnShutdown);
        }

        public async Task AddAsync(HttpContext context)
        {
            var clientId = CreateId();
            var cancel = new CancellationTokenSource();
            var client = new ServerSentEventClient(Response: context.Response, Cancel: cancel);
            if (clients.TryAdd(clientId, client))
            {
                WriteHeaderAsync(clientId, client);
                context.RequestAborted.WaitHandle.WaitOne();
                RemoveClient(clientId);
                await Task.FromResult(true);
            }
        }

        public async Task SendMessageAsync(ServerSentEventMessage message)
        {
            foreach (var c in clients)
            {
                if (c.Key == message.Id)
                {
                    continue;
                }
                var messageJson = JsonSerializer.Serialize(message);
                await c.Value.Response.WriteAsync("event:" + message.EventName + "\n");
                await c.Value.Response.WriteAsync($"data: {messageJson}\r\r", c.Value.Cancel.Token);
                await c.Value.Response.Body.FlushAsync(c.Value.Cancel.Token);
            }
        }
        private async void WriteHeaderAsync(string clientId, ServerSentEventClient client, string eventName="connect")
        {
            try
            {
                var clientIdJson = JsonSerializer.Serialize(new SseClientId { ClientId = clientId });
                client.Response.Headers.Add("Content-Type", "text/event-stream");
                client.Response.Headers.Add("Cache-Control", "no-cache");
                client.Response.Headers.Add("Connection", "keep-alive");

                await client.Response.WriteAsync("event:" + eventName + "\n");
                // Send ID to client-side after connecting
                await client.Response.WriteAsync($"data: {clientIdJson}\r\r", client.Cancel.Token);
                await client.Response.Body.FlushAsync(client.Cancel.Token);
            }
            catch (OperationCanceledException ex)
            {
                
            }

        }
        private void OnShutdown()
        {
            var tmpClients = new List<KeyValuePair<string, ServerSentEventClient>>();
            foreach (var c in clients)
            {
                c.Value.Cancel.Cancel();
                tmpClients.Add(c);
            }
            foreach (var c in tmpClients)
            {
                clients.TryRemove(c);
            }
        }
        public void RemoveClient(string id)
        {
            var target = clients.FirstOrDefault(c => c.Key == id);
            if (string.IsNullOrEmpty(target.Key))
            {
                return;
            }
            target.Value.Cancel.Cancel();
            clients.TryRemove(target);
        }
        private string CreateId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
