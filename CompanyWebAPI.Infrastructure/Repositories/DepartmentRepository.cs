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
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        private readonly CompanySystemContext _db;
        public DepartmentRepository(CompanySystemContext db) : base(db)
        {
            _db = db;
        }

        public Department Update(Department foundDepartment, DTODepartment department)
        {
            foundDepartment.DeptName = department.DeptName;
            foundDepartment.MgrEmpNo = department.MgrEmpNo;
            return foundDepartment;
        }
        public async Task<IEnumerable<Employee>> GetFemaleManagers(int pageNumber, int pageSize)
        {
            var femaleManagers = await _db.Departments
               .Where(d => d.MgrEmpNoNavigation.Sex == "Female")
               .Select(d => d.MgrEmpNoNavigation)
               .OrderBy(e => e.Lname)
               .ThenBy(e => e.Fname)
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToListAsync();
            return femaleManagers;
        }
        public async Task<int> GetCountingOfFemaleManagers()
        {
            var femaleManagers = await _db.Departments
                .Where(d => d.MgrEmpNoNavigation.Sex == "Female")
                .Select(d => d.MgrEmpNoNavigation)
                .OrderBy(e => e.Lname)
                .ThenBy(e => e.Fname)
                .ToListAsync();
            return femaleManagers.Count();
        }
        public async Task<IEnumerable<object>> GetDepartmentsWithMoreThanTenEmployees(int pageNumber, int pageSize)
        {
            var departmentsWithMoreThanTenEmployees = await _db.Departments
                .Select(d => new
                {
                    Department = d,
                    EmployeeCount = d.Employees.Count()
                })
                .Where(d => d.EmployeeCount > 10)
                .Select(d => new
                {
                    d.Department.DeptNo,
                    d.Department.DeptName,
                    d.EmployeeCount
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return departmentsWithMoreThanTenEmployees;
        }
        public async Task<IEnumerable<Employee>> GetManagersUnderFourty(int pageNumber, int pageSize)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var managersUnder40 = await _db.Employees
                .Where(e => e.Departments.Any() && // Check if the employee is a manager
                            (today.Year - e.Dob.Year - ((today < e.Dob.AddYears(today.Year - e.Dob.Year)) ? 1 : 0)) < 40).Skip((pageNumber - 1) * pageSize).Take(pageSize)  
                .ToListAsync();
            return managersUnder40;
        }
        public async Task<IEnumerable<object>> GetManagersDueToRetireThisYear(int intValue)
        {
            var currentYear = DateOnly.FromDateTime(DateTime.Now).Year;
            var retirementAge = intValue;

            var managersDueToRetire = await _db.Departments
                .Select(d => new
                {
                    Manager = d.MgrEmpNoNavigation,
                    RetirementYear = d.MgrEmpNoNavigation.Dob.Year + retirementAge
                })
                .Where(m => m.RetirementYear == currentYear)
                .OrderBy(m => m.Manager.Lname)
                .Select(m => new
                {
                    EmpNo = m.Manager.EmpNo,
                    Fname = m.Manager.Fname,
                    Lname = m.Manager.Lname,
                    Address = m.Manager.Address,
                    Dob = m.Manager.Dob,
                    Sex = m.Manager.Sex,
                    Position = m.Manager.Position,
                    DeptNo = m.Manager.DeptNo,
                    RetirementAge = retirementAge
                })
                .ToListAsync();
            return managersDueToRetire;
        }
        public async Task<IEnumerable<object>> GetFemaleManagersWithProjectsAsync()
        {
            var result = await _db.Departments
                .Include(d => d.MgrEmpNoNavigation)
                .Where(d => d.MgrEmpNoNavigation.Sex == "Female" && d.MgrEmpNoNavigation.Position == "Manager")
                .Select(d => new
                {
                    ManagerName = d.MgrEmpNoNavigation.Fname + " " + d.MgrEmpNoNavigation.Lname,
                    Projects = d.Projects.Select(p => new
                    {
                        p.ProjNo,
                        p.ProjName
                    })
                })
                .ToListAsync();

            return result;
        }
        public async Task<IEnumerable<Project>> GetProjectsManagedByPlanningDepartmentAsync()
        {
            var planningDepartment = await _db.Departments
                .FirstOrDefaultAsync(d => d.DeptName == "Planning");

            if (planningDepartment == null)
            {
                throw new ArgumentException("The Planning department does not exist.");
            }

            var projects = await _db.Projects
                .Where(p => p.DeptNo == planningDepartment.DeptNo)
                .ToListAsync();

            return projects;
        }
    }
}
