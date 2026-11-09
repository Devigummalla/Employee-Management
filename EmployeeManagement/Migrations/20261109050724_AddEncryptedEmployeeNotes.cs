using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptedEmployeeNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ConfidentialNotesCiphertext",
                table: "Employees",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfidentialNotesKeyVersion",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ConfidentialNotesNonce",
                table: "Employees",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ConfidentialNotesTag",
                table: "Employees",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfidentialNotesCiphertext",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ConfidentialNotesKeyVersion",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ConfidentialNotesNonce",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ConfidentialNotesTag",
                table: "Employees");
        }
    }
}
