using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
{
Console.WriteLine("\n>>> STEP 1: Building the query object (nodatabase contact)...");
    var query = context.Students.Where(s => s.GPA >= 3.0m);
Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
    var orderedQuery = query.OrderBy(s => s.Name);
Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
    var results = orderedQuery.ToList(); // Execution is triggered here
Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");
    return Ok(results);
}
[HttpGet("translation-fail")]
public IActionResult TranslationFail()
{
   var students = context.Students
    .Where(s => s.GPA >= 3.5m)
    .ToList();

//    // Client-side
//     var students = context.Students
//     .AsEnumerable()
//     .Where(s => IsHonorRoll(s.GPA))
//     .ToList();

    return Ok(students);
}

private static bool IsHonorRoll(decimal gpa)
{
    return gpa >= 3.5m;
}
}