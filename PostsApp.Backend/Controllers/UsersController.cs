using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using PostsApp.Backend.Models;
using PostsApp.Backend.ResponseModels;
using System.Data.SqlClient;

namespace PostsApp.Backend.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IMapper mapper;

        public UsersController(IConfiguration config)
        {
            this.config = config;

            var mapperConfig = new MapperConfiguration(cfg => {
                cfg.CreateMap<Post, PostWithUserInfoDTO>();
            });
            this.mapper = mapperConfig.CreateMapper();
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDTO>>> GetUsers()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var users = await connection.QueryAsync<UserDTO>("SELECT * FROM Users");
            return Ok(users);
        }

        [HttpGet("{email}")]
        public async Task<ActionResult<UserDTO>> GetUserByEmail([FromRoute] string email)
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var parameters = new { email = email };
            var isUserFound = connection.QueryFirstOrDefault<UserDTO>("SELECT * FROM Users WHERE Email = @email", parameters);
            var sqlQuery = "SELECT * FROM Users WHERE Email = @email";
            if (isUserFound == null)
                return BadRequest();  
            var queryResult = await connection.QueryFirstAsync<UserDTO>(sqlQuery, parameters);
            return Ok(queryResult);

        }

        [HttpGet("posts")]
        public async Task<ActionResult<List<PostWithUserInfoDTO>>> GetPostsWithUserInfo()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var sqlQuery = @"SELECT *
                           FROM Posts p
                           JOIN Users u ON p.UserId = u.Id
                           ORDER BY p.CreatedDate ASC";
            var queryResult = connection.Query<PostWithUserInfoDTO, UserDTO, PostWithUserInfoDTO>(
                sqlQuery,
                (post, user) =>
                {
                    post.User = user;
                    return post;
                }
                );
            return Ok(mapper.Map<List<PostWithUserInfoDTO>>(queryResult));
        }

        [HttpGet("{email}/posts")]
        public async Task<ActionResult<List<PostWithUserInfoDTO>>> GetPostsByEmail([FromRoute] string email)
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var parameters = new { email = email };
            var isUserFound = connection.ExecuteScalar<bool>("SELECT COUNT(1) FROM Users WHERE Email = @email", parameters);
            var sqlQuery = @"SELECT *
                           FROM Posts p
                           JOIN Users u ON p.UserId = u.Id
                           WHERE u.Email = @email
                           ORDER BY p.CreatedDate ASC";
            if (isUserFound)
            {
                var queryResult = connection.Query<PostWithUserInfoDTO, UserDTO, PostWithUserInfoDTO>(
                sqlQuery,
                (post, user) =>
            {
                    post.User = user;
                    return post;
                },
                parameters
                );
                return Ok(mapper.Map<List<PostWithUserInfoDTO>>(queryResult));
            }
            return BadRequest();
        }

    }
}
