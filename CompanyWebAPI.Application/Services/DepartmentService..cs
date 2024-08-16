using CompanyWebAPI.Infrastructure.Models;
using CompanyAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using CompanyAPI.DTO;
using CompanyWebAPI.Domain.Repositories;

namespace CompanyAPI.Services
{
    public class DepartmentService : IDepartmentsService
    {
        private readonly CompanySystemContext _context;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IEmployeeRepository _employeeRepository;
        public DepartmentService(CompanySystemContext context, IDepartmentRepository departmentRepository, IEmployeeRepository employeeRepository)
        {
            _context = context;
            _departmentRepository = departmentRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<bool> ValidateDupplicateDepartmentName(string departmentName)
        {
            bool isDupplicate = await _departmentRepository.AnyAsync(d => d.DeptName == departmentName);
            return isDupplicate;
        }
        public async Task<Department> AddDepartment(DTODepartment inputDepartment)
        {
            if (inputDepartment.MgrEmpNo != null) {
                if (!await _employeeRepository.AnyAsync(e => e.EmpNo == inputDepartment.MgrEmpNo))
                {
                    throw new ArgumentException("Invalid Manager Employee ID");
                }
                if (await _departmentRepository.AnyAsync(d => d.MgrEmpNo == inputDepartment.MgrEmpNo))
                {
                    throw new ArgumentException("Invalid input Manager Employee ID, Manager has been asigned to another Department");
                }
            }
            bool isDupplicate = await ValidateDupplicateDepartmentName(inputDepartment.DeptName);
            if (isDupplicate)
            {
                throw new ArgumentException("Department Name is already exist");
            }
            var department = new Department
            {
                DeptName = inputDepartment.DeptName,
                MgrEmpNo = inputDepartment.MgrEmpNo
            };
            await _departmentRepository.AddAsync(department);
            await _departmentRepository.SaveAsync();
            return department;
        }
        public async Task<IEnumerable<Department>> GetAllDepartments(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var departments = await _departmentRepository.GetAllAsync(pageNumber,pageSize);
            return departments;
        }
        public async Task<Department> GetDepartmentById(int id)
        {
            Department chosenDepartment = await _departmentRepository.GetFirstOrDefaultAsync(foundDepartment => foundDepartment.DeptNo == id);
            return chosenDepartment;
        }
        public async Task<Department> UpdateDepartment(DTODepartment department, int id)
        {
            if (!await _employeeRepository.AnyAsync(e => e.EmpNo == department.MgrEmpNo))
            {
                throw new ArgumentException("Invalid Manager Employee ID");
            }
            var foundDepartment = await GetDepartmentById(id);
            if (foundDepartment is null)
            {
                return null;
            }
            bool isDupplicate = await ValidateDupplicateDepartmentName(department.DeptName);
            if (isDupplicate)
            {
                throw new ArgumentException("Department Name is already exist");
            }
            _departmentRepository.Update(foundDepartment, department);
            await _departmentRepository.SaveAsync();
            return foundDepartment;
        }
        public async Task<bool> DeleteDepartment(int id)
        {
            var foundDepartment = await GetDepartmentById(id);
            if (foundDepartment is null)
            {
                return false;
            }
            _departmentRepository.Remove(foundDepartment);
            await _departmentRepository.SaveAsync();
            return true;
        }
        public async Task<IEnumerable<Employee>> GetFemaleManagers(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber =1;
            var femaleManagers = await _departmentRepository.GetFemaleManagers(pageNumber, pageSize);
            return femaleManagers;
        }
        public async Task<int> GetCountingOfFemaleManagers()
        {
            var femaleManagersTotal = await _departmentRepository.GetCountingOfFemaleManagers();
            return femaleManagersTotal;
        }
        public async Task<IEnumerable<Employee>> GetManagersUnderFourty(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var managersUnder40 = await _departmentRepository.GetManagersUnderFourty(pageNumber, pageSize);
            return managersUnder40;
        }

        public async Task<IEnumerable<object>> GetDepartmentsWithMoreThanTenEmployees(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var departmentsWithMoreThanTenEmployees = await _departmentRepository.GetDepartmentsWithMoreThanTenEmployees(pageNumber, pageSize);
            return departmentsWithMoreThanTenEmployees;
        }
    }
}