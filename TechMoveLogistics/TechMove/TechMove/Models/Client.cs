using System.ComponentModel.DataAnnotations;

namespace TechMove.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Region { get; set; } = string.Empty;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        //naviagtion property, one to many, user can have many contracts user their own account
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();


    }
}
//References
//Anon., 2010. Models In ASP.NET MVC 5. [Online] 
//Available at: https://www.c-sharpcorner.com/article/models-in-asp-net-mvc5/
//Anon., 2010. MVC :: What is a model?. [Online] 
//Available at: https://stackoverflow.com/questions/4221632/mvc-what-is-a-model
//Anon., 2022. Understanding Models, Views, and Controllers (C#). [Online] 
//Available at: https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions-1/overview/understanding-models-views-and-controllers-cs