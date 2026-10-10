using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MunPath.Models.Roles
{
    [Table("Roles")]
    public class RoleModel
    {
        [Key]
        [Required]
        [Column("role_name")]
        [StringLength(30)]
        public string RoleName {get; set;} = string.Empty;

    }
}