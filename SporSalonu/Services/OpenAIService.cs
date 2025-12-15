using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace YeniSalon.Services
{
    public interface IOpenAIService
    {
        Task<string> GetExerciseRecommendationAsync(string userInput, int? age, string? gender,
                                                   int? height, decimal? weight, string? goal);
        Task<string> GetDietRecommendationAsync(string userInput, int? age, string? gender,
                                               int? height, decimal? weight, string? goal);
        Task<string> GetBodyAnalysisAsync(string imageBase64, string userInput);
        Task<string> GenerateVisualSimulationAsync(string description);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAISettings _settings;

        public OpenAIService(HttpClient httpClient, IOptions<OpenAISettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        }

        public async Task<string> GetExerciseRecommendationAsync(string userInput, int? age, string? gender,
                                                               int? height, decimal? weight, string? goal)
        {
            var prompt = BuildExercisePrompt(userInput, age, gender, height, weight, goal);
            return await CallOpenAIAsync(prompt);
        }

        public async Task<string> GetDietRecommendationAsync(string userInput, int? age, string? gender,
                                                           int? height, decimal? weight, string? goal)
        {
            var prompt = BuildDietPrompt(userInput, age, gender, height, weight, goal);
            return await CallOpenAIAsync(prompt);
        }

        public async Task<string> GetBodyAnalysisAsync(string imageBase64, string userInput)
        {
            var request = new
            {
                model = "gpt-4-vision-preview",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = $"Analiz yap: {userInput}" },
                            new
                            {
                                type = "image_url",
                                image_url = new { url = $"data:image/jpeg;base64,{imageBase64}" }
                            }
                        }
                    }
                },
                max_tokens = 2000
            };

            return await CallOpenAIVisionAsync(request);
        }

        public async Task<string> GenerateVisualSimulationAsync(string description)
        {
            var request = new
            {
                model = "dall-e-3",
                prompt = $"Fitness sonrası vücut simülasyonu: {description}. Gerçekçi, detaylı, fitness odaklı.",
                n = 1,
                size = "1024x1024"
            };

            return await CallDALLEAsync(request);
        }

        private string BuildExercisePrompt(string userInput, int? age, string? gender,
                                         int? height, decimal? weight, string? goal)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Aşağıdaki bilgilere göre kişiselleştirilmiş bir egzersiz programı öner:");

            if (!string.IsNullOrEmpty(userInput))
                sb.AppendLine($"Kullanıcı notu: {userInput}");
            if (age.HasValue)
                sb.AppendLine($"Yaş: {age}");
            if (!string.IsNullOrEmpty(gender))
                sb.AppendLine($"Cinsiyet: {gender}");
            if (height.HasValue)
                sb.AppendLine($"Boy: {height} cm");
            if (weight.HasValue)
                sb.AppendLine($"Kilo: {weight} kg");
            if (!string.IsNullOrEmpty(goal))
                sb.AppendLine($"Hedef: {goal}");

            sb.AppendLine("\nCevabını şu formatta ver:");
            sb.AppendLine("1. **Genel Değerlendirme:** [Değerlendirme]");
            sb.AppendLine("2. **Haftalık Program:**");
            sb.AppendLine("   - Pazartesi: [Egzersizler]");
            sb.AppendLine("   - Salı: [Egzersizler]");
            sb.AppendLine("   - ...");
            sb.AppendLine("3. **Set ve Tekrar Önerileri:**");
            sb.AppendLine("4. **İpuçları:**");
            sb.AppendLine("5. **Önerilen Süre:**");

            return sb.ToString();
        }

        private string BuildDietPrompt(string userInput, int? age, string? gender,
                                     int? height, decimal? weight, string? goal)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Aşağıdaki bilgilere göre kişiselleştirilmiş bir diyet programı öner:");

            if (!string.IsNullOrEmpty(userInput))
                sb.AppendLine($"Kullanıcı notu: {userInput}");
            if (age.HasValue)
                sb.AppendLine($"Yaş: {age}");
            if (!string.IsNullOrEmpty(gender))
                sb.AppendLine($"Cinsiyet: {gender}");
            if (height.HasValue)
                sb.AppendLine($"Boy: {height} cm");
            if (weight.HasValue)
                sb.AppendLine($"Kilo: {weight} kg");
            if (!string.IsNullOrEmpty(goal))
                sb.AppendLine($"Hedef: {goal}");

            sb.AppendLine("\nCevabını şu formatta ver:");
            sb.AppendLine("1. **Günlük Kalori İhtiyacı:** [Kalori]");
            sb.AppendLine("2. **Makro Dağılımı:**");
            sb.AppendLine("   - Protein: [g]");
            sb.AppendLine("   - Karbonhidrat: [g]");
            sb.AppendLine("   - Yağ: [g]");
            sb.AppendLine("3. **Günlük Öğün Planı:**");
            sb.AppendLine("   - Kahvaltı: [Öneriler]");
            sb.AppendLine("   - Öğle Yemeği: [Öneriler]");
            sb.AppendLine("   - Akşam Yemeği: [Öneriler]");
            sb.AppendLine("   - Ara Öğünler: [Öneriler]");
            sb.AppendLine("4. **Önerilen Besinler:**");
            sb.AppendLine("5. **Kaçınılması Gerekenler:**");

            return sb.ToString();
        }

        private async Task<string> CallOpenAIAsync(string prompt)
        {
            var request = new
            {
                model = "gpt-4",
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 2000,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);

            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "Cevap alınamadı.";
        }

        private async Task<string> CallOpenAIVisionAsync(object request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);

            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "Cevap alınamadı.";
        }

        private async Task<string> CallDALLEAsync(object request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/images/generations", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("data")[0].GetProperty("url").GetString() ?? "";
        }
    }

    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
    }

    public class OpenAIResponse
    {
        public List<Choice> Choices { get; set; } = new();
    }

    public class Choice
    {
        public Message Message { get; set; } = new();
    }

    public class Message
    {
        public string Content { get; set; } = string.Empty;
    }
}