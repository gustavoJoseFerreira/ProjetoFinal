using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystemAPI.Models
{
    [Table("Movimentos")]
    public class Movimento
    {
        [Key]
        public int MovimentoID { get; set; }

        [Required]
        public int ContaID { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoMovimento { get; set; }  // "Credito" ou "Debito"

        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        public DateTime DataMovimento { get; set; } = DateTime.UtcNow;

        [StringLength(250)]
        public string Descricao { get; set; }

        // Navigation property
        public ContaBancaria Conta { get; set; }
    }

    public class MovimentoDto
    {
        public int MovimentoID { get; set; }
        public string TipoMovimento { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataMovimento { get; set; }
        public string Descricao { get; set; }
        public string IBAN { get; set; }
    }
}