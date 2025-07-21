using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystemAPI.Models
{

    [Table("Utilizadores")]
    public class Utilizador
    {
        [Key]
        public int UtilizadorID { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [StringLength(20)]
        public string Telemovel { get; set; }

        [Required]
        [StringLength(50)]
        public string Perfil { get; set; }  // "Cliente" ou "Administrador"

        public bool Ativo { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(9, MinimumLength = 9)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "NIF deve conter apenas números")]
        public string NIF { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 8)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Cartão Cidadão deve conter apenas números")]
        public string CartaoCidadao { get; set; }

        public DateTime? DataNascimento { get; set; }

        [StringLength(100)]
        public string Profissao { get; set; }

        [StringLength(250)]
        public string Morada { get; set; }

        // Navigation properties
        public ICollection<UtilizadorPermissao> UtilizadorPermissoes { get; set; }
        public ICollection<ContaBancaria> ContasBancarias { get; set; }
        public ICollection<Notificacao> Notificacoes { get; set; }
        public ICollection<LogAcesso> LogsAcesso { get; set; }
    }

    public class UtilizadorRegisterDto
    {
        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [StringLength(20)]
        public string Telemovel { get; set; }

        [Required]
        [StringLength(9, MinimumLength = 9)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "NIF deve conter apenas números")]
        public string NIF { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 8)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Cartão Cidadão deve conter apenas números")]
        public string CartaoCidadao { get; set; }

        public DateTime? DataNascimento { get; set; }

        [StringLength(100)]
        public string Profissao { get; set; }

        [StringLength(250)]
        public string Morada { get; set; }
    }

    public class UtilizadorLoginDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class UtilizadorProfileDto
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telemovel { get; set; }
        public string Perfil { get; set; }
        public string NIF { get; set; }
        public string CartaoCidadao { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string Profissao { get; set; }
        public string Morada { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}