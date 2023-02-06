using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PostsApp.Backend.Models;
using PostsApp.Backend.RequestModels;
using PostsApp.Backend.ResponseModels;
using System.Collections;
using System.Data.SqlClient;
using System.Net;

namespace PostsApp.Backend.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IMapper mapper;

        public PostsController(IConfiguration config)
        {
            this.config = config;

            var mapperConfig = new MapperConfiguration(cfg => {
                cfg.CreateMap<Post, PostDTO>();
            });
            this.mapper = mapperConfig.CreateMapper();
        }

        [HttpGet]
        public async Task<ActionResult<List<PostDTO>>> GetPosts()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var posts = await connection.QueryAsync<PostDTO>("SELECT * FROM Posts");
            return Ok(posts);
        }

        [HttpGet("noMessages")]
        public async Task<ActionResult<List<PostDTO>>> GetPostsWithoutMessages()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var sqlQuery = @"SELECT *
                            FROM Posts p
                            LEFT OUTER JOIN PostMessages pm ON pm.PostId = p.Id
                            WHERE pm.MessageId IS NULL";
            var queryResult = connection.Query<Post, PostMessage, Post>(
                sqlQuery,
                (post, postMessage) =>
                {
                    //postMessage.Post = post;
                    return post;
                }
                );
            return Ok(mapper.Map<List<PostDTO>>(queryResult));
        }

        [HttpGet("messages")]
        public async Task<ActionResult<List<PostDTO>>> GetPostsWithMessages()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var sqlQuery = @"SELECT *
                            FROM Posts p
                            JOIN PostMessages pm ON pm.PostId = p.Id
                            WHERE pm.MessageId IS NOT NULL";
            var queryResult = connection.Query<Post, PostMessage, Post>(
                sqlQuery,
                (post, postMessage) =>
                {
                    postMessage.Post = post;
                    return post;
                }
                );
            return Ok(mapper.Map<List<PostDTO>>(queryResult).DistinctBy(post => post.Content));
        }

        [HttpPost()]
        public async Task<ActionResult<PostDTO>> CreatePost([FromBody] PostModelRequest post)
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var requestParameters = new { userId = post.UserId, content = post.Content };
            var isUserFound = await connection.QueryAsync<User>("SELECT * FROM Users WHERE Id = @userId", requestParameters);
            if (isUserFound.Count() == 0)
                {
                return BadRequest();
                }
            await connection.ExecuteAsync("INSERT INTO Posts (Content, UserId) VALUES (@Content, @UserId)", requestParameters);
            return Created(new Uri("api/posts", UriKind.Relative), post);
        }

        [HttpPut()]
        public async Task<ActionResult<Post>> UpdatePost([FromBody] PutModelRequest post)
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var requestParameters = new { content = post.Content, id = post.Id };
            var isPostFound = await connection.QueryAsync<User>("SELECT * FROM Posts WHERE Id = @id", requestParameters);
            if (isPostFound.Count() == 0)
                {
                return BadRequest();
            }
            await connection.ExecuteAsync("UPDATE Posts SET Content = @Content WHERE Id = @Id", post);
            return Ok(post);
        }

        [HttpDelete()]
        public async Task<ActionResult<Post>> DeletePost([FromBody] Guid postId)
        {
            //var parameters = new { email = "clabat1@paypal.com" };
            //var requestParameters = new { userId = post.UserId, content = post.Content };
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var isPostFound = await connection.QueryAsync<User>("SELECT * FROM Posts WHERE Id = @id", new { id = postId });
            if (isPostFound.Count() == 0)
            {
                return BadRequest();
            }
            await connection.ExecuteAsync("DELETE FROM Posts WHERE Id = @Id", new { Id = postId });
            return new NoContentResult();
        }
    }
}
