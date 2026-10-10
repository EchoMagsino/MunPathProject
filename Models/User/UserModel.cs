using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using MunPath.Models.Roles;

namespace MunPath.Models.User
{
    [Table("User")]
    public class UserModel
    {
        public int Id {get; set;}

        [Required]
        [StringLength(50)]
        [Column("first_name")]
        public string FirstName {get; set;} = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("last_name")]
        public string LastName {get; set;} = string.Empty;

        [Required]
        [StringLength(254)]
        [Column("email")]
        public string Email {get; set;} = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Column("password")]
        public string PasswordHash {get; set;} = string.Empty;

        [Column("email_verified")]
        public bool EmailVerified { get; set; } = false;

        //foreign key of role
        [Column("role_name")]
        public string RoleName {get; set;} = string.Empty;

        [ForeignKey(nameof(RoleName))]

        public RoleModel Role {get; set;} = null!;
        

        
    }



}
