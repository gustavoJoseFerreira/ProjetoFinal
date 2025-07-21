using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystemAPI.Models
{
    [Table("Backups")]
    public class Backup
    {
        [Key]
        public int BackupID { get; set; }

        [Required]
        [StringLength(200)]
        public string NomeArquivo { get; set; }

        public DateTime DataBackup { get; set; } = DateTime.UtcNow;

        public decimal TamanhoMB { get; set; }
    }
}