using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystemAPI.Models
{
    [Table("UtilizadorPermissoes")]
    public class UtilizadorPermissao
    {
        public int UtilizadorID { get; set; }
        public Utilizador Utilizador { get; set; }

        public int PermissaoID { get; set; }
        public Permissao Permissao { get; set; }
    }
}