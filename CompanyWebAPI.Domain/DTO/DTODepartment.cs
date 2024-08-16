using System.ComponentModel.DataAnnotations;

namespace CompanyAPI.DTO
{
    public class DTODepartment
    {
        [Required(ErrorMessage = "DeptName is required")]
        public string DeptName { get; set; }
        public int? MgrEmpNo { get; set; }
    }
}
