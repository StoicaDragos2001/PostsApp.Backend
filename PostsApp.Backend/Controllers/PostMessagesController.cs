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
    }
}
