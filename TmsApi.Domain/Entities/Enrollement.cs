
namespace TmsApi.Domain.Entities;
public class Enrollment
{
public int Id { get; set; }
public int StudentId { get; set; }
public int CourseId { get; set; }
public decimal? Grade { get; set; } // Nullable, as student may be currently enrolled
public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
public bool IsArchived {get; set ;}=false;

//For checking and working on migration we Add new properties instead of change the whole database use migration
public int Year { get; set; }

// Navigation properties back to entities
public Student Student { get; set; } = null!;
public Course Course { get; set; } = null!;
}