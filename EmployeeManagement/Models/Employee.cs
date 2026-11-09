
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string EntraObjectId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Department { get; set; }

        [MaxLength(100)]
        public string? Designation { get; set; }

        public DateTime? JoiningDate { get; set; }

        public bool IsActive { get; set; } = true;

        // AES-GCM encrypted confidential employee notes

        public byte[]? ConfidentialNotesCiphertext { get; set; }

        public byte[]? ConfidentialNotesNonce { get; set; }

        public byte[]? ConfidentialNotesTag { get; set; }

        public int? ConfidentialNotesKeyVersion { get; set; }

    }
}
