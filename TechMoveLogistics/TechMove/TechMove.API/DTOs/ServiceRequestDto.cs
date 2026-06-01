namespace TechMove.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for Service Request responses
    /// Used when returning service request data to clients
    /// </summary>
    public class ServiceRequestDto
    {
        /// <summary>
        /// Unique identifier for the service request
        /// </summary>
        public int ServiceRequestId { get; set; }

        /// <summary>
        /// ID of the associated contract
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Contract number for reference
        /// </summary>
        public string ContractNumber { get; set; } = string.Empty;

        /// <summary>
        /// Name of the client
        /// </summary>
        public string ClientName { get; set; } = string.Empty;

        /// <summary>
        /// Title of the service request
        /// </summary>
        public string RequestTitle { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the service request
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Amount in USD
        /// </summary>
        public decimal AmountUSD { get; set; }

        /// <summary>
        /// Amount in ZAR (converted)
        /// </summary>
        public decimal AmountZAR { get; set; }

        /// <summary>
        /// Current status of the request (Pending, InProgress, Completed, Cancelled)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Exchange rate used for currency conversion
        /// </summary>
        public decimal ExchangeRateUsed { get; set; }

        /// <summary>
        /// Date when the request was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the request was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Formatted amount in USD with currency symbol
        /// </summary>
        public string FormattedAmountUSD => $"${AmountUSD:N2}";

        /// <summary>
        /// Formatted amount in ZAR with currency symbol
        /// </summary>
        public string FormattedAmountZAR => $"R{AmountZAR:N2}";

        /// <summary>
        /// Status badge color for UI
        /// </summary>
        public string StatusBadgeColor => Status?.ToLower() switch
        {
            "pending" => "warning",
            "inprogress" => "info",
            "completed" => "success",
            "cancelled" => "danger",
            _ => "secondary"
        };

        /// <summary>
        /// Status display name with proper capitalization
        /// </summary>
        public string StatusDisplayName => Status?.ToLower() switch
        {
            "pending" => "Pending",
            "inprogress" => "In Progress",
            "completed" => "Completed",
            "cancelled" => "Cancelled",
            _ => Status ?? "Unknown"
        };

        /// <summary>
        /// Time elapsed since creation
        /// </summary>
        public string TimeElapsed
        {
            get
            {
                var elapsed = DateTime.UtcNow - CreatedAt;
                if (elapsed.TotalDays > 1)
                    return $"{(int)elapsed.TotalDays} days ago";
                if (elapsed.TotalHours > 1)
                    return $"{(int)elapsed.TotalHours} hours ago";
                if (elapsed.TotalMinutes > 1)
                    return $"{(int)elapsed.TotalMinutes} minutes ago";
                return "Just now";
            }
        }
    }
}