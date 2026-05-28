using System.ComponentModel.DataAnnotations;

namespace TechMove.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a new contract
    /// Used when clients send data to create a contract
    /// </summary>
    public class CreateContractDto
    {
        /// <summary>
        /// ID of the client this contract belongs to
        /// </summary>
        [Required(ErrorMessage = "Client ID is required")]
        public int ClientId { get; set; }

        /// <summary>
        /// Unique contract number
        /// </summary>
        [Required(ErrorMessage = "Contract number is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Contract number must be between 3 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\-_]+$", ErrorMessage = "Contract number can only contain letters, numbers, hyphens, and underscores")]
        public string ContractNumber { get; set; } = string.Empty;

        /// <summary>
        /// Name of the service lead responsible for this contract
        /// </summary>
        [Required(ErrorMessage = "Service lead name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Service lead name must be between 2 and 100 characters")]
        public string ServiceLead { get; set; } = string.Empty;

        /// <summary>
        /// Contract start date
        /// </summary>
        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Contract end date
        /// </summary>
        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Contract status (Draft, Active, Expired, OnHold)
        /// </summary>
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Draft|Active|Expired|OnHold)$", ErrorMessage = "Status must be Draft, Active, Expired, or OnHold")]
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Terms and conditions of the contract
        /// </summary>
        [StringLength(2000, ErrorMessage = "Terms and conditions cannot exceed 2000 characters")]
        public string? TermsAndConditions { get; set; }

        /// <summary>
        /// Validates that start date is before end date
        /// </summary>
        public bool IsValidDateRange()
        {
            return StartDate <= EndDate;
        }

        /// <summary>
        /// Validates that dates are not in the distant past or future
        /// </summary>
        public bool IsReasonableDateRange()
        {
            var minDate = new DateTime(2000, 1, 1);
            var maxDate = new DateTime(2100, 12, 31);
            return StartDate >= minDate && EndDate <= maxDate;
        }
    }
}