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
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.Timeout = TimeSpan.FromSeconds(60); // Timeout süresini artır
        }

        public async Task<string> GetExerciseRecommendationAsync(string goal, int? age, string? gender, int? height, decimal? weight, string? additionalInfo)
        {
            var prompt = $@"Kullanıcı için kişiselleştirilmiş egzersiz programı oluştur:

**Kullanıcı Bilgileri:**
- Hedef: {goal ?? "Belirtilmemiş"}
- Yaş: {age?.ToString() ?? "Belirtilmemiş"}
- Cinsiyet: {gender ?? "Belirtilmemiş"}
- Boy: {height?.ToString() ?? "Belirtilmemiş"} cm
- Kilo: {weight?.ToString() ?? "Belirtilmemiş"} kg
- Ek Bilgiler: {additionalInfo ?? "Belirtilmemiş"}

**İstenen Format:**
1. Haftalık Egzersiz Programı (Gün gün)
2. Her Egzersiz için Set ve Tekrar Sayıları
3. Dinlenme Süreleri
4. Isınma ve Soğuma Önerileri
5. Güvenlik Uyarıları
6. İlerleme Tavsiyeleri

Lütfen detaylı, anlaşılır ve Türkçe bir program hazırla.";

            return await GetChatResponseAsync(prompt);
        }

        public async Task<string> GetDietRecommendationAsync(string goal, int? age, string? gender, int? height, decimal? weight, string? additionalInfo)
        {
            var prompt = $@"Kullanıcı için kişiselleştirilmiş beslenme programı oluştur:

**Kullanıcı Bilgileri:**
- Hedef: {goal ?? "Belirtilmemiş"}
- Yaş: {age?.ToString() ?? "Belirtilmemiş"}
- Cinsiyet: {gender ?? "Belirtilmemiş"}
- Boy: {height?.ToString() ?? "Belirtilmemiş"} cm
- Kilo: {weight?.ToString() ?? "Belirtilmemiş"} kg
- Ek Bilgiler: {additionalInfo ?? "Belirtilmemiş"}

**İstenen Format:**
1. Günlük Kalori İhtiyacı
2. Makro Besin Dağılımı (Protein, Karbonhidrat, Yağ)
3. Öğün Örnekleri (Kahvaltı, Öğle, Akşam, Ara Öğünler)
4. Su Tüketimi Tavsiyesi
5. Önerilen Gıdalar
6. Kaçınılması Gereken Gıdalar
7. Örnek Yemek Tarifleri (1-2 tane)

Lütfen detaylı, anlaşılır ve Türkçe bir program hazırla.";

            return await GetChatResponseAsync(prompt);
        }

        public async Task<string> GetBodyAnalysisAsync(string base64Image, string additionalInfo)
        {
            // NOT: OpenAI'nin Vision API'si için farklı bir endpoint gerekebilir
            // Şimdilik sadece metin tabanlı analiz yapıyoruz
            var prompt = $@"Kullanıcının vücut fotoğrafını analiz et ve değerlendirme yap:

**Kullanıcı Açıklaması:** {additionalInfo ?? "Belirtilmemiş"}

**Analiz İçeriği:**
1. Genel Vücut Kompozisyonu Değerlendirmesi
2. Olası Vücut Tipi (Ektomorf, Mezomorf, Endomorf)
3. Güçlü Yönler
4. Geliştirilmesi Gereken Alanlar
5. Önerilen Egzersiz Türleri
6. Dikkat Edilmesi Gereken Noktalar
7. 4 Haftalık Hedef Planı

Lütfen yapıcı, motive edici ve profesyonel bir dille Türkçe analiz hazırla.";

            return await GetChatResponseAsync(prompt);
        }

        public async Task<string> GenerateVisualSimulationAsync(string description)
        {
            // DALL-E API'si için farklı bir implementasyon gerekir
            // Şimdilik alternatif bir çözüm sunuyoruz
            return "Görsel simülasyon özelliği yakında eklenecektir. Şu anda metin tabanlı öneriler alabilirsiniz.";
        }

        private async Task<string> GetChatResponseAsync(string prompt)
        {
            try
            {
                // OpenAI API'sine istek hazırla
                var requestData = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "Sen bir fitness koçu ve beslenme uzmanısın. Türkçe cevap ver. Cevaplarında markdown formatını kullan. Kullanıcıya motive edici ve yardımcı ol."
                        },
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    temperature = 0.7,
                    max_tokens = 1500
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // API'ye istek gönder
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(responseContent);
                    var aiResponse = document.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    return aiResponse ?? "Yanıt alınamadı.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"API Hatası ({response.StatusCode}): {errorContent}";
                }
            }
            catch (HttpRequestException ex)
            {
                return $"Bağlantı hatası: {ex.Message}. Lütfen internet bağlantınızı kontrol edin.";
            }
            catch (TaskCanceledException)
            {
                return "İstek zaman aşımına uğradı. Lütfen daha sonra tekrar deneyin.";
            }
            catch (Exception ex)
            {
                return $"Beklenmeyen hata: {ex.Message}";
            }
        }
    }

    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-3.5-turbo";
    }
}