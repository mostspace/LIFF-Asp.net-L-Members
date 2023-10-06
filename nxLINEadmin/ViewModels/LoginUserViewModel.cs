using System.ComponentModel.DataAnnotations;

namespace nxLINEadmin.ViewModels
{
    public class LoginUserViewModel
    {
        [Required(ErrorMessage = "ユーザー名を入力してください")]
        [StringLength(100)]
        public string UserLoginId { get; set; } = null!;

        [Required(ErrorMessage = "パスワードを入力してください")]
        [StringLength(100)]
        public string Password { get; set; } = null!;
    }
}
