using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMove.API.Models
{
    public enum ContractStatus
    {
        Draft,
        Active,
        Expired,
        OnHold
    }

    public class Contract
    {
        [Key]
        public int ContractId { get; set; }

        [Required]
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }

        [Required]
        [StringLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ServiceLead { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [StringLength(1000)]
        public string? TermsAndConditions { get; set; }

        [StringLength(500)]
        public string? SignedAgreementPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        // Computed property to check if contract is active
        [NotMapped]
        public bool IsActive => Status == ContractStatus.Active &&
                                 StartDate <= DateTime.UtcNow &&
                                 EndDate >= DateTime.UtcNow;
    }
}