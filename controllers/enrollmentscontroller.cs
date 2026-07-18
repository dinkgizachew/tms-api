using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Service;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List enrolments for a course")]
    public async Task<IActionResult> GetEnrollments(
        int courseId,
        CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null)
        {
            return NotFound();
        }

        var enrollments = await enrollmentService.GetAllAsync();
        var result = enrollments
            .Where(e => e.CourseCode == course.Code)
            .Select(e => new EnrollmentResponseDto(e.Id, e.StudentId, e.CourseCode, e.EnrolledAt))
            .ToList();

        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one enrolment for a course")]
    public async Task<IActionResult> GetEnrollment(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null)
        {
            return NotFound();
        }

        var record = await enrollmentService.GetByIdAsync(id.ToString());
        if (record is null || record.CourseCode != course.Code)
        {
            return NotFound();
        }

        return Ok(new EnrollmentResponseDto(record.Id, record.StudentId, record.CourseCode, record.EnrolledAt));
    }

    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Enrol a student in a course")]
    [EndpointDescription("Returns 404 if the course does not exist, 409 if the course has reached MaxCapacity.")]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null)
        {
            return NotFound();
        }

        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course capacity reached",
                Detail = $"Course {course.Code} has reached its maximum capacity.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var record = await enrollmentService.EnrollAsync(request.StudentId, course.Code);
        var response = new EnrollmentResponseDto(record.Id, record.StudentId, record.CourseCode, record.EnrolledAt);

        return CreatedAtAction(
            nameof(GetEnrollment),
            new { courseId, id = record.Id },
            response);
    }
}
