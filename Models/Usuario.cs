using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models
{
    public class Usuario
    {
        [Key]
        public long Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
        
        public DateTime UltimaConexion { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(50)]
        public string Pais { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string TipoUsuario { get; set; } = string.Empty; // Administrador, Operador, Cliente
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        public bool Activo { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
    }
} 