using CompanyAPI.DTO;
using CompanyWebAPI.Domain.Repositories;
using CompanyWebAPI.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyWebAPI.Infrastructure.Repositories
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        private readonly CompanySystemContext _db;
        public EmployeeRepository(CompanySystemContext db) : base(db)
        {
            _db = db;
        }

        public Employee Update(Employee foundEmployee, DTOEmployee employee)
        {
            foundEmployee.DeptNo = employee.DeptNo;
            foundEmployee.Position = employee.Position;
            foundEmployee.Fname = employee.Fname;
            foundEmployee.Lname = employee.Lname;
            foundEmployee.Sex = employee.Sex;
            foundEmployee.Address = employee.Address;
            foundEmployee.Dob = employee.Dob;
            return foundEmployee;
        }
        public async Task<List<Employee>> GetNonManagersAsync(int pageNumber, int pageSize)
        {
            var nonManagers = await _db.Employees
                .GroupJoin(_db.Departments,
                           employee => employee.EmpNo,
                           department => department.MgrEmpNo,
                           (employee, departments) => new { Employee = employee, Departments = departments })
                .Where(ed => !ed.Departments.Any())
                .Select(ed => ed.Employee)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return nonManagers;
        }
        public async Task<IEnumerable<Employee>> GetITEmployees(int pageNumber, int pageSize)
        {
            var itEmployees = await _db.Employees.Join(_db.Departments, employee => employee.DeptNo,
                department => department.DeptNo, (employee, department) => new { Employee = employee, Department = department })
                .Where(ed => ed.Department.DeptName == "IT").Select(ed => ed.Employee)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return itEmployees;
        }
        public async Task<IEnumerable<Employee>> GetBRICSEmployees(int pageNumber, int pageSize)
        {
            var countries = new[] { "Brazil", "China", "Russia", "India", "South Africa" }.Select(c => c.ToLower()).ToArray();
            var employees = await _db.Employees.Where(x => countries.Any(country => x.Address.ToLower()
               .Contains(country))).OrderBy(x => x.Lname).Skip((pageNumber - 1) * pageSize)
               .Take(pageSize).ToListAsync();
            return employees;
        }
    }
}
