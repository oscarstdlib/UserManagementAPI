using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserManagementAPI.Attributes;
using UserManagementAPI.Data;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermisosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PermisosController> _logger;

        public PermisosController(ApplicationDbContext context, ILogger<PermisosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize("permisos", "GET")]
        public async Task<ActionResult<IEnumerable<Permiso>>> GetPermisos()
        {
            try
            {
                var currentUserTipo = GetCurrentUserTipo();

                // Only Administrador can access permissions
                if (currentUserTipo != "Administrador")
                {
                    return Forbid();
                }

                var permisos = await _context.Permisos
                    .Where(p => p.Activo)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} permissions for user {UserId}", permisos.Count, GetCurrentUserId());
                return Ok(permisos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        [Authorize("permisos", "GET")]
        public async Task<ActionResult<Permiso>> GetPermiso(int id)
        {
            try
            {
                var currentUserTipo = GetCurrentUserTipo();

                // Only Administrador can access permissions
                if (currentUserTipo != "Administrador")
                {
                    return Forbid();
                }

                var permiso = await _context.Permisos.FindAsync(id);

                if (permiso == null)
                {
                    return NotFound(new { message = "Permiso no encontrado" });
                }

                _logger.LogInformation("Retrieved permission {PermissionId} for user {UserId}", id, GetCurrentUserId());
                return Ok(permiso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permission {PermissionId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize("permisos", "POST")]
        public async Task<ActionResult<Permiso>> CreatePermiso([FromBody] Permiso permiso)
        {
            try
            {
                var currentUserTipo = GetCurrentUserTipo();

                // Only Administrador can create permissions
                if (currentUserTipo != "Administrador")
                {
                    return Forbid();
                }

                permiso.FechaCreacion = DateTime.UtcNow;
                permiso.Activo = true;

                _context.Permisos.Add(permiso);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created permission {PermissionId} by user {UserId}", permiso.Id, GetCurrentUserId());
                return CreatedAtAction(nameof(GetPermiso), new { id = permiso.Id }, permiso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        [Authorize("permisos", "PUT")]
        public async Task<IActionResult> UpdatePermiso(int id, [FromBody] Permiso permiso)
        {
            try
            {
                var currentUserTipo = GetCurrentUserTipo();

                // Only Administrador can update permissions
                if (currentUserTipo != "Administrador")
                {
                    return Forbid();
                }

                var existingPermiso = await _context.Permisos.FindAsync(id);

                if (existingPermiso == null)
                {
                    return NotFound(new { message = "Permiso no encontrado" });
                }

                existingPermiso.Nombre = permiso.Nombre;
                existingPermiso.Descripcion = permiso.Descripcion;
                existingPermiso.Recurso = permiso.Recurso;
                existingPermiso.Accion = permiso.Accion;
                existingPermiso.Activo = permiso.Activo;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated permission {PermissionId} by user {UserId}", id, GetCurrentUserId());
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission {PermissionId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize("permisos", "DELETE")]
        public async Task<IActionResult> DeletePermiso(int id)
        {
            try
            {
                var currentUserTipo = GetCurrentUserTipo();

                // Only Administrador can delete permissions
                if (currentUserTipo != "Administrador")
                {
                    return Forbid();
                }

                var permiso = await _context.Permisos.FindAsync(id);

                if (permiso == null)
                {
                    return NotFound(new { message = "Permiso no encontrado" });
                }

                _context.Permisos.Remove(permiso);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted permission {PermissionId} by user {UserId}", id, GetCurrentUserId());
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting permission {PermissionId}", id);
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
    }
} 