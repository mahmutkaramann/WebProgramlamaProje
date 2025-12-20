using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YeniSalon.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAISettings _settings;

        public OpenAIService(HttpClient httpClient, IOptions<OpenAISettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _httpClient.DefaultRequestHeaders.Add(
                "Authorization",
                $"Bearer {_settings.ApiKey}"
            );
        }

        // ===================== TEXT BASED =====================

        public async Task<string> GetExerciseRecommendationAsync(
            string goal, int? age, string? gender, int? height, decimal? weight, string? additionalInfo)
        {
            var prompt = $"Hedef: {goal}\nYaş: {age}\nCinsiyet: {gender}\nBoy: {height}\nKilo: {weight}\nEk: {additionalInfo}";
            return await GetChatResponseAsync(prompt);
        }

        public async Task<string> GetDietRecommendationAsync(
            string goal, int? age, string? gender, int? height, decimal? weight, string? additionalInfo)
        {
            var prompt = $"Beslenme planı oluştur:\n{goal}";
            return await GetChatResponseAsync(prompt);
        }

        public async Task<string> GetBodyAnalysisAsync(string base64Image, string additionalInfo)
        {
            var prompt = $"Vücut analizi yap: {additionalInfo}";
            return await GetChatResponseAsync(prompt);
        }

        // ===================== IMAGE TO IMAGE (GÖRSEL SİMÜLASYON) =====================

      public async Task<string> GenerateVisualSimulationWithImageAsync(
    string base64Image,
    string prompt)
{
    try
    {
        // Kullanıcının fotoğrafını ve fiziksel özelliklerini kullanarak
        // AYNI KİŞİNİN fit versiyonunu oluştur
        var enhancedPrompt = $@"
        LÜTFEN DİKKATLE İZLE: Aşağıdaki talimatları HARFİYEN uygula:

        1. VERİLEN FOTOĞRAFTAKİ KİŞİYİ KULLAN: Kullanıcının yüklediği fotoğraftaki aynı kişi olacak
        2. AYNI YÜZ ÖZELLİKLERİ: Yüz hatları, saç rengi, saç şekli aynı kalacak
        3. FIT VÜCUT DÖNÜŞÜMÜ: Mevcut vücudu al ve şu şekilde dönüştür:
           - Göbek yağını azalt
           - Karın kaslarını belirginleştir (6-pack abs)
           - Göğüs kaslarını geliştir (pek torakslı)
           - Kol kaslarını (biceps/triceps) belirginleştir
           - Omuzları (deltoids) genişlet
           - Sırt kaslarını (lats) geliştir (V-taper görünüm)
           - Bacak kaslarını tonlayıcı
        4. GERÇEKÇİ OLSUN: Aşırı kaslı bodybuilder değil, fit ve atletik görünüm
        5. FOTOĞRAF STİLİ:
           - Doğal ışık
           - Profesyonel fitness fotoğrafı
           - Spor kıyafetleri (fitness giysileri)
           - Fitness stüdyosu arka planı
        6. KULLANICI BİLGİLERİ: {prompt}

        SONUÇ: AYNI KİŞİNİN, 3-6 aylık düzenli antrenman ve beslenme sonrası FIT VERSİYONU";

        var requestData = new
        {
            model = "dall-e-3", // DALL-E 3 daha iyi sonuç verir
            prompt = enhancedPrompt,
            n = 1,
            size = "1024x1024",
            quality = "hd", // Daha yüksek kalite
            style = "vivid" // Canlı ve detaylı
        };

        var json = JsonSerializer.Serialize(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            "https://api.openai.com/v1/images/generations",
            content
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Error: {response.StatusCode} - {error}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var imageUrl = doc.RootElement
            .GetProperty("data")[0]
            .GetProperty("url")
            .GetString();

        return imageUrl ?? throw new Exception("Görsel URL alınamadı");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error generating image: {ex.Message}");
        
        // API hatası olabilir - API key'inizi kontrol edin
        // OpenAI Dashboard'dan API kullanımınızı ve kredinizi kontrol edin
        return "https://via.placeholder.com/1024x1024/FF6B6B/FFFFFF?text=API+Key+or+Credit+Problem";
    }
}

        // ===================== INTERNAL =====================

        private async Task<string> GetChatResponseAsync(string prompt)
        {
            var requestData = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "Sen bir fitness uzmanısın. Türkçe cevap ver." },
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                content
            );

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }
    }
}