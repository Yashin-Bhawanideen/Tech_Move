using System.ComponentModel.DataAnnotations;

namespace TechMove.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for updating just the contract status
    /// Used for PATCH operations to change contract status
    /// </summary>
    public class UpdateContractStatusDto
    {
        /// <summary>
        /// New status for the contract
        /// Valid values: Draft, Active, Expired, OnHold
        /// </summary>
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Draft|Active|Expired|OnHold)$",
            ErrorMessage = "Status must be one of: Draft, Active, Expired, OnHold")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Optional notes about the status change
        /// </summary>
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}