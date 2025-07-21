using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystemAPI.Models
{
    [Table("Permissoes")]
    public class Permissao
    {
        [Key]
        public int PermissaoID { get; set; }

        [Required]
        [StringLength(100)]
        public string NomePermissao { get; set; }

        public ICollection<UtilizadorPermissao> UtilizadorPermissoes { get; set; }
    }
}