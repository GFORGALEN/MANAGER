using ConstructionManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("tables")]
        public IActionResult GetTables()
        {
            var tables = _context.Model.GetEntityTypes()
                .Select(t => t.GetTableName())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .Distinct()
                .ToList();

            return Ok(tables);
        }

        [HttpPost("echo")]
        public IActionResult Echo([FromBody] TestPostRequest request)
        {
            return Ok(new
            {
                request.Name,
                request.Message,
                receivedAt = DateTime.UtcNow
            });
        }
    }

    public class TestPostRequest
    {
        public required string Name { get; set; }

        public required string Message { get; set; }
    }
}
