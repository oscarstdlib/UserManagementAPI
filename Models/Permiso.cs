using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models
{
    public class Permiso
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Recurso { get; set; } = string.Empty; // API endpoint or resource
        
        [Required]
        [StringLength(20)]
        public string Accion { get; set; } = string.Empty; // GET, POST, PUT, DELETE
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
} 