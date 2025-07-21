using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystemAPI.Models
{
    [Table("Notificacoes")]
    public class Notificacao
    {
        [Key]
        public int NotificacaoID { get; set; }

        [Required]
        public int UtilizadorID { get; set; }

        [Required]
        [StringLength(100)]
        public string Titulo { get; set; }

        [Required]
        [StringLength(500)]
        public string Mensagem { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;

        public bool Lida { get; set; } = false;

        // Navigation property
        public Utilizador Utilizador { get; set; }
    }

    public class NotificacaoDto
    {
        public int NotificacaoID { get; set; }
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        public DateTime DataEnvio { get; set; }
        public bool Lida { get; set; }
    }

    public class NotificacaoCreateDto
    {
        [Required]
        public int UtilizadorID { get; set; }

        [Required]
        [StringLength(100)]
        public string Titulo { get; set; }

        [Required]
        [StringLength(500)]
        public string Mensagem { get; set; }
    }
}