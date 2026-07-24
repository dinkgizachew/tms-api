namespace TmsApi.Application.Dtos;

public record EnrollmentResponseDto(
    string Id,
    string StudentId,
    string CourseCode,
    DateTime EnrolledAt);
