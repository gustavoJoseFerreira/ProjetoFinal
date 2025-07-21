using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystemAPI.Models
{
    [Table("Pagamentos")]
    public class Pagamento
    {
        [Key]
        public int PagamentoID { get; set; }

        [Required]
        public int ContaID { get; set; }

        [Required]
        [StringLength(150)]
        public string Entidade { get; set; }

        [Required]
        [StringLength(50)]
        public string Referencia { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        public DateTime DataPagamento { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ContaBancaria Conta { get; set; }
    }

    public class PagamentoDto
    {
        [Required]
        public string IBAN { get; set; }

        [Required]
        [StringLength(150)]
        public string Entidade { get; set; }

        [Required]
        [StringLength(50)]
        public string Referencia { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }
    }

    public class PagamentoResponseDto
    {
        public int PagamentoID { get; set; }
        public string IBAN { get; set; }
        public string Entidade { get; set; }
        public string Referencia { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataPagamento { get; set; }
    }
}