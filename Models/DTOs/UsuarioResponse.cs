namespace UserManagementAPI.Models.DTOs
{
    public class UsuarioResponse
    {
        public long Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime UltimaConexion { get; set; }
        public string Pais { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
        public List<string> Permisos { get; set; } = new List<string>();
    }
} 