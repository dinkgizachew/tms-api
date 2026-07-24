using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public record EnrollStudentRequest
{
    [Required]
    public required string StudentId { get; init; }
}
