using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record EnrollStudentRequest
{
    [Required]
    public required string StudentId { get; init; }
}
