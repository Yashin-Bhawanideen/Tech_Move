using System.Linq.Expressions;

namespace TechMove.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
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