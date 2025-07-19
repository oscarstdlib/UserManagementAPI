using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Attributes;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaisesController : ControllerBase
    {
        private readonly CountriesService _countriesService;
        private readonly ILogger<PaisesController> _logger;

        public PaisesController(CountriesService countriesService, ILogger<PaisesController> logger)
        {
            _countriesService = countriesService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize("paises", "GET")]
        public async Task<ActionResult<List<string>>> GetPaises()
        {
            try
            {
                _logger.LogInformation("Fetching countries for user {UserId}", GetCurrentUserId());
                
                var paises = await _countriesService.GetCountryNamesAsync();
                
                _logger.LogInformation("Successfully retrieved {Count} countries for user {UserId}", paises.Count, GetCurrentUserId());
                
                return Ok(paises);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching countries for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { message = "Error al obtener los países" });
            }
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? long.Parse(userIdClaim.Value) : 0;
        }
    }
} 