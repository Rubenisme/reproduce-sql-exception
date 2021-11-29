using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReproduceSqlException.Db;
using System;
using System.Threading.Tasks;

namespace ReproduceSqlException
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly Func<TestDbContext> _contextFactory;

        public ValuesController(Func<TestDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        [HttpGet]
        public async Task<ContentResult> Test()
        {
            await using var context = _contextFactory();
            var connectionString = context.Database.GetConnectionString();

            return new(){Content = "Hello world" , ContentType = "text/html", StatusCode = StatusCodes.Status200OK };
        }
    }
}
