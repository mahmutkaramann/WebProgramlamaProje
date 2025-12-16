using System.Threading.Tasks;

// Test etmek için geçici olarak oluşturuldu.

namespace YeniSalon.Services
{
    public class MockOpenAIService : IOpenAIService
    {
        public async Task<string> GetExerciseRecommendationAsync(string goal, int? age, string? gender, int? height, decimal? weight, string? additionalInfo)
        {
            await Task.Delay(1000); // Simüle edilmiş gecikme

            return $@"## Egzersiz Programı Önerisi

**Hedef:** {goal}

**Haftalık Program:**
- Pazartesi: Göğüs ve Triceps
  - Bench Press: 3x8-10
  - Incline Dumbbell Press: 3x10-12
  - Triceps Pushdown: 3x12-15
  
- Salı: Sırt ve Biceps
  - Pull-ups: 3xMax
  - Barbell Row: 3x8-10
  - Bicep Curl: 3x12-15
  
- Çarşamba: Dinlenme veya Hafif Kardiyo
  
- Perşembe: Bacak
  - Squat: 3x8-10
  - Leg Press: 3x10-12
  - Leg Curl: 3x12-15
  
- Cuma: Omuz
  - Military Press: 3x8-10
  - Lateral Raise: 3x12-15
  - Face Pull: 3x15-20

**Beslenme Tavsiyeleri:**
- Günde 2-3 litre su tüketin
- Protein alımını artırın
- Kompleks karbonhidratları tercih edin";
        }

        public async Task<string> GetDietRecommendationAsync(string goal, int? age, string? gender, int? height, decimal? weight, string? additionalInfo)
        {
            await Task.Delay(1000);

            return $@"## Beslenme Programı

**Günlük Kalori İhtiyacı:** ~2500 kcal

**Öğün Örnekleri:**

**Kahvaltı (07:00):**
- 3 yumurta (1 tam, 2 beyaz)
- 1 dilim tam buğday ekmeği
- 5-6 zeytin
- Bol yeşillik

**Ara Öğün (10:30):**
- 1 avuç badem veya ceviz
- 1 meyve

**Öğle Yemeği (13:00):**
- 150-200g ızgara tavuk/balık
- Bol salata
- 4-5 yemek kaşığı bulgur pilavı

**Antrenman Öncesi (16:00):**
- 1 muz
- 1 bardak süt

**Akşam Yemeği (20:00):**
- 1 kase sebze yemeği
- 1 kase yoğurt
- Salata";
        }

        public async Task<string> GetBodyAnalysisAsync(string base64Image, string additionalInfo)
        {
            await Task.Delay(1000);
            return "Vücut analizi başarıyla tamamlandı. Öneriler:\n\n1. Ektomorf vücut tipine sahipsiniz\n2. Kas kütlesi artırımı için ağırlık antrenmanı önerilir\n3. Kardiyo süresini sınırlı tutun\n4. Protein alımını artırın";
        }

        public async Task<string> GenerateVisualSimulationAsync(string description)
        {
            await Task.Delay(1000);
            return "https://example.com/simulated-image.jpg";
        }
    }
}