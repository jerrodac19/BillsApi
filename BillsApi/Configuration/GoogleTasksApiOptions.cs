namespace BillsApi.Configuration
{
    public class GoogleTasksApiOptions
    {
        public string BaseUrl { get; set; } = "https://tasks.googleapis.com/tasks/v1/";
        public string DefaultTaskListId { get; set; } = "@default";
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string RefreshToken { get; set; }
    }
}
