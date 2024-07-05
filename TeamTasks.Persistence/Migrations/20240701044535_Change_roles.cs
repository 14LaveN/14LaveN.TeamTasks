using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTasks.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Change_roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "dbo",
                table: "roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RoleUser1",
                schema: "dbo",
                columns: table => new
                {
                    Role1Value = table.Column<int>(type: "integer", nullable: false),
                    User1Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser1", x => new { x.Role1Value, x.User1Id });
                    table.ForeignKey(
                        name: "FK_RoleUser1_roles_Role1Value",
                        column: x => x.Role1Value,
                        principalSchema: "dbo",
                        principalTable: "roles",
                        principalColumn: "Value",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser1_users_User1Id",
                        column: x => x.User1Id,
                        principalSchema: "dbo",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "roles",
                keyColumn: "Value",
                keyValue: 1,
                column: "UserId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_roles_UserId",
                schema: "dbo",
                table: "roles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser1_User1Id",
                schema: "dbo",
                table: "RoleUser1",
                column: "User1Id");

            migrationBuilder.AddForeignKey(
                name: "FK_roles_users_UserId",
                schema: "dbo",
                table: "roles",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_roles_users_UserId",
                schema: "dbo",
                table: "roles");

            migrationBuilder.DropTable(
                name: "RoleUser1",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_roles_UserId",
                schema: "dbo",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "dbo",
                table: "roles");
        }
    }
}
