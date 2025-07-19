namespace UserManagementAPI.Models.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UsuarioResponse Usuario { get; set; } = new UsuarioResponse();
    }
} 