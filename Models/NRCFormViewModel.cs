namespace ProjectManagementSystem.Models
{
    public class NRCFormViewModel
    {
        public Student Student { get; set; }
        public List<AcademicYear> AcademicYearList { get; set; }

        public List<NRCType> NRCTypeList { get; set; }

        public List<string> RegionCodeMList { get; set; } // ၁, ၂, ၃...

        public List<NRCTownship> TownshipList { get; set; }
        public List<StudentDepartment> DepartmentList { get; set; }
        public List<ProjectMember> ProjectMembers { get; set; } = new();

        public IFormFile? ProfilePhoto { get; set; }

    }

}
