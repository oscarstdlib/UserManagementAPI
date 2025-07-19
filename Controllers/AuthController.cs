using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using UserManagementAPI.Data;
using UserManagementAPI.Models;
using UserManagementAPI.Models.DTOs;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, JwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for user: {Email}", request.Email);

                var usuario = await _context.Usuarios
                    .Include(u => u.Permisos)
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.Activo);

                if (usuario == null)
                {
                    _logger.LogWarning("Login failed: User not found or inactive - {Email}", request.Email);
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                var hashedPassword = HashPassword(request.Password);
                if (usuario.Password != hashedPassword)
                {
                    _logger.LogWarning("Login failed: Invalid password for user - {Email}", request.Email);
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Update last connection
                usuario.UltimaConexion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateToken(usuario);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var response = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(8),
                    Usuario = new UsuarioResponse
                    {
                        Id = usuario.Id,
                        NombreCompleto = usuario.NombreCompleto,
                        Email = usuario.Email,
                        UltimaConexion = usuario.UltimaConexion,
                        Pais = usuario.Pais,
                        TipoUsuario = usuario.TipoUsuario,
                        FechaCreacion = usuario.FechaCreacion,
                        Activo = usuario.Activo,
                        Permisos = usuario.Permisos.Select(p => $"{p.Recurso}:{p.Accion}").ToList()
                    }
                };

                _logger.LogInformation("Login successful for user: {Email}", request.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", request.Email);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
} 