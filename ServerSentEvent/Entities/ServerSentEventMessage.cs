using System.Text.Json.Serialization;

namespace ServerSentEvent.Entities
{
    public class ServerSentEventMessage
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = null!;
        [JsonPropertyName("message")]
        public string Message { get; init; }
        public string EventName { get; set; }
    }

    public class SseClientId
    {
        [JsonPropertyName("clientId")]
        public string ClientId { get; init; }
    }
}
