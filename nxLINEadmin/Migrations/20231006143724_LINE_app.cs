using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nxLINEadmin.Migrations
{
    public partial class LINE_app : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lineaccount",
                columns: table => new
                {
                    lineaccount_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    lineaccount_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    lineaccount_shortcode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    lineaccount_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    lineaccount_email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    istalk = table.Column<bool>(type: "bit", nullable: false),
                    talkmessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isprofile = table.Column<bool>(type: "bit", nullable: false),
                    profile_setting = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    entrypoint = table.Column<int>(type: "int", nullable: true),
                    startrank = table.Column<int>(type: "int", nullable: true),
                    pointexpire = table.Column<int>(type: "int", nullable: true),
                    memberscard_color = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    memberscard_designurl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    memberscard_isusecamera = table.Column<bool>(type: "bit", nullable: false),
                    memberscard_liffid = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    line_channelid = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    line_channelsecret = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    line_channelaccesstoken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    issmaregi = table.Column<bool>(type: "bit", nullable: false),
                    smaregi_contractid = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    lineaccount_logourl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    lineaccount_created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lineaccount_updated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lineaccount_deleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lineaccount", x => x.lineaccount_id);
                });

            migrationBuilder.CreateTable(
                name: "member",
                columns: table => new
                {
                    member_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    member_pos_id = table.Column<int>(type: "int", nullable: true),
                    member_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    member_shop_id = table.Column<int>(type: "int", nullable: false),
                    member_lastname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    member_firstname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    member_lastname_kana = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    member_firstname_kana = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    member_zipcode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    member_pref = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    member_address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    member_tel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    member_fax = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    member_mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    member_email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    member_gender = table.Column<byte>(type: "tinyint", nullable: true),
                    member_birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    member_hold_point = table.Column<int>(type: "int", nullable: true),
                    member_point_limit_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    member_last_pointget_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    member_last_pointget_point = table.Column<short>(type: "smallint", nullable: true),
                    member_last_visit_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    member_join_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    member_drop_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    member_allow_email = table.Column<byte>(type: "tinyint", nullable: true),
                    member_rank = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    member_note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    member_status = table.Column<byte>(type: "tinyint", nullable: false),
                    member_ordinal = table.Column<int>(type: "int", nullable: false),
                    member_visibility = table.Column<bool>(type: "bit", nullable: false),
                    member_tag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    member_nonce = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    member_lineid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    member_stripeId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    member_password_hash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    member_password_salt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    member_email_verify_token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    member_email_verify_expired_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    member_password_reset_token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    member_password_reset_verify_expired_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    member_is_password_reset_verified = table.Column<bool>(type: "bit", nullable: false),
                    member_pending_email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    member_pending_email_verify_token = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    member_is_signup_verified = table.Column<bool>(type: "bit", nullable: false),
                    member_signup_verify_token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    member_searchtext = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    member_createat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    member_updateat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    member_deleteat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_member", x => x.member_id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    status = table.Column<int>(type: "int", nullable: false),
                    user_loginid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    user_pwd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    user_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    user_email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    user_lineaccount_id = table.Column<int>(type: "int", nullable: true),
                    user_lineaccount_role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    user_lastlogindatetime = table.Column<DateTime>(type: "datetime", nullable: true),
                    user_created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    user_updated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.user_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lineaccount");

            migrationBuilder.DropTable(
                name: "member");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
