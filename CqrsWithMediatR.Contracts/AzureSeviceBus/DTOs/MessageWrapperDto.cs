using System.Text.Json;

namespace CqrsWithMediatR.Contracts.AzureSeviceBus.DTOs
{
    public class MessageWrapperDto
    {
        public required string EventType { get; set; }
        public JsonElement Payload { get; set; }
    }
}
