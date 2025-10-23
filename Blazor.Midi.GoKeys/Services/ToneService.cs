using Blazor.Midi.GoKeys.Models;
using System.Net.Http;

namespace Blazor.Midi.GoKeys.Services
{
    public class ToneService : IToneService
    {
        private readonly HttpClient _httpClient;
        private Dictionary<string, List<Tone>> _tonesByCategory = new();
        private List<Tone> _allTones = new();

        public bool IsInitialized { get; private set; }

        public ToneService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task InitializeAsync()
        {
            if (IsInitialized) return; 

            try
            {
                var csvContent = await _httpClient.GetStringAsync("data/tones.csv");
                _allTones = ParseCsv(csvContent);

                _tonesByCategory = _allTones
                    .GroupBy(p => p.Category)
                    .ToDictionary(g => g.Key, g => g.ToList());

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement Tones: {ex.Message}");
                throw;
            }
        }

        private List<Tone> ParseCsv(string csvContent)
        {
            var Tones = new List<Tone>();
            var lines = csvContent.Split('\n');

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var columns = line.Split(',');
                if (columns.Length < 6) continue;

                try
                {
                    Tones.Add(new Tone
                    {
                        Category = columns[0].Trim(),
                        Num = columns[1].Trim(),
                        Name = columns[2].Trim(),
                        MSB = int.Parse(columns[3].Trim()),
                        LSB = int.Parse(columns[4].Trim()),
                        PC = int.Parse(columns[5].Trim())
                    });
                }
                catch
                {
                    continue;
                }
            }

            return Tones;
        }

        public List<string> GetCategories()
            => _tonesByCategory.Keys.OrderBy(c => c).ToList();

        public List<Tone> GetTonesByCategory(string category)
            => _tonesByCategory.TryGetValue(category, out var list)
                ? list
                : new List<Tone>();

        public List<Tone> SearchTones(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _allTones;

            return _allTones
                .Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Num.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public Tone? GetToneByNum(string num)
            => _allTones.FirstOrDefault(p => p.Num == num);
    }
}
