namespace ProjetoEcommerce.Modelos
{
    public class Estoque
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public int QuantidadeReservada { get; set; } // NOVA PROPRIEDADE
        public int PontoRepor { get; set; }
        public int EstoqueMinimo { get; set; } // NOVA PROPRIEDADE
        public DateTime? UltimoMovimento { get; set; }

        // Propriedade de navegação
        public virtual Produto Produto { get; set; }

        // ENCAPSULAMENTO - Métodos para gerenciar estoque
        public bool TemEstoqueSuficiente(int quantidade)
        {
            return (QuantidadeDisponivel - QuantidadeReservada) >= quantidade;
        }

        public int EstoqueReal()
        {
            return QuantidadeDisponivel - QuantidadeReservada;
        }

        public void Reservar(int quantidade)
        {
            if (!TemEstoqueSuficiente(quantidade))
                throw new InvalidOperationException("Estoque insuficiente para reserva");

            QuantidadeReservada += quantidade;
            UltimoMovimento = DateTime.Now;
        }

        public void LiberarReserva(int quantidade)
        {
            if (QuantidadeReservada < quantidade)
                throw new InvalidOperationException("Quantidade de reserva a liberar é maior que a reservada");

            QuantidadeReservada -= quantidade;
            UltimoMovimento = DateTime.Now;
        }

        public void BaixarEstoque(int quantidade)
        {
            if (!TemEstoqueSuficiente(quantidade))
                throw new InvalidOperationException("Estoque insuficiente para baixa");

            QuantidadeDisponivel -= quantidade;
            UltimoMovimento = DateTime.Now;
        }

        public void AdicionarEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");

            QuantidadeDisponivel += quantidade;
            UltimoMovimento = DateTime.Now;
        }

        // POLIMORFISMO - Método virtual que pode ser sobrescrito
        public virtual bool PrecisaRepor()
        {
            return EstoqueReal() <= PontoRepor;
        }

        // SOBRECARGA DE MÉTODOS
        public string StatusEstoque()
        {
            var estoqueReal = EstoqueReal();
            if (estoqueReal <= EstoqueMinimo)
                return "CRÍTICO";
            else if (estoqueReal <= PontoRepor)
                return "ALERTA";
            else
                return "NORMAL";
        }

        public string StatusEstoque(bool incluirQuantidade)
        {
            var status = StatusEstoque();
            return incluirQuantidade ? $"{status} ({EstoqueReal()} unidades)" : status;
        }
    }
}