using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystemAPI.Models
{
    [Table("LogsAcesso")]
    public class LogAcesso
    {
        [Key]
        public int LogID { get; set; }

        [Required]
        public int UtilizadorID { get; set; }

        public DateTime DataHora { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string IP { get; set; }

        public bool Sucesso { get; set; }

        [StringLength(250)]
        public string Descricao { get; set; }

        // Navigation property
        public Utilizador Utilizador { get; set; }
    }
}