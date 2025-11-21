using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.DBModels
{
    public class NRCFormViewModel
    {
        public Student Student { get; set; }
        public List<AcademicYear> AcademicYearList { get; set; }

        public List<Nrctype> NRCTypeList { get; set; }

        public List<string> RegionCodeMList { get; set; } // ၁, ၂, ၃...

        public List<Nrctownship> TownshipList { get; set; }
        public List<StudentDepartment> DepartmentList { get; set; }
        public List<ProjectMember> ProjectMembers { get; set; } = new();

        public IFormFile? ProfilePhoto { get; set; }

    }

}
