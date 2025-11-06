//using ProjectManagementSystem.Data;
using ProjectManagementSystem.DBModels;

namespace ProjectManagementSystem.DBModels
{
    public static class DbInitializer
    {
        public static void SeedAcademicYears(PMSDbContext context)
        {
            var currentYear = DateTime.Now.Year;

            for (int year = 2000; year < currentYear; year++)
            {
                string range = $"{year}-{year + 1}";

                if (!context.AcademicYears.Any(a => a.YearRange == range))
                {
                    context.AcademicYears.Add(new DBModels.AcademicYear
                    {
                        YearRange = range
                    });
                }
            }

            context.SaveChanges();
        }
    }
}
