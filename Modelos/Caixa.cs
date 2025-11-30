namespace ProjetoEcommerce.Modelos
{
    public class Caixa
    {
        public int Id { get; set; }
        public int LojaId { get; set; }
        public decimal SaldoAtual { get; set; }
        public string Status { get; set; } = "Aberto"; // "Aberto" ou "Fechado"
        public DateTime DataAbertura { get; set; } // NOVA PROPRIEDADE
        public DateTime? DataFechamento { get; set; } // NOVA PROPRIEDADE
        public decimal SaldoInicial { get; set; } // NOVA PROPRIEDADE

        // Propriedades de navegação
        public virtual Loja Loja { get; set; }
        public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();

        // ENCAPSULAMENTO - Construtor
        public Caixa()
        {
            DataAbertura = DateTime.Now;
        }

        public Caixa(int lojaId, decimal saldoInicial) : this()
        {
            LojaId = lojaId;
            SaldoInicial = saldoInicial;
            SaldoAtual = saldoInicial;
        }

        // MÉTODOS COM ENCAPSULAMENTO
        public void FecharCaixa()
        {
            if (Status == "Fechado")
                throw new InvalidOperationException("Caixa já está fechado");

            Status = "Fechado";
            DataFechamento = DateTime.Now;
        }

        public void ReabrirCaixa()
        {
            if (Status == "Aberto")
                throw new InvalidOperationException("Caixa já está aberto");

            Status = "Aberto";
            DataFechamento = null;
        }

        public void AdicionarTransacao(Transacao transacao)
        {
            if (Status != "Aberto")
                throw new InvalidOperationException("Não é possível adicionar transação em caixa fechado");

            Transacoes.Add(transacao);

            if (transacao.Tipo == "Entrada")
                SaldoAtual += transacao.Valor;
            else if (transacao.Tipo == "Saida")
                SaldoAtual -= transacao.Valor;
        }

        // POLIMORFISMO - Método virtual
        public virtual decimal CalcularSaldoDoDia()
        {
            var transacoesDoDia = Transacoes
                .Where(t => t.Data.Date == DateTime.Today)
                .ToList();

            var totalEntradas = transacoesDoDia
                .Where(t => t.Tipo == "Entrada")
                .Sum(t => t.Valor);

            var totalSaidas = transacoesDoDia
                .Where(t => t.Tipo == "Saida")
                .Sum(t => t.Valor);

            return totalEntradas - totalSaidas;
        }

        // SOBRECARGA DE MÉTODOS
        public decimal CalcularSaldoPeriodo()
        {
            return CalcularSaldoPeriodo(DataAbertura, DateTime.Now);
        }

        public decimal CalcularSaldoPeriodo(DateTime dataInicio, DateTime dataFim)
        {
            var transacoesPeriodo = Transacoes
                .Where(t => t.Data >= dataInicio && t.Data <= dataFim)
                .ToList();

            var totalEntradas = transacoesPeriodo
                .Where(t => t.Tipo == "Entrada")
                .Sum(t => t.Valor);

            var totalSaidas = transacoesPeriodo
                .Where(t => t.Tipo == "Saida")
                .Sum(t => t.Valor);

            return totalEntradas - totalSaidas;
        }

        public bool EstaAberto()
        {
            return Status == "Aberto";
        }
    }
}