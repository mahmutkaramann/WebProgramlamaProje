using System.Threading.Tasks;
using YeniSalon.Services;
using Microsoft.Extensions.Options;

namespace YeniSalon.Services
{
    public interface IOpenAIService
    {
        Task<string> GetExerciseRecommendationAsync(string goal, int? age, string? gender, int? height, decimal? weight, string? additionalInfo);
        Task<string> GetDietRecommendationAsync(string goal, int? age, string? gender, int? height, decimal? weight, string? additionalInfo);
        Task<string> GetBodyAnalysisAsync(string base64Image, string additionalInfo);
        
        // GÖRSEL SİMÜLASYON
        Task<string> GenerateVisualSimulationWithImageAsync(string base64Image, string prompt);
    }
}