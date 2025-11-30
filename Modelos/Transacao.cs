namespace ProjetoEcommerce.Modelos
{
    public class Transacao
    {
        public int Id { get; set; }
        public int CaixaId { get; set; }
        public string Tipo { get; set; } // "Entrada" ou "Saida"
        public string Categoria { get; set; } // "Venda", "Estorno", "Despesa", etc.
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string Descricao { get; set; }
        public int? PedidoId { get; set; }
        public string MetodoPagamento { get; set; }
        public string Observacao { get; set; }

        // Propriedade de navegação
        public virtual Caixa Caixa { get; set; }

        // ENCAPSULAMENTO - Construtor
        public Transacao()
        {
            Data = DateTime.Now;
        }

        public Transacao(int caixaId, string tipo, string categoria, decimal valor, string descricao) : this()
        {
            CaixaId = caixaId;
            Tipo = tipo;
            Categoria = categoria;
            Valor = valor;
            Descricao = descricao;
        }

        // MÉTODOS COM ENCAPSULAMENTO
        public bool EhEntrada()
        {
            return Tipo == "Entrada";
        }

        public bool EhSaida()
        {
            return Tipo == "Saida";
        }

        // POLIMORFISMO - Método virtual
        public virtual bool Validar()
        {
            return !string.IsNullOrWhiteSpace(Tipo) &&
                   !string.IsNullOrWhiteSpace(Categoria) &&
                   !string.IsNullOrWhiteSpace(Descricao) &&
                   Valor > 0 &&
                   CaixaId > 0;
        }

        // SOBRECARGA DE MÉTODOS
        public string ObterResumo()
        {
            return $"{Tipo} - {Categoria}: R$ {Valor:F2}";
        }

        public string ObterResumo(bool incluirData)
        {
            var resumo = ObterResumo();
            return incluirData ? $"{Data:dd/MM/yyyy} - {resumo}" : resumo;
        }

        public bool EhDoDia()
        {
            return Data.Date == DateTime.Today;
        }

        public bool EhDoMes()
        {
            return Data.Year == DateTime.Now.Year && Data.Month == DateTime.Now.Month;
        }
    }
}