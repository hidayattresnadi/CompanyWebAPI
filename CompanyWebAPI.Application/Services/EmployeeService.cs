using CompanyAPI.DTO;
using CompanyAPI.Interfaces;
using CompanyWebAPI.Domain.Repositories;
using CompanyWebAPI.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CompanyAPI.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IConfiguration _configuration;
        public EmployeeService(IEmployeeRepository employeeRepository, IDepartmentRepository departmentRepository, IConfiguration configuration)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _configuration = configuration;
        }
        public async Task<bool> ValidateDupplicateEmployee(string firstName, string LastName)
        {
            bool isDupplicate = await _employeeRepository.AnyAsync(e => e.Fname == firstName && e.Lname == LastName);
            return isDupplicate;
        }
        public async Task<DTOEmployee> AddEmployee(DTOEmployee inputEmployee)
        {
            if (inputEmployee.DeptNo != null)
            {
                if (!await _departmentRepository.AnyAsync(d => d.DeptNo == inputEmployee.DeptNo))
                {
                    throw new ArgumentException("Invalid Department ID");
                }
                var section = _configuration.GetSection("CompanyConstraint");
                var value = section["MaximumITEmployees"];
                int intValue = int.Parse(value);
                var itEmployees = await _employeeRepository.GetAllAsync(e => e.DeptNo == 1);
                int itEmplyeesCount = itEmployees.Count();
                if (itEmplyeesCount == intValue)
                {
                    throw new ArgumentException("Maximum IT Employees are " + intValue);
                }
            }
            bool isDupplicate = await ValidateDupplicateEmployee(inputEmployee.Fname, inputEmployee.Lname);
            if (isDupplicate)
            {
                throw new ArgumentException("First Name and Last Name are already exist");
            }
            var newEmployee = new Employee
            {
                Fname = inputEmployee.Fname,
                Lname = inputEmployee.Lname,
                Address = inputEmployee.Address,
                Dob = inputEmployee.Dob,
                Sex = inputEmployee.Sex,
                Position = inputEmployee.Position,
                DeptNo = inputEmployee.DeptNo
            };
            await _employeeRepository.AddAsync(newEmployee);
            await _employeeRepository.SaveAsync();

            if (inputEmployee.DeptNo.HasValue && inputEmployee.Position == "Manager")
            {
                var department = await _departmentRepository.GetFirstOrDefaultAsync(d => d.DeptNo == inputEmployee.DeptNo);
                if (department != null)
                {
                    department.MgrEmpNo = newEmployee.EmpNo;
                    await _departmentRepository.SaveAsync();
                }
            }
            inputEmployee.EmpNo = newEmployee.EmpNo;
            return inputEmployee;
        }
        public async Task<IEnumerable<Employee>> GetAllEmployees(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var employees = await _employeeRepository.GetAllAsync(pageNumber, pageSize);
            return employees;
        }
        public async Task<Employee> GetEmployeeById(int id)
        {
            Employee chosenEmployee = await _employeeRepository.GetFirstOrDefaultAsync(foundEmployee => foundEmployee.EmpNo == id);
            return chosenEmployee;
        }
        public async Task<Employee> UpdateEmployee(DTOEmployee employee, int id)
        {
            if (!await _departmentRepository.AnyAsync(d => d.DeptNo == employee.DeptNo))
            {
                throw new ArgumentException("Invalid Department ID");
            }
            if (employee.Position == "Manager")
            {
                if (await _departmentRepository.AnyAsync(d => d.MgrEmpNo == id))
                {
                    throw new ArgumentException("Manager Id has been asigned, cannot update employee position");
                }
            }
            var foundEmployee = await GetEmployeeById(id);
            if (foundEmployee is null)
            {
                return null;
            }
            bool isDupplicate = await ValidateDupplicateEmployee(employee.Fname, employee.Lname);
            if (isDupplicate)
            {
                throw new ArgumentException("First Name and Last Name are already exist");
            }
            if (employee.DeptNo == 1)
            {
                var section = _configuration.GetSection("CompanyConstraint");
                var value = section["MaximumITEmployees"];
                int intValue = int.Parse(value);
                if (foundEmployee.DeptNo != 1)
                {
                    var itEmployees = await _employeeRepository.GetAllAsync(e => e.DeptNo == 1);
                    int itEmplyeesCount = itEmployees.Count();
                    if (itEmplyeesCount == intValue)
                    {
                        throw new ArgumentException("Maximum IT Employees are " + intValue);
                    }
                }
            }

            _employeeRepository.Update(foundEmployee, employee);
            await _employeeRepository.SaveAsync();
            return foundEmployee;
        }
        public async Task<bool> DeleteEmployee(int id)
        {
            var foundEmployee = await GetEmployeeById(id);
            if (foundEmployee is null)
            {
                return false;
            }
            _employeeRepository.Remove(foundEmployee);
            await _employeeRepository.SaveAsync();
            return true;
        }
        public async Task<IEnumerable<Employee>> GetEmployeesBorne1980(int pageNumber)
        {
            var startDate = new DateOnly(1980, 1, 1);
            var endDate = new DateOnly(1990, 1, 1);
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var employees = await _employeeRepository.GetAllAsync(x => x.Dob >= startDate && x.Dob <= endDate, pageNumber, pageSize);
            return employees;
        }

        public async Task<IEnumerable<Employee>> GetFemaleEmployeesBorne1980(int pageNumber)
        {
            var startDate = new DateOnly(1990, 1, 1);
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var employees = await _employeeRepository.GetAllAsync(x => x.Dob > startDate && x.Sex == "Female", pageNumber, pageSize);
            return employees;
        }
        public async Task<IEnumerable<Employee>> GetNonManagerEmployees(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var nonManagers = await _employeeRepository.GetNonManagersAsync(pageNumber, pageSize);
            return nonManagers;
        }
        public async Task<IEnumerable<Employee>> GetBRICSEmployees(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var employees = await _employeeRepository.GetBRICSEmployees(pageNumber, pageSize);
            return employees;
        }
        public async Task<IEnumerable<Employee>> GetITEmployees(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var itEmployees = await _employeeRepository.GetITEmployees(pageNumber, pageSize);
            return itEmployees;
        }
    }
}