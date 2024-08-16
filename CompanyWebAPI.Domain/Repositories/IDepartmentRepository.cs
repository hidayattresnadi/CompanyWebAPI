using CompanyAPI.DTO;
using CompanyWebAPI.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyWebAPI.Domain.Repositories
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        public Department Update(Department foundDepartment, DTODepartment department);
        public Task<IEnumerable<Employee>> GetFemaleManagers(int pageNumber, int pageSize);
        public Task<int> GetCountingOfFemaleManagers();
        public Task<IEnumerable<object>> GetDepartmentsWithMoreThanTenEmployees(int pageNumber, int pageSize);
        public Task<IEnumerable<Employee>> GetManagersUnderFourty(int pageNumber, int pageSize);
        public Task<IEnumerable<object>> GetManagersDueToRetireThisYear(int intValue);
        public Task<IEnumerable<object>> GetFemaleManagersWithProjectsAsync();
        public Task<IEnumerable<Project>> GetProjectsManagedByPlanningDepartmentAsync();
    }
}
