using System.ComponentModel.DataAnnotations;

using TechMove.Models;

namespace TechMove.ViewModel
{
    public class ContractViewModel
    {
        public int ContractId { get; set;  }

        [Required]
        public int ClientId { get; set; }

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
        public ContractStatus Status { get; set; }

        [StringLength(1000)]
        public string? TermsAndConditions {  get; set; }
        public IFormFile? SignedAgreement { get; set; }
        public string? ExistingFilePath {  get; set; }
    }
}
//References
//Anon., 2010. Models In ASP.NET MVC 5. [Online] 
//Available at: https://www.c-sharpcorner.com/article/models-in-asp-net-mvc5/
//Anon., 2010. MVC :: What is a model?. [Online] 
//Available at: https://stackoverflow.com/questions/4221632/mvc-what-is-a-model
//Anon., 2022. Understanding Models, Views, and Controllers (C#). [Online] 
//Available at: https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions-1/overview/understanding-models-views-and-controllers-cs