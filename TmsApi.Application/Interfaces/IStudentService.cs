using TmsApi.Application.Dtos;
using TmsApi.Application.Services;

namespace TmsApi.Application.Interfaces;

public interface IStudentService
{
    Task<StudentRecord> AddAsync(string id, string name);
    Task<StudentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<StudentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}
