using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace nxLINEadminAPI.Entity
{
    public class MemberRegSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int member_reg_id { get; set; } = 0;

        public bool member_reg_is_name { get; set; } = false;

        public bool member_reg_is_furigana { get; set; } = false;
        
        public bool member_reg_is_tel { get; set; } = false;
        
        public bool member_reg_is_email { get; set; } = false;
        
        public bool member_reg_is_birthday { get; set; } = false;
        
        public bool member_reg_is_gender { get; set; } = false;
        
        public bool member_reg_is_address { get; set; } = false;
        
        [StringLength(50)]
        public string overview { get; set; } = string.Empty;

    }
}
