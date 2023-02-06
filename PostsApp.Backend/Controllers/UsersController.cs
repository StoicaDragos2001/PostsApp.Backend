using Dapper;
using Microsoft.AspNetCore.Mvc;
using PostsApp.Backend.Models;
using System.Data.SqlClient;

namespace PostsApp.Backend.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration config;

        public UsersController(IConfiguration config)
        {
            this.config = config;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var users = await connection.QueryAsync<User>("SELECT * FROM Users");
            return Ok(users);
        }

        [HttpGet("{email}")]
        public async Task<ActionResult<List<User>>> GetUser(string email)
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var parameters = new { email = email };
            var exists = connection.ExecuteScalar<bool>("SELECT COUNT(1) FROM Users WHERE Email = @email", parameters);
            var sqlQuery = "SELECT * FROM Users WHERE Email = @email";
            if (exists)
            {
                var queryResult = await connection.QueryFirstAsync<User>(sqlQuery, parameters);
                return Ok(queryResult);
            }
            return BadRequest();
        }

    }
}
