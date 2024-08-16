using CompanyAPI.DTO;
using CompanyWebAPI.Infrastructure.Models;

namespace CompanyAPI.Interfaces
{
    public interface IDepartmentsService
    {
        Task<Department> AddDepartment(DTODepartment department);
        Task<IEnumerable<Department>> GetAllDepartments(int pageNumber);
        Task<Department> GetDepartmentById(int id);
        Task<Department> UpdateDepartment(DTODepartment department, int id);
        Task<bool> DeleteDepartment(int id);
        Task<IEnumerable<Employee>> GetFemaleManagers(int pageNumber);
        Task<int> GetCountingOfFemaleManagers();
        Task<IEnumerable<object>> GetDepartmentsWithMoreThanTenEmployees(int pageNumber);
        Task<IEnumerable<Employee>> GetManagersUnderFourty(int pageNumber);
        Task<IEnumerable<object>> GetManagersDueToRetireThisYear();
        Task<IEnumerable<object>> GetFemaleManagersWithProjectsAsync();
        Task<IEnumerable<Project>> GetProjectsManagedByPlanningDepartmentAsync();
    }
}
