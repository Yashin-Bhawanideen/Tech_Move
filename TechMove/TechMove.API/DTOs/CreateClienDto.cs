using System.ComponentModel.DataAnnotations;

namespace TechMove.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a new client
    /// Used when clients send data to create a client
    /// </summary>
    public class CreateClientDto
    {
        /// <summary>
        /// Name of the client company
        /// </summary>
        [Required(ErrorMessage = "Client name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Client name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the client
        /// </summary>
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Phone number of the client
        /// </summary>
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Physical address of the client
        /// </summary>
        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Geographic region of the client
        /// </summary>
        [Required(ErrorMessage = "Region is required")]
        [RegularExpression("^(Africa|North America|South America|Europe|Asia|Australia|Antarctica)$",
            ErrorMessage = "Region must be a valid continent")]
        public string Region { get; set; } = string.Empty;
    }
}