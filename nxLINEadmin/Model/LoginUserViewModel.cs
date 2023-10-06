using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace nxLINEadmin.Model
{
    public partial class UserModel
    {
        public int UserId { get; set; }
        public int Status { get; set; } = 1;

        [Required(ErrorMessage = "その名前はすでに使用されています")]
        [StringLength(100)]
        public string UserLoginId { get; set; }

        [Required(ErrorMessage = "パスワードを入力してください")]
        [StringLength(100)] 
        public string UserPwd { get; set; }

        [Required(ErrorMessage = "ユーザー名を入力してください")]
        [StringLength(100)] 
        public string UserName { get; set; }

        [Required(ErrorMessage = "メールアドレスの入力は必須です")]
        [EmailAddress]
        public string UserEmail { get; set; }
        public int? UserLineAccountID { get; set; }
        public string? UserLineAccountRole { get; set; }
        public DateTime? UserLastlogindatetime { get; set; }
        public DateTime? UpdateDatetime { get; set; }
    }
    public class LoginUserViewModel
    {
        [Required(ErrorMessage = "ユーザーIDを入力してください")]
        [StringLength(100)]
        public string UserLoginId { get; set; } = null!;

        [Required(ErrorMessage = "パスワードを入力してください")]
        [StringLength(100)]
        public string Password { get; set; } = null!;
    }

    public class RegisterUserViewModel : UserModel
    {
        [DisplayName("パスワード確認")]
        [Required(ErrorMessage = "パスワード確認")]
        [StringLength(100)]
        public string UserRePwd { get; set; }

    }
}
