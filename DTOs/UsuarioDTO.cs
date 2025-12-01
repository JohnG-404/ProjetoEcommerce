namespace ProjetoEcommerce.DTOs
{
    public class UsuarioDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
    }

    public class UsuarioResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
    }

    public class LoginDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}