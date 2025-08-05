using ProjectManagementSystem.Data;

namespace ProjectManagementSystem.Models
{
    public static class DbInitializer
    {
        public static void SeedAcademicYears(ApplicationDbContext context)
        {
            var currentYear = DateTime.Now.Year;

            for (int year = 2000; year < currentYear; year++)
            {
                string range = $"{year}-{year + 1}";

                if (!context.AcademicYears.Any(a => a.YearRange == range))
                {
                    context.AcademicYears.Add(new AcademicYear
                    {
                        YearRange = range
                    });
                }
            }

            context.SaveChanges();
        }
    }
}
