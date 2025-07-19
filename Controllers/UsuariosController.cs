using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserManagementAPI.Attributes;
using UserManagementAPI.Data;
using UserManagementAPI.Models;
using UserManagementAPI.Models.DTOs;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(ApplicationDbContext context, ILogger<UsuariosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize("usuarios", "GET")]
        public async Task<ActionResult<IEnumerable<UsuarioResponse>>> GetUsuarios()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserTipo = GetCurrentUserTipo();

                var query = _context.Usuarios
                    .Include(u => u.Permisos)
                    .AsQueryable();

                // Apply role-based filtering
                if (currentUserTipo == "Operador")
                {
                    query = query.Where(u => u.TipoUsuario == "Cliente");
                }
                else if (currentUserTipo == "Cliente")
                {
                    query = query.Where(u => u.Id == currentUserId);
                }

                var usuarios = await query.ToListAsync();

                var response = usuarios.Select(u => new UsuarioResponse
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Email = u.Email,
                    UltimaConexion = u.UltimaConexion,
                    Pais = u.Pais,
                    TipoUsuario = u.TipoUsuario,
                    FechaCreacion = u.FechaCreacion,
                    Activo = u.Activo,
                    Permisos = u.Permisos.Select(p => $"{p.Recurso}:{p.Accion}").ToList()
                });

                _logger.LogInformation("Retrieved {Count} users for user {UserId}", response.Count(), currentUserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        [Authorize("usuarios", "GET")]
        public async Task<ActionResult<UsuarioResponse>> GetUsuario(long id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserTipo = GetCurrentUserTipo();

                // Check if user can access this specific user
                if (currentUserTipo == "Cliente" && currentUserId != id)
                {
                    return Forbid();
                }

                var usuario = await _context.Usuarios
                    .Include(u => u.Permisos)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Apply role-based filtering for Operador
                if (currentUserTipo == "Operador" && usuario.TipoUsuario != "Cliente")
                {
                    return Forbid();
                }

                var response = new UsuarioResponse
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
                };

                _logger.LogInformation("Retrieved user {UserId} for user {CurrentUserId}", id, currentUserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize("usuarios", "POST")]
        public async Task<ActionResult<UsuarioResponse>> CreateUsuario([FromBody] UsuarioCreateRequest request)
        {
            try
            {
                var currentUserTipo = GetCurrentUserTipo();

                // Only Administrador can create users
                if (currentUserTipo != "Administrador")
                {
                    return Forbid();
                }

                // Check if email already exists
                if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new { message = "El email ya está registrado" });
                }

                var hashedPassword = HashPassword(request.Password);

                var usuario = new Usuario
                {
                    NombreCompleto = request.NombreCompleto,
                    Email = request.Email,
                    Password = hashedPassword,
                    Pais = request.Pais,
                    TipoUsuario = request.TipoUsuario,
                    FechaCreacion = DateTime.UtcNow,
                    UltimaConexion = DateTime.UtcNow,
                    Activo = true
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Add permissions if specified
                if (request.PermisoIds.Any())
                {
                    var permisos = await _context.Permisos
                        .Where(p => request.PermisoIds.Contains(p.Id))
                        .ToListAsync();

                    foreach (var permiso in permisos)
                    {
                        usuario.Permisos.Add(permiso);
                    }

                    await _context.SaveChangesAsync();
                }

                var response = new UsuarioResponse
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
                };

                _logger.LogInformation("Created user {UserId} by user {CurrentUserId}", usuario.Id, GetCurrentUserId());
                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        [Authorize("usuarios", "PUT")]
        public async Task<IActionResult> UpdateUsuario(long id, [FromBody] UsuarioUpdateRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserTipo = GetCurrentUserTipo();

                var usuario = await _context.Usuarios
                    .Include(u => u.Permisos)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Check permissions
                if (currentUserTipo == "Cliente" && currentUserId != id)
                {
                    return Forbid();
                }

                if (currentUserTipo == "Operador" && usuario.TipoUsuario != "Cliente")
                {
                    return Forbid();
                }

                // Only Administrador can change user type
                if (currentUserTipo != "Administrador" && request.TipoUsuario != usuario.TipoUsuario)
                {
                    return Forbid();
                }

                // Check if email already exists (excluding current user)
                if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email && u.Id != id))
                {
                    return BadRequest(new { message = "El email ya está registrado" });
                }

                usuario.NombreCompleto = request.NombreCompleto;
                usuario.Email = request.Email;
                usuario.Pais = request.Pais;
                usuario.TipoUsuario = request.TipoUsuario;
                usuario.Activo = request.Activo;

                if (!string.IsNullOrEmpty(request.Password))
                {
                    usuario.Password = HashPassword(request.Password);
                }

                // Update permissions if specified and user is Administrador
                if (currentUserTipo == "Administrador" && request.PermisoIds.Any())
                {
                    usuario.Permisos.Clear();
                    var permisos = await _context.Permisos
                        .Where(p => request.PermisoIds.Contains(p.Id))
                        .ToListAsync();

                    foreach (var permiso in permisos)
                    {
                        usuario.Permisos.Add(permiso);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated user {UserId} by user {CurrentUserId}", id, currentUserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize("usuarios", "DELETE")]
        public async Task<IActionResult> DeleteUsuario(long id)
        {
            try
            {
                var currentUserTipo = GetCurrentUserTipo();

                // Only Administrador can delete users
                if (currentUserTipo != "Administrador")
                {
                    return Forbid();
                }

                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted user {UserId} by user {CurrentUserId}", id, GetCurrentUserId());
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? long.Parse(userIdClaim.Value) : 0;
        }

        private string GetCurrentUserTipo()
        {
            var tipoClaim = User.FindFirst("TipoUsuario");
            return tipoClaim?.Value ?? string.Empty;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
} 