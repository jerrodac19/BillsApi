using BillsApi.JsonConverters;
using System.Text.Json.Serialization;

namespace BillsApi.Dtos
{
    public class GoogleTaskCreateDto
    {
        public string? Id { get; set; }

        public required string Title { get; set; }

        public string? Notes { get; set; }

        [JsonConverter(typeof(Rfc3339DateTimeConverter))]
        public DateTime? Due { get; set; }
    }
}
