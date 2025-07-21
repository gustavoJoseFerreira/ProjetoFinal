using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystemAPI.Models
{
    [Table("ContasBancarias")]
    public class ContaBancaria
    {
        [Key]
        public int ContaID { get; set; }

        [Required]
        public int UtilizadorID { get; set; }

        [Required]
        [StringLength(34)]
        public string IBAN { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoConta { get; set; }  // "Conta Corrente", "Poupança", etc.

        [Range(0, double.MaxValue)]
        public decimal Saldo { get; set; } = 0;

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Ativa";  // "Ativa", "Bloqueada", "Encerrada"

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime? DataEncerramento { get; set; }

        // Navigation properties
        public Utilizador Utilizador { get; set; }
        public ICollection<Movimento> Movimentos { get; set; }
        public ICollection<Transferencia> TransferenciasEnviadas { get; set; }
        public ICollection<Transferencia> TransferenciasRecebidas { get; set; }
        public ICollection<Pagamento> Pagamentos { get; set; }
    }

    public class ContaBancariaCreateDto
    {
        [Required]
        public string TipoConta { get; set; }
    }

    public class ContaBancariaDto
    {
        public int ContaID { get; set; }
        public string IBAN { get; set; }
        public string TipoConta { get; set; }
        public decimal Saldo { get; set; }
        public string Estado { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}