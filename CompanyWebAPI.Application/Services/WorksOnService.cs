using CompanyAPI.DTO;
using CompanyAPI.Interfaces;
using CompanyWebAPI.Domain.Repositories;
using CompanyWebAPI.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CompanySystemWebAPI.Services
{
    public class WorksOnService : IWorksOnService
    {
        private readonly IWorksOnRepository _worksOnRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IConfiguration _configuration;
        public WorksOnService(IWorksOnRepository worksOnRepository, IProjectRepository projectRepository, IEmployeeRepository employeeRepository,
                              IConfiguration configuration)
        {
            _worksOnRepository = worksOnRepository;
            _projectRepository = projectRepository;
            _employeeRepository = employeeRepository;
            _configuration = configuration;
        }
        public async Task<WorksOn> AddWorksOn(DTOWorksOn inputWorksOn)
        {
            if (!await _projectRepository.AnyAsync(pro => pro.ProjNo == inputWorksOn.ProjNo))
            {
                throw new ArgumentException("Invalid Project ID");
            }
            if (!await _employeeRepository.AnyAsync(e => e.EmpNo == inputWorksOn.EmpNo))
            {
                throw new ArgumentException("Invalid Employee ID");
            }
            var section = _configuration.GetSection("CompanyConstraint");
            var value = section["MaximumProjects"];
            int intValue = int.Parse(value);
            var validateTotalProjectEmployee = await _worksOnRepository.GetAllAsync(w => w.EmpNo == inputWorksOn.EmpNo);
            int totalProjectCount = validateTotalProjectEmployee.Count();
            if (totalProjectCount == intValue)
            {
                throw new ArgumentException("Maximum Projects For Employees are " + intValue);
            }

            var newWorksOn = new WorksOn
            {
                EmpNo = inputWorksOn.EmpNo,
                ProjNo = inputWorksOn.ProjNo,
                DateWorked = inputWorksOn.DateWorked,
                Hoursworked = inputWorksOn.Hoursworked
            };
            await _worksOnRepository.AddAsync(newWorksOn);
            await _worksOnRepository.SaveAsync();
            return newWorksOn;
        }
        public async Task<IEnumerable<WorksOn>> GetAllWorksOns(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var worksOns = await _worksOnRepository.GetAllAsync(pageNumber, pageSize);
            return worksOns;
        }
        public async Task<WorksOn> GetWorksOnById(int id)
        {
            WorksOn chosenWorksOn = await _worksOnRepository.GetFirstOrDefaultAsync(foundWorksOn => foundWorksOn.WorkNo == id);
            return chosenWorksOn;
        }
        public async Task<WorksOn> UpdateWorksOn(DTOWorksOn worksOn, int id)
        {
            if (!await _projectRepository.AnyAsync(pro => pro.ProjNo == worksOn.ProjNo))
            {
                throw new ArgumentException("Invalid Project ID");
            }
            if (!await _employeeRepository.AnyAsync(e => e.EmpNo == worksOn.EmpNo))
            {
                throw new ArgumentException("Invalid Employee ID");
            }
            var foundWorksOn = await GetWorksOnById(id);
            if (foundWorksOn is null)
            {
                return null;
            }
            _worksOnRepository.Update(foundWorksOn, worksOn);
            await _worksOnRepository.SaveAsync();
            return foundWorksOn;
        }
        public async Task<bool> DeleteWorksOn(int id)
        {
            var foundWorksOn = await GetWorksOnById(id);
            if (foundWorksOn is null)
            {
                return false;
            }
            _worksOnRepository.Remove(foundWorksOn);
            await _worksOnRepository.SaveAsync();
            return true;
        }

        public async Task<object> GetTotalHoursWorkedEmployee(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var totalHoursWorkedPerEmployee = await _worksOnRepository.GetTotalHoursWorkedEmployee(pageNumber, pageSize);
            return totalHoursWorkedPerEmployee;
        }
    }
}
