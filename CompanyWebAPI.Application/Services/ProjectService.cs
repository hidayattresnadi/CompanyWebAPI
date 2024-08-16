using CompanyAPI.DTO;
using CompanyAPI.Interfaces;
using CompanyWebAPI.Domain.Repositories;
using CompanyWebAPI.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CompanySystemWebAPI.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IConfiguration _configuration;
        public ProjectService(CompanySystemContext context, IProjectRepository projectRepository, IDepartmentRepository departmentRepository,
        IConfiguration configuration)
        {
            _projectRepository = projectRepository;
            _departmentRepository = departmentRepository;
            _configuration = configuration;
        }
        public async Task<bool> ValidateDupplicateProjectName(string projectName)
        {
            bool isDupplicate = await _projectRepository.AnyAsync(p => p.ProjName == projectName);
            return isDupplicate;
        }
        public async Task<Project> AddProject(DTOProject inputProject)
        {
            if (!await _departmentRepository.AnyAsync(d => d.DeptNo == inputProject.DeptNo))
            {
                throw new ArgumentException("Invalid Department ID");
            }
            bool isDupplicate = await ValidateDupplicateProjectName(inputProject.ProjName);
            if (isDupplicate)
            {
                throw new ArgumentException("Project Name is Already Exist");
            }
            var newProject = new Project
            {
                DeptNo = inputProject.DeptNo,
                ProjName = inputProject.ProjName
            };
            var section = _configuration.GetSection("CompanyConstraint");
            var value = section["MaximumDepartmentProjects"];
            int intValue = int.Parse(value);
            var validateTotalDepartmentProjects = await _projectRepository.GetAllAsync(p => p.DeptNo == inputProject.DeptNo);
            int totalProjectCount = validateTotalDepartmentProjects.Count();
            if (totalProjectCount == intValue)
            {
                throw new ArgumentException("Maximum Projects For Departments are " + intValue);
            }
            await _projectRepository.AddAsync(newProject);
            await _projectRepository.SaveAsync();
            return newProject;
        }
        public async Task<IEnumerable<Project>> GetAllProjects(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var projects = await _projectRepository.GetAllAsync(pageNumber, pageSize);
            return projects;
        }
        public async Task<Project> GetProjectById(int id)
        {
            Project chosenProject = await _projectRepository.GetFirstOrDefaultAsync(foundProject => foundProject.ProjNo == id);
            return chosenProject;
        }
        public async Task<Project> UpdateProject(DTOProject project, int id)
        {
            if (!await _departmentRepository.AnyAsync(d => d.DeptNo == project.DeptNo))
            {
                throw new ArgumentException("Invalid Department ID");
            }
            var foundProject = await GetProjectById(id);
            if (foundProject is null)
            {
                return null;
            }
            bool isDupplicate = await ValidateDupplicateProjectName(project.ProjName);
            if (isDupplicate)
            {
                throw new ArgumentException("Project Name is Already Exist");
            }
            var section = _configuration.GetSection("CompanyConstraint");
            var value = section["MaximumDepartmentProjects"];
            int maxProjects = int.Parse(value);
            if (foundProject != null)
            {
                int oldDeptNo = foundProject.DeptNo;
                if (project.DeptNo != oldDeptNo)
                {
                    int totalProjectsInNewDept = (await _projectRepository.GetAllAsync(p => p.DeptNo == project.DeptNo)).Count();
                    if (totalProjectsInNewDept >= maxProjects)
                    {
                        throw new ArgumentException("Maximum Projects For Department " + project.DeptNo + " are " + maxProjects);
                    }
                }
            }
            else
            {
                int totalProjectsInDept = (await _projectRepository.GetAllAsync(p => p.DeptNo == project.DeptNo)).Count();
                if (totalProjectsInDept >= maxProjects)
                {
                    throw new ArgumentException("Maximum Projects For Department " + project.DeptNo + " are " + maxProjects);
                }
            }
            _projectRepository.Update(foundProject, project);
            await _projectRepository.SaveAsync();
            return foundProject;
        }
        public async Task<bool> DeleteProject(int id)
        {
            var foundProject = await GetProjectById(id);
            if (foundProject is null)
            {
                return false;
            }
            _projectRepository.Remove(foundProject);
            await _projectRepository.SaveAsync();
            return true;
        }
        public async Task<IEnumerable<DTOProject>> GetITHRProjects(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var projects = await _projectRepository.GetITHRProjects(pageNumber, pageSize);
            return projects;
        }
        public async Task<IEnumerable<Project>> GetProjectsWithNoEmployees(int pageNumber)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;
            var projectsWithNoEmployees = await _projectRepository.GetProjectsWithNoEmployees(pageNumber, pageSize);
            return projectsWithNoEmployees;
        }
    }
}