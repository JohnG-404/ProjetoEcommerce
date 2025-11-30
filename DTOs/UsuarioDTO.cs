namespace ProjetoEcommerce.DTOs
{
    public class UsuarioDTO
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string Telefone { get; set; }
    }

    public class UsuarioResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Role { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    public class LoginDTO
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }
}