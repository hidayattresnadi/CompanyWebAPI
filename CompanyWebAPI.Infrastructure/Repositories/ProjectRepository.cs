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
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        private readonly CompanySystemContext _db;
        public ProjectRepository(CompanySystemContext db) : base(db)
        {
            _db = db;
        }

        public Project Update(Project foundProject, DTOProject project)
        {
            foundProject.ProjName = project.ProjName;
            foundProject.DeptNo = project.DeptNo;
            return foundProject;
        }
        public async Task<IEnumerable<DTOProject>> GetITHRProjects(int pageNumber, int pageSize)
        {
            var projects = await _db.Projects.Include(p => p.DeptNoNavigation)
               .Where(p => p.DeptNoNavigation.DeptName == "IT" || p.DeptNoNavigation.DeptName == "HR")
               .Select(p => new DTOProject
               {
                   ProjNo = p.ProjNo,
                   DeptNo = p.DeptNo,
                   ProjName = p.ProjName
               }).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return projects;
        }
        public async Task<IEnumerable<Project>> GetProjectsWithNoEmployees(int pageNumber, int pageSize)
        {
            var projectsWithNoEmployees = await _db.Projects
                .Select(p => new
                {
                    Project = p,
                    HasEmployees = _db.WorksOns.Any(w => w.ProjNo == p.ProjNo)
                })
                .Where(p => !p.HasEmployees)
                .Select(p => p.Project)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync();
            return projectsWithNoEmployees;
        }
    }
}
