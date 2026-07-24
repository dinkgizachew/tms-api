using TmsApi.Application.Dtos;
using TmsApi.Application.Services;

namespace TmsApi.Application.Interfaces;

public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);

    Task<CourseRecord> AddAsync(string code, string title);
    Task<CourseRecord?> GetByCodeAsync(string code);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string code);
}
