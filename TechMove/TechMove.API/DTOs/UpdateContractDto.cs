using System.ComponentModel.DataAnnotations;

namespace TechMove.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for updating an entire contract
    /// Used for PUT operations to replace contract data
    /// </summary>
    public class UpdateContractDto
    {
        /// <summary>
        /// ID of the client this contract belongs to
        /// </summary>
        [Required(ErrorMessage = "Client ID is required")]
        public int ClientId { get; set; }

        /// <summary>
        /// Unique contract number for reference
        /// </summary>
        [Required(ErrorMessage = "Contract number is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Contract number must be between 3 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\-_]+$", ErrorMessage = "Contract number can only contain letters, numbers, hyphens, and underscores")]
        public string ContractNumber { get; set; } = string.Empty;

        /// <summary>
        /// Name of the service lead/manager
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
        /// Current status of the contract (Draft, Active, Expired, OnHold)
        /// </summary>
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Draft|Active|Expired|OnHold)$",
            ErrorMessage = "Status must be one of: Draft, Active, Expired, OnHold")]
        public string Status { get; set; } = string.Empty;

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

        /// <summary>
        /// Validates that end date is not more than 10 years from start date
        /// </summary>
        public bool IsValidDuration()
        {
            return (EndDate - StartDate).TotalDays <= 3650; // 10 years max
        }
    }
}