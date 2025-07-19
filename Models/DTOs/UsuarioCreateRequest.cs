using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models.DTOs
{
    public class UsuarioCreateRequest
    {
        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Pais { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string TipoUsuario { get; set; } = string.Empty;
        
        public List<int> PermisoIds { get; set; } = new List<int>();
    }
} 