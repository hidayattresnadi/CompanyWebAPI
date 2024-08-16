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
    public class WorksOnRepository : Repository<WorksOn>, IWorksOnRepository
    {
        private readonly CompanySystemContext _db;
        public WorksOnRepository(CompanySystemContext db) : base(db)
        {
            _db = db;
        }

        public WorksOn Update(WorksOn foundWorksOn, DTOWorksOn worksOn)
        {
            foundWorksOn.ProjNo = worksOn.ProjNo;
            foundWorksOn.EmpNo = worksOn.EmpNo;
            foundWorksOn.DateWorked = worksOn.DateWorked;
            foundWorksOn.Hoursworked = worksOn.Hoursworked;
            return foundWorksOn;
        }
        public async Task<object> GetTotalHoursWorkedEmployee(int pageNumber, int pageSize)
        {
            var totalHoursWorkedPerEmployee = await _db.WorksOns
      .GroupBy(w => w.EmpNo)
            .Select(g => new
            {
          Employee = _db.Employees
              .Where(e => e.EmpNo == g.Key)
              .Select(e => new
              {
                  Name = e.Fname + " " + e.Lname,
              })
              .FirstOrDefault(),
          TotalHoursWorked = g.Sum(w => w.Hoursworked),
                Projects = _db.Projects
              .Where(p => _db.WorksOns.Any(w => w.EmpNo == g.Key && w.ProjNo == p.ProjNo))
              .Select(p => p.ProjName)
              .ToList()
      })
      .Skip((pageNumber - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync();
            return totalHoursWorkedPerEmployee;
        }
        public async Task<decimal> GetTotalHoursProject(DTOWorksOn newWorkOn)
        {
            var totalHours = await _db.WorksOns
            .Where(w => w.ProjNo == newWorkOn.ProjNo)
            .SumAsync(w => w.Hoursworked);
            return totalHours;
        }
        public async Task<List<object>> GetTotalHoursWorkedByFemaleEmployeesReport()
        {
            var report = await _db.Employees
                .Where(e => e.Sex == "Female" && e.WorksOns.Any())
                .GroupBy(e => new { e.DeptNo, e.Lname })
                .Select(group => new
                {
                    DepartmentNumber = group.Key.DeptNo,
                    LastName = group.Key.Lname,
                    TotalHoursWorked = group.Sum(e => e.WorksOns.Sum(w => w.Hoursworked))
                })
                .OrderBy(r => r.DepartmentNumber)
                .ThenBy(r => r.LastName)
                .ToListAsync();
            return report.Cast<object>().ToList();
        }
        public async Task<IEnumerable<object>> GetMaxAndMinHoursWorkedByEmployeeAsync()
        {
            var result = await _db.WorksOns
                .GroupBy(w => w.EmpNo)
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    MaxHoursWorked = g.Max(w => w.Hoursworked),
                    MinHoursWorked = g.Min(w => w.Hoursworked)
                })
                .ToListAsync();

            return result;
        }
    }
}
