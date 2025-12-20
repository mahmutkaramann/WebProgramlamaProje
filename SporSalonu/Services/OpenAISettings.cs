using YeniSalon.Services;
using Microsoft.Extensions.Options;

namespace YeniSalon.Services
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;  // appsettings.json içindeki Model alanı için
    }
}
