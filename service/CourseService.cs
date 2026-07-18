using TmsApi.Dtos;

namespace TmsApi.Service;

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

public class CourseService : ICourseService
{
    private readonly Dictionary<string, CourseRecord> _store = new();
    private readonly ILogger<CourseService> _logger;
    private int _nextId = 1;

    public CourseService(ILogger<CourseService> logger)
    {
        _logger = logger;
    }

    public Task<CourseRecord> AddAsync(string code, string title)
    {
        var record = new CourseRecord(_nextId++, code, title, 0, 0, DateTime.UtcNow);
        _store[code] = record;

        _logger.LogInformation(
            "Added course {CourseCode} with title {CourseTitle}",
            code, title);

        return Task.FromResult(record);
    }

    public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var record = _store.Values.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(record is null ? null : MapDto(record));
    }

    public Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var query = _store.Values.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c => c.Code.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                || c.Title.Contains(request.Search, StringComparison.OrdinalIgnoreCase));
        }

        query = request.OrderBy.ToLowerInvariant() switch
        {
            "code" => request.Descending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "maxcapacity" => request.Descending ? query.OrderByDescending(c => c.MaxCapacity) : query.OrderBy(c => c.MaxCapacity),
            _ => request.Descending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title)
        };

        var totalCount = query.Count();
        var items = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapDto)
            .ToList();

        return Task.FromResult(new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_store.ContainsKey(code));
    }

    public Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_store.ContainsKey(request.Code))
        {
            throw new InvalidOperationException($"A course with code '{request.Code}' already exists.");
        }

        var record = new CourseRecord(
            _nextId++,
            request.Code,
            request.Title,
            request.MaxCapacity,
            0,
            DateTime.UtcNow);

        _store[request.Code] = record;

        return Task.FromResult(MapDto(record));
    }

    public Task<CourseRecord?> GetByCodeAsync(string code)
    {
        _store.TryGetValue(code, out var record);
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {
        IReadOnlyList<CourseRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string code)
    {
        if (_store.Remove(code))
        {
            _logger.LogInformation("Deleted course {CourseCode}", code);
            return Task.FromResult(true);
        }
        else
        {
            _logger.LogWarning("Attempted to delete non-existent course {CourseCode}", code);
            return Task.FromResult(false);
        }
    }

    private static CourseResponseDto MapDto(CourseRecord record) => new(
        record.Id,
        record.Code,
        record.Title,
        record.MaxCapacity,
        record.EnrollmentCount);
}

public record CourseRecord(
    int Id,
    string Code,
    string Title,
    int MaxCapacity,
    int EnrollmentCount,
    DateTime CreatedAt);