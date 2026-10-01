using System.ComponentModel.DataAnnotations;

namespace PKValves.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(30)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Please select a subject.")]
        [RegularExpression(
            "^(Product Enquiry|Valves|Pipes|Hoses|Pumps|Tanks|Plumbing|General Enquiry)$",
            ErrorMessage = "Please select a subject from the list.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your message.")]
        [StringLength(
            5000,
            ErrorMessage = "Your message cannot exceed 5000 characters.")]
        public string Message { get; set; } = string.Empty;
    }
}