using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nxLINEadminAPI.Migrations
{
    /// <inheritdoc />
    public partial class MemberRegSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberRegSettings",
                columns: table => new
                {
                    member_reg_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    member_reg_is_name = table.Column<bool>(type: "bit", nullable: false),
                    member_reg_is_furigana = table.Column<bool>(type: "bit", nullable: false),
                    member_reg_is_tel = table.Column<bool>(type: "bit", nullable: false),
                    member_reg_is_email = table.Column<bool>(type: "bit", nullable: false),
                    member_reg_is_birthday = table.Column<bool>(type: "bit", nullable: false),
                    member_reg_is_gender = table.Column<bool>(type: "bit", nullable: false),
                    member_reg_is_address = table.Column<bool>(type: "bit", nullable: false),
                    overview = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberRegSettings", x => x.member_reg_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberRegSettings");
        }
    }
}
