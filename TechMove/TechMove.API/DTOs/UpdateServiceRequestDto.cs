using System.ComponentModel.DataAnnotations;

namespace TechMove.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for updating an existing service request
    /// Used for PUT operations to replace request data
    /// </summary>
    public class UpdateServiceRequestDto
    {
        /// <summary>
        /// Title of the service request
        /// </summary>
        [Required(ErrorMessage = "Request title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string RequestTitle { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the service request
        /// </summary>
        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Amount in USD
        /// </summary>
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999.99")]
        public decimal AmountUSD { get; set; }
    }
}