namespace TmsApi.Dtos;

public record LinkDto(string Rel, string Href, string Method);

public record CourseDetailDto(
    int Id,
    string Code,
    string Title,
    int MaxCapacity,
    int EnrollmentCount,
    IEnumerable<LinkDto> Links);
