using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using MunPath.Models.User;

namespace MunPathProject.Models
{
    [Table("student_academic_record")]
    public class StudentAcademicRecord
    {
        [Required]
        [Key]
        public int Id {get; set;}

        [Required]
        public int UserId {get; set;}
        
        [Column("strand")]
        public string? Strand {get; set;} = string.Empty;

        [Column("gwa")]
        [Range(typeof(decimal),"1.0", "5.0")]
        public decimal? GWA {get; set;}

        public UserModel? User {get; set;}    
    }
}