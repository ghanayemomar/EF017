using C01.SqlQuery.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace C01.SqlQuery
{
    class Program
    {
        public static void Main(string[] args)
        {
            GlobalQueryFilter();
            Console.ReadKey();
        }
        private static void SqlQuery()
        {
            using (var context = new AppDbContext())
            {
                var courses = context.Courses.FromSql($"SELECT * FROM dbo.Courses");
                foreach (var c in courses)
                {
                    Console.WriteLine($"{c.CourseName} ({c.HoursToComplete})");
                }
            }
            Console.ReadKey();
        }
        private static void passSqlParameter()
        {
            // on DbSet.
            // primary key only to search.
            // (local cache) first. if not exist it queries the database.
            // Returns null if the entity is not found.

            //var c1 = context.Courses.Find(1);
            //Console.WriteLine($"{c1.CourseName} ({c1.HoursToComplete})");

            // On IEnumerable or IQueryable.
            // Retrieves the first element of a sequence, or a default value.
            // You can provide a predicate(a condition) to filter the results. 
            //var c2 = context.Courses.FirstOrDefault(x => x.Id == 1);
            //Console.WriteLine($"{c2.CourseName} ({c2.HoursToComplete})");

            // On IEnumerable or IQueryable.
            // Retrieves the single element of a sequence or a default value.
            // more than one element satisfies, exception thrown.
            // Useful when you expect the query to return only one result.

            //var c3 = context.Courses.SingleOrDefault(x => x.Id == 1);
            //Console.WriteLine($"{c3.CourseName} ({c3.HoursToComplete})");
            using (var context = new AppDbContext())
            {

                var c1 = context.Courses
                .FromSql($"SELECT * FROM dbo.Courses Where Id = {1}")
                .FirstOrDefault();
                Console.WriteLine($"{c1.CourseName} ({c1.HoursToComplete})");

                var c2 = context.Courses
                   .FromSqlInterpolated($"SELECT * FROM dbo.Courses Where Id = {1}")
                   .FirstOrDefault();

                Console.WriteLine($"{c2.CourseName} ({c2.HoursToComplete})");

                // var courseId = "1; DELETE FROM dbo.Courses";

                var courseIdParam = new SqlParameter("@courseId", 1);
                var c3 = context.Courses
                .FromSqlRaw("SELECT * FROM dbo.Courses Where Id = @courseId", courseIdParam)
                .FirstOrDefault();

                Console.WriteLine($"{c3.CourseName} ({c3.HoursToComplete})");
            }

        }
        private static void CallingStoredProcedure()
        {
            using (var context = new AppDbContext())
            {
                var startDateParam = new SqlParameter("@StartDate", System.Data.SqlDbType.Date)
                {
                    Value = new DateTime(2023, 01, 01)
                };
                var endDateParam = new SqlParameter("@EndDate", System.Data.SqlDbType.Date)
                {
                    Value = new DateTime(2023, 06, 30)
                };

                var sections = context.SectionWithDetails
                    .FromSql($"Exec dbo.sp_GetSectionWithninDateRange {startDateParam}, {endDateParam}")
                    .ToList();

                foreach (var s in sections)
                {
                    Console.WriteLine(s);
                }
            }
            Console.ReadKey();
        }
        private static void CourseOverView()
        {
            using (var context = new AppDbContext())
            {
                var coursesOverviews = context.CourseOverviews.ToList();
                foreach (var courseOverview in coursesOverviews)
                {
                    Console.WriteLine(courseOverview);
                }
            }
        }
        private static void CallScalerValuedFunction()
        {
            using (var context = new AppDbContext())
            {
                var startDate = new DateTime(2023, 09, 24);
                var endDate = new DateTime(2023, 12, 26);
                var startTime = new TimeSpan(08, 00, 00);
                var endTime = new TimeSpan(11, 00, 00);
                var result = context.Instructors.Select(x => new
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    DateRange = $"{startDate.ToShortDateString()} - {endDate.ToShortDateString()}",
                    TimeRange = $"{startTime.ToString("hh\\:mm")} - {startTime.ToString("hh\\:mm")}",
                    status = AppDbContext.GetInstructorAvailability(x.Id, startDate, endDate, startTime, endTime)
                }).ToList();
                foreach (var item in result)
                {
                    Console.WriteLine($"{item.Id}\t{item.FullName,-20}\t{item.DateRange}\t{item.TimeRange}\t{item.status}");
                }
            }
        }
        private static void CallTableValuedFunction()
        {
            using (var context = new AppDbContext())
            {
                foreach (var section in context.GetSectionsExceedingParticipantCount(21))
                {
                    Console.WriteLine($"{section.Id}\t{section.SectionName}\t{section.DateRange}");
                }
            }
        }
        private static void GlobalQueryFilter()
        {
            using (var context = new AppDbContext())
            {
                foreach (var section in context.Sections)
                {
                    Console.WriteLine($"{section.Id}\t{section.SectionName}\t{section.DateRange}");
                }
            }
        }




    }
}
