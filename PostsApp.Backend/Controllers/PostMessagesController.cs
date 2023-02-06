using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PostsApp.Backend.Models;
using System.Data.SqlClient;

namespace PostsApp.Backend.Controllers
{
    [Route("api/postMessages")]
    [ApiController]
    public class PostMessagesController : ControllerBase
    {
        private readonly IConfiguration config;

        public PostMessagesController(IConfiguration config)
        {
            this.config = config;
        }

        [HttpGet]
        public async Task<ActionResult<List<PostMessage>>> GetMessages()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var messages = await connection.QueryAsync<PostMessage>("SELECT * FROM PostMessages");
            return Ok(messages);
        }
    }
}
