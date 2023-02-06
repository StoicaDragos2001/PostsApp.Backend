using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PostsApp.Backend.Models;
using System.Data.SqlClient;

namespace PostsApp.Backend.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IConfiguration config;

        public MessagesController(IConfiguration config)
        {
            this.config = config;
        }

        [HttpGet]
        public async Task<ActionResult<List<Message>>> GetMessages()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var messages = await connection.QueryAsync<Message>("SELECT * FROM Posts");
            return Ok(messages);
        }
    }
}
