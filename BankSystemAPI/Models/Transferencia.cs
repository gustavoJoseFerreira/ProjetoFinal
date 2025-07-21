using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystemAPI.Models
{
    [Table("Transferencias")]
    public class Transferencia
    {
        [Key]
        public int TransferenciaID { get; set; }

        [Required]
        public int ContaOrigemID { get; set; }

        [Required]
        public int ContaDestinoID { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        public DateTime DataTransferencia { get; set; } = DateTime.UtcNow;

        [StringLength(250)]
        public string Descricao { get; set; }

        // Navigation properties
        public ContaBancaria ContaOrigem { get; set; }
        public ContaBancaria ContaDestino { get; set; }
    }

    public class TransferenciaDto
    {
        [Required]
        public string IBANOrigem { get; set; }

        [Required]
        public string IBANDestino { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        public string Descricao { get; set; }
    }

    public class TransferenciaResponseDto
    {
        public int TransferenciaID { get; set; }
        public string IBANOrigem { get; set; }
        public string IBANDestino { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataTransferencia { get; set; }
        public string Descricao { get; set; }
    }
}