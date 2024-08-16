using CompanyAPI.DTO;
using CompanyWebAPI.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyWebAPI.Domain.Repositories
{
    public interface IProjectRepository : IRepository<Project>
    {
        public Project Update(Project foundProject, DTOProject project);
        public Task<IEnumerable<DTOProject>> GetITHRProjects(int pageNumber, int pageSize);
        public Task<IEnumerable<Project>> GetProjectsWithNoEmployees(int pageNumber, int pageSize);
    }
}
