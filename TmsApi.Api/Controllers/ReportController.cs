using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{
    // 1. Active students with GPA >= 3.0
    [HttpGet("active-students-count")]
    public async Task<IActionResult> ActiveStudentsCount()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(count);
    }

    // 2. Courses with most enrollments
    [HttpGet("courses-by-enrollments")]
    public async Task<IActionResult> CoursesByEnrollments()
    {
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(list);
    }

    // 3. Average GPA per course
    [HttpGet("average-gpa-per-course")]
    public async Task<IActionResult> AverageGpaPerCourse()
    {
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(list);
    }

    // 4A. Students with zero enrollments (Subquery)
    [HttpGet("students-no-enrollments-a")]
    public async Task<IActionResult> StudentsNoEnrollmentsA()
    {
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    }

    // 4B. Students with zero enrollments (Left Join)
    [HttpGet("students-no-enrollments-b")]
    public async Task<IActionResult> StudentsNoEnrollmentsB()
    {
        var list = await context.Students
            .LeftJoin(
                context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, e) => new { s, e })
            .Where(x => x.e == null)
            .Select(x => x.s.Name)
            .ToListAsync();

        return Ok(list);
    }

    //***********
    //Paginate 
    [HttpGet("students/page/{page}")]
    public async Task<IActionResult> GetStudents(int page){
        int pageSize =3;

        var student =await context.Students
        .OrderBy(s=> s.Name)
        .Skip((page-1)*pageSize)
        .Take(pageSize)
        .ToListAsync();

        return Ok(student);
    }
    [HttpGet("top-course")]
    public async Task<IActionResult> TopCourse(){
        var list =await context.Enrollments
        .GroupBy(e => e.Course.Title)
        .Select(g => new{
            Course = g.Key,
            Count =g.Count()
        })
        .OrderByDescending(x =>x.Count)
        .Take(5)
        .ToListAsync();

        return Ok(list);
    }
    //Bad Qurey to acess a data
   [HttpGet("bad")]
public async Task<IActionResult> Bad(CancellationToken cancellationToken)
{
    var students = await context.Students
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    foreach (var s in students)
    {
        var count = await context.Enrollments
            .AsNoTracking()
            .CountAsync(e => e.StudentId == s.Id, cancellationToken);

        Console.WriteLine($"{s.Name}: {count} enrollments");
    }

    return Ok();
}
//Good Qurey to acess a data
[HttpGet("good")]
public async Task<IActionResult> Good(CancellationToken cancellationToken)
{
    var report = await context.Students
        .AsNoTracking()
        .Select(s => new
        {
            s.Name,
            EnrollmentCount = s.Enrollments.Count
        })
        .ToListAsync(cancellationToken);

    foreach (var r in report)
    {
        Console.WriteLine($"{r.Name}: {r.EnrollmentCount} enrollments");
    }

    return Ok(report);
}
[HttpGet("students/admin")]
public async Task <IActionResult> AdminStudents(){
    var students = await context.Students
            .IgnoreQueryFilters()
            .ToListAsync();

    return Ok(students);
}

[HttpPut("archive")]
public async Task<IActionResult> ArchiveOldEnrollments(CancellationToken cancellationToken)
{
    var cutoff = new DateTime(2025, 1, 1);

    await context.Enrollments
        .Where(e => e.EnrolledAt < cutoff)
        .ExecuteUpdateAsync(s => s
            .SetProperty(e => e.IsArchived, true),
            cancellationToken);

    return Ok("Archive completed.");
}
}