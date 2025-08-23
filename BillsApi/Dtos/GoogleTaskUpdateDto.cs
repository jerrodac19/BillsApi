using BillsApi.JsonConverters;
using System.Text.Json.Serialization;

namespace BillsApi.Dtos
{
    public class GoogleTaskUpdateDto
    {
        public string? Title { get; set; }

        public string? Status { get; set; }

        [JsonConverter(typeof(Rfc3339DateTimeConverter))]
        public DateTime? Due { get; set; }
    }
}
