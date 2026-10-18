using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using MunPath.Models.User;


namespace MunPath.Models.EmailVerification
{
    [Table("email_verifications")]
    public class EmailVerificationModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("otp_hash")]
        public string OtpHash { get; set; } = string.Empty;

        [Column("expires_at_utc")]
        public DateTime ExpiresAtUtc { get; set; }

        [Column("attemps")]
        public int Attempts { get; set; } = 0;

        [Column("created_at_utc")]
        public DateTime CreatedAtUtc { get; set; }
        
        [Column("is_used")]
        public bool IsUsed { get; set; } = false;

        public UserModel? users {get; set;}


    }
}