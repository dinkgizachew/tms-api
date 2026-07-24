using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Asp.Versioning;
using Scalar.AspNetCore;
using TmsApi;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Services;
using TmsApi.Domain.Entities;
using TmsApi.Api.Filters;
using TmsApi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

// Authentication / Authorization
builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", options =>
    {
        options.TimeProvider = TimeProvider.System;
    });
builder.Services.AddAuthorization();

// Services
builder.Services.AddScoped<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ProblemDetails registration
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v1";
});
builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v2";
});
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));
builder.Services.AddScoped<ICourseService, CourseService>();
var app = builder.Build();

// Middleware order
app.UseMiddleware<RequestLoggingMiddleware>(); // outer wrapper
// app.UseProblemDetails();   
//  // format errors/status codes as JSON
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("TMS API Reference")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        options
               .AddDocument("v1", "API Version 1.0")
               .AddDocument("v2", "API Version 2.0");
    });
}
app.UseExceptionHandler();    // catch exceptions
app.UseStatusCodePages();    // wrap empty-body codes
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();



// // Endpoints
// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001", 
//     letterGrade = "A"
// })).RequireAuthorization();

// app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
// {
//     worker.ProcessBatch();
//     return Results.Ok("processed");
// });

// app.MapPost("/api/enrollments/test", async (IEnrollmentService service) =>
// {
//     // … your test logic …
//     return Results.Ok("Logging test completed - check console for structured logs");
// });

// // Test route that throws
// app.MapGet("/api/error", () =>
// {
//     throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
// });

app.MapControllers();


// // Seed test data at startup
// using (var scope = app.Services.CreateScope())
// {
// var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
//     context.Database.Migrate(); // Applies any pending migrations; keeps migration history intact
// if (!context.Students.Any())
// {
//     var students = new List<Student>
// {
//     new() { RegistrationNumber = "TMS-2026-0001", Name = "AliceSmith", GPA = 3.8m, IsActive = true },
//     new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m, IsActive = true },
//     new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
//     new() { RegistrationNumber = "TMS-2026-0004", Name = "DianaPrince", GPA = 3.9m, IsActive = true },
//     new() { RegistrationNumber = "TMS-2026-0005", Name = "EvanWright", GPA = 2.5m, IsActive = true }
// };
// context.Students.AddRange(students);

//     var courses = new List<Course>
// {
//     new() { Code = "CS-101", Title = "Introduction to ComputerScience", MaxCapacity = 30 },
//     new() { Code = "CS-201", Title = "Data Structures and Algorithms", MaxCapacity = 25 },
//     new() { Code = "MAT-101", Title = "Calculus I", MaxCapacity =40 }
// };
// context.Courses.AddRange(courses);

// context.SaveChanges();
//     var enrollments = new List<Enrollment>
// {
// new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
// new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
// new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
// new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
// };
// context.Enrollments.AddRange(enrollments);

// context.SaveChanges();
// }
// }
// ////////M6_s2
// if (app.Environment.IsDevelopment())
// {
//     using var scope = app.Services.CreateScope();

//     var context = scope.ServiceProvider
//         .GetRequiredService<TmsDbContext>();

//     await DataSeeder.SeedAsync(context);
// }
app.Run();
