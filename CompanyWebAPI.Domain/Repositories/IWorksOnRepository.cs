using CompanyAPI.DTO;
using CompanyWebAPI.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyWebAPI.Domain.Repositories
{
    public interface IWorksOnRepository : IRepository<WorksOn>
    {
        public WorksOn Update(WorksOn foundWorksOn, DTOWorksOn worksOn);
        public Task<object> GetTotalHoursWorkedEmployee(int pageNumber, int pageSize);
    }
}
