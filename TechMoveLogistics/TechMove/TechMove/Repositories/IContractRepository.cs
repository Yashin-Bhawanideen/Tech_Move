using System.Linq.Expressions;
using TechMove.Models;

namespace TechMove.Repositories
{
    public interface IContractRepository : IRepository<Contract>
    {
        Task<IEnumerable<Contract>> GetContractsWithDetailsAsync();
        Task<Contract?> GetContractWithDetailsAsync(int id);
        Task<IEnumerable<Contract>> GetActiveContractsAsync();
        Task<IEnumerable<Contract>> SearchContractsAsync(DateTime? startDate, DateTime? endDate, ContractStatus? status);
        Task<bool> IsContractActiveForServiceAsync(int contractId);

        Task<IEnumerable<Client>> GetAvailableClientsAsync();
    }
}
//References
//Bala, 2014. Understanding Interfaces in C#. [Online] 
//Available at: https://www.c-sharpcorner.com/UploadFile/sekarbalag/Interface-In-CSharp/
//MNsr, 2012. C# How to use interfaces. [Online] 
//Available at: https://stackoverflow.com/questions/7762291/c-sharp-how-to-use-interfaces
//W3schools, 2025. C# Interface. [Online] 
//Available at: https://www.w3schools.com/cs/cs_interface.php
//Anon., 2009. Allow multiple roles to access controller action. [Online] 
//Available at: https://stackoverflow.com/questions/700166/allow-multiple-roles-to-access-controller-action
//Anon., 2024. Role-based authorization in ASP.NET Core. [Online] 
//Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-9.0
//Anon., 2025. Role Based Access Of An MVC Application. [Online] 
//Available at: https://www.c-sharpcorner.com/UploadFile/rahul4_saxena/role-based-access-of-an-mvc-application/