namespace ProjetoEcommerce.Modelos
{
    public class Estoque
    {
        public int Id { get; set; }
        public int ProdutoFisicoId { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public int QuantidadeReservada { get; set; }
        public int PontoRepor { get; set; }
        public int EstoqueMinimo { get; set; }
        public DateTime? UltimoMovimento { get; set; }

        // Propriedade de navegação
        public virtual ProdutoFisico ProdutoFisico { get; set; }

        // MÉTODOS QUE ESTAVAM FALTANDO:
        public void AdicionarEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");

            QuantidadeDisponivel += quantidade;
            UltimoMovimento = DateTime.Now;
        }

        public void BaixarEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");

            if (QuantidadeDisponivel < quantidade)
                throw new InvalidOperationException("Estoque insuficiente");

            QuantidadeDisponivel -= quantidade;
            UltimoMovimento = DateTime.Now;
        }

        public void Reservar(int quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");

            if (QuantidadeDisponivel - QuantidadeReservada < quantidade)
                throw new InvalidOperationException("Estoque disponível insuficiente para reserva");

            QuantidadeReservada += quantidade;
            UltimoMovimento = DateTime.Now;
        }

        public void LiberarReserva(int quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");

            if (QuantidadeReservada < quantidade)
                throw new InvalidOperationException("Quantidade de reserva insuficiente");

            QuantidadeReservada -= quantidade;
            UltimoMovimento = DateTime.Now;
        }

        public string StatusEstoque()
        {
            var estoqueReal = EstoqueReal();
            if (estoqueReal <= EstoqueMinimo)
                return "Crítico";
            if (estoqueReal <= PontoRepor)
                return "Atenção";
            return "Normal";
        }

        public bool PrecisaRepor()
        {
            return EstoqueReal() <= PontoRepor;
        }

        // MÉTODOS EXISTENTES:
        public bool TemEstoqueSuficiente(int quantidade)
        {
            return (QuantidadeDisponivel - QuantidadeReservada) >= quantidade;
        }

        public int EstoqueReal()
        {
            return QuantidadeDisponivel - QuantidadeReservada;
        }
    }
}