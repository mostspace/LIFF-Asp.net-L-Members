using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace nxLINEadmin.Entity
{
    public class Member
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int member_id { get; set; } = 0;

        public int? member_pos_id { get; set; }

        [Required]
        [StringLength(100)]
        public string member_code { get; set; } = string.Empty;

        public int member_shop_id { get; set; } = 0;

        [Required]
        [StringLength(50)]
        public string member_lastname { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string member_firstname { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string member_lastname_kana { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string member_firstname_kana { get; set; } = string.Empty;

        [StringLength(10)]
        public string member_zipcode { get; set; } = string.Empty;

        [StringLength(10)]
        public string member_pref { get; set; } = string.Empty;

        [StringLength(1000)]
        public string member_address { get; set; } = string.Empty;

        [StringLength(20)]
        public string member_tel { get; set; } = string.Empty;

        [StringLength(20)]
        public string member_fax { get; set; } = string.Empty;

        [StringLength(20)]
        public string member_mobile { get; set; } = string.Empty;

        [StringLength(50)]
        [EmailAddress]
        public string member_email { get; set; } = string.Empty;

        public byte? member_gender { get; set; } = null;

        public DateTime? member_birthday { get; set; }

        public int? member_hold_point { get; set; }

        public DateTime? member_point_limit_date { get; set; }

        public DateTime? member_last_pointget_date { get; set; }

        public short? member_last_pointget_point { get; set; }

        public DateTime? member_last_visit_date { get; set; }

        public DateTime? member_join_date { get; set; }

        public DateTime? member_drop_date { get; set; }

        public byte? member_allow_email { get; set; }

        [StringLength(50)]
        public string member_rank { get; set; } = string.Empty;

        [MaxLength]
        public string member_note { get; set; } = string.Empty;

        [Required]
        public byte member_status { get; set; }

        [Required]
        public int member_ordinal { get; set; } = 0;

        [Required]
        public bool member_visibility { get; set; }

        [StringLength(50)]
        public string member_tag { get; set; } = string.Empty;

        [StringLength(1000)]
        public string member_nonce { get; set; } = string.Empty;

        [StringLength(100)]
        public string member_lineid { get; set; } = string.Empty;

        [StringLength(200)]
        public string member_stripeId { get; set; } = string.Empty;

        [MaxLength]
        public string member_password_hash { get; set; } = string.Empty;

        [MaxLength]
        public string member_password_salt { get; set; } = string.Empty;

        [MaxLength]
        public string member_email_verify_token { get; set; } = string.Empty;

        public DateTime? member_email_verify_expired_at { get; set; }

        [MaxLength]
        public string member_password_reset_token { get; set; } = string.Empty;

        public DateTime? member_password_reset_verify_expired_at { get; set; }

        [Required]
        public bool member_is_password_reset_verified { get; set; }

        [StringLength(50)]
        public string member_pending_email { get; set; } = string.Empty;

        [StringLength(50)]
        public string member_pending_email_verify_token { get; set; } = string.Empty;

        [Required]
        public bool member_is_signup_verified { get; set; }

        [MaxLength]
        public string member_signup_verify_token { get; set; } = string.Empty;

        [MaxLength]
        public string member_searchtext { get; set; } = string.Empty;

        [Required]
        public DateTime member_createat { get; set; }

        public DateTime? member_updateat { get; set; }

        public DateTime? member_deleteat { get; set; }
    }
}
