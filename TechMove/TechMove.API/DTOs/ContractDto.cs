namespace TechMove.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for Contract responses
    /// Used when returning contract data to clients
    /// </summary>
    public class ContractDto
    {
        /// <summary>
        /// Unique identifier for the contract
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// ID of the associated client
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Name of the associated client
        /// </summary>
        public string ClientName { get; set; } = string.Empty;

        /// <summary>
        /// Unique contract number for reference
        /// </summary>
        public string ContractNumber { get; set; } = string.Empty;

        /// <summary>
        /// Name of the service lead/manager
        /// </summary>
        public string ServiceLead { get; set; } = string.Empty;

        /// <summary>
        /// Contract start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Contract end date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Current status of the contract (Draft, Active, Expired, OnHold)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Path to the signed agreement PDF file
        /// </summary>
        public string? SignedAgreementPath { get; set; }

        /// <summary>
        /// Terms and conditions of the contract
        /// </summary>
        public string? TermsAndConditions { get; set; }

        /// <summary>
        /// Date when the contract was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the contract was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Number of service requests associated with this contract
        /// </summary>
        public int ServiceRequestsCount { get; set; }

        /// <summary>
        /// Indicates if the contract is currently active (calculated property)
        /// </summary>
        public bool IsActive => Status == "Active" && StartDate <= DateTime.UtcNow && EndDate >= DateTime.UtcNow;
    }
}