using System.Text.Json.Serialization;

namespace Pandora.Admin.WebAPI.Models;

public class GetSessionResponseModel
{
    
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("authProvider")]
    public string AuthProvider { get; set; }

    [JsonPropertyName("expires")]
    public string Expires { get; set; }

    [JsonPropertyName("user")]
    public User1 User { get; set; }
    
    
    public class User1
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("groups")]
        public string[] Groups { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("image")]
        public object Image { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("picture")]
        public object Picture { get; set; }
    }
    
}