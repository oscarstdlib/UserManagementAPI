using System.Text.Json;
using UserManagementAPI.Services;

namespace UserManagementAPI.Services
{
    public class CountriesService
    {
        private readonly HttpClient _httpClient;
        private readonly CacheService _cacheService;
        private readonly ILogger<CountriesService> _logger;

        public CountriesService(HttpClient httpClient, CacheService cacheService, ILogger<CountriesService> logger)
        {
            _httpClient = httpClient;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<List<CountryInfo>> GetCountriesAsync()
        {
            const string cacheKey = "countries:all";
            const string apiUrl = "https://restcountries.com/v3.1/all?fields=name";

            try
            {
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    _logger.LogInformation("Fetching countries from external API");
                    
                    var response = await _httpClient.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var countries = JsonSerializer.Deserialize<List<CountryInfo>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Successfully fetched {Count} countries from external API", countries?.Count ?? 0);
                    
                    return countries ?? new List<CountryInfo>();
                }, TimeSpan.FromHours(24)); // Cache for 24 hours
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching countries from external API");
                throw;
            }
        }

        public async Task<List<string>> GetCountryNamesAsync()
        {
            var countries = await GetCountriesAsync();
            return countries.Select(c => c.Name?.Common ?? c.Name?.Official ?? "Unknown").ToList();
        }
    }

    public class CountryInfo
    {
        public CountryName? Name { get; set; }
    }

    public class CountryName
    {
        public string? Common { get; set; }
        public string? Official { get; set; }
    }
} 