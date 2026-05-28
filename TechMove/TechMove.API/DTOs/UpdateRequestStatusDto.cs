using System.ComponentModel.DataAnnotations;

namespace TechMove.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for updating just the service request status
    /// Used for PATCH operations to change request status only
    /// </summary>
    public class UpdateRequestStatusDto
    {
        /// <summary>
        /// New status for the service request
        /// Valid values: Pending, InProgress, Completed, Cancelled
        /// </summary>
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Pending|InProgress|Completed|Cancelled)$",
            ErrorMessage = "Status must be one of: Pending, InProgress, Completed, Cancelled")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Optional notes about the status change
        /// </summary>
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        /// <summary>
        /// Reason for status change (for audit trail)
        /// </summary>
        [StringLength(200, ErrorMessage = "Reason cannot exceed 200 characters")]
        public string? ChangeReason { get; set; }

        /// <summary>
        /// Completion notes (when status is Completed)
        /// </summary>
        [StringLength(1000, ErrorMessage = "Completion notes cannot exceed 1000 characters")]
        public string? CompletionNotes { get; set; }

        /// <summary>
        /// Validates that status transition is allowed
        /// </summary>
        /// <param name="currentStatus">Current status of the request</param>
        /// <returns>True if transition is allowed, false otherwise</returns>
        public bool IsValidTransition(string currentStatus)
        {
            var current = currentStatus.ToLower();
            var newStatus = Status.ToLower();

            // Define allowed transitions
            var allowedTransitions = new Dictionary<string, string[]>
            {
                ["pending"] = new[] { "inprogress", "cancelled" },
                ["inprogress"] = new[] { "completed", "cancelled", "pending" },
                ["completed"] = new[] { "pending" },
                ["cancelled"] = new[] { "pending" }
            };

            if (allowedTransitions.ContainsKey(current) && allowedTransitions[current].Contains(newStatus))
                return true;

            return current == newStatus; // Same status is always allowed
        }

        /// <summary>
        /// Gets the validation error message for invalid transition
        /// </summary>
        /// <param name="currentStatus">Current status of the request</param>
        /// <returns>Error message explaining allowed transitions</returns>
        public string GetTransitionErrorMessage(string currentStatus)
        {
            var current = currentStatus.ToLower();

            var allowedTransitions = new Dictionary<string, string[]>
            {
                ["pending"] = new[] { "InProgress", "Cancelled" },
                ["inprogress"] = new[] { "Completed", "Cancelled", "Pending" },
                ["completed"] = new[] { "Pending" },
                ["cancelled"] = new[] { "Pending" }
            };

            if (allowedTransitions.ContainsKey(current))
            {
                return $"Cannot change status from {currentStatus} to {Status}. Allowed transitions: {string.Join(", ", allowedTransitions[current])}";
            }

            return $"Invalid status transition from {currentStatus} to {Status}";
        }

        /// <summary>
        /// Checks if completion notes are required
        /// </summary>
        public bool RequiresCompletionNotes()
        {
            return Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if change reason is required
        /// </summary>
        public bool RequiresChangeReason()
        {
            return Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
                   Status.Equals("Pending", StringComparison.OrdinalIgnoreCase);
        }
    }
}