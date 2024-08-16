using CompanyAPI.DTO;
using CompanyWebAPI.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyWebAPI.Domain.Repositories
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        public Employee Update(Employee foundEmployee, DTOEmployee employee);
        public Task<List<Employee>> GetNonManagersAsync(int pageNumber, int pageSize);
        public Task<IEnumerable<Employee>> GetITEmployees(int pageNumber, int pageSize);
        public Task<IEnumerable<Employee>> GetBRICSEmployees(int pageNumber, int pageSize);
    }
}
