using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMove.Models
{
    public enum RequestStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }
    public class ServiceRequest
    {
        [Key]
        public int ServiceRequestId { get; set; }

        [Required]
        public int ContractId { get; set; }

        [ForeignKey("ContractId")]
        public virtual Contract? Contract { get; set; }

        [Required]
        [StringLength(200)]
        public string RequestTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountUSD { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountZAR { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [Required]
        [Column(TypeName = "decimal(10,4)")]
        public decimal ExchangeRateUsed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
//References
//Anon., 2010. Models In ASP.NET MVC 5. [Online] 
//Available at: https://www.c-sharpcorner.com/article/models-in-asp-net-mvc5/
//Anon., 2010. MVC :: What is a model?. [Online] 
//Available at: https://stackoverflow.com/questions/4221632/mvc-what-is-a-model
//Anon., 2022. Understanding Models, Views, and Controllers (C#). [Online] 
//Available at: https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions-1/overview/understanding-models-views-and-controllers-cs