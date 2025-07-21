using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystemAPI.Models
{
    [Table("LogsErros")]
    public class LogErro
    {
        [Key]
        public int ErroID { get; set; }

        public DateTime DataHora { get; set; } = DateTime.UtcNow;

        [StringLength(20)]
        public string Severidade { get; set; }  // "Info", "Warning", "Error", "Critical"

        public string Mensagem { get; set; }

        public string StackTrace { get; set; }
    }
}