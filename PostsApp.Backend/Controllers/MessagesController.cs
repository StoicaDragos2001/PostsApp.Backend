using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PostsApp.Backend.Models;
using PostsApp.Backend.ResponseModels;
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
        public async Task<ActionResult<List<MessageDTO>>> GetMessages()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var messages = await connection.QueryAsync<MessageDTO>("SELECT * FROM Messages");
            return Ok(messages);
        }
    }
}
