using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace nxLINEadmin.Entity
{
    public class LineAccount
    {
        [Required]
        [Key]
        public int LineaccountId { get; set; }

        [Required]
        [MaxLength(100)]
        [DisplayName("LineaccountCode")]
        public string LineaccountCode { get; set; } = string.Empty;
        [Required]
        [MaxLength(5)]
        [DisplayName("lineaccount_shortcode")]
        public string LineaccountShortcode { get; set; } = string.Empty;
        [MaxLength(50)]
        [DisplayName("名前")]
        public string? LineaccountName { get; set; }
        [Required]
        [MaxLength(200)]
        [DisplayName("メールアドレス")]
        public string LineaccountEmail { get; set; } = string.Empty;
        [DisplayName("トーク機能")]
        public bool Istalk { get; set; } = false;
        [DisplayName("トーク自動応答メッセージ")]
        public string? TalkMessage { get; set; }
        [DisplayName("初回会員登録")]
        public bool IsProfile { get; set; } = false;
        public string? ProfileSetting { get; set; }
        [DisplayName("会員登録完了ポイント")]
        public int? EntryPoint { get; set; }
        [DisplayName("ポイント還元率")]
        public int? StartRank { get; set; }
        [DisplayName("ポイント失効期限")]
        public int? PointExpire { get; set; }
        [MaxLength(10)]
        [DisplayName("会員証デフォルトカラー")]
        public string? MembersCardColor { get; set; }
        [MaxLength(1000)]
        [DisplayName("会員証ヘッダー画像")]
        public string? MembersCardDesignUrl { get; set; }
        [DisplayName("メンバーズカード使用時のカメラ状態")]
        public bool MembersCardIsUseCamera { get; set; } = false;
        [MaxLength(20)]
        [DisplayName("メンバーズカードリフID")]
        public string? MembersCardLiffId { get; set; }
        [MaxLength(10)]
        [DisplayName("LINEチャンネルID")]
        public string? LineChannelId { get; set; }
        [MaxLength(50)]
        [DisplayName("ラインチャンネルの秘密")]
        public string? LineChannelSecret { get; set; }
        [MaxLength(200)]
        public string? LineChannelAccessToken { get; set; }
        [DisplayName("スマレジ連携")]
        public bool IsSmaregi { get; set; } = false;
        [MaxLength(20)]
        [DisplayName("スマレギ契約ID")]
        public string? SmaregiContractId { get; set; }
        [MaxLength(1000)]
        [DisplayName("LINEアカウントのロゴ")]
        public string? LineaccountLogoUrl { get; set; }
        public DateTime? LineaccountCreated { get; set; }
        public DateTime? LineaccountUpdated { get; set; }
        public DateTime? LineaccountDeleted { get; set; }
    }
}
