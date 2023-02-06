using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PostsApp.Backend.Models;
using PostsApp.Backend.RequestModels;
using System.Collections;
using System.Data.SqlClient;

namespace PostsApp.Backend.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IConfiguration config;

        public PostsController(IConfiguration config)
        {
            this.config = config;
        }

        [HttpGet]
        public async Task<ActionResult<List<Post>>> GetPosts()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var posts = await connection.QueryAsync<Post>("SELECT * FROM Posts");
            return Ok(posts);
        }

        [HttpGet("noMessages")]
        public async Task<ActionResult<List<Post>>> GetPostsWithoutMessages()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var sqlQuery = @"SELECT *
                            FROM Posts
                            LEFT OUTER JOIN PostMessages ON PostMessages.PostId = Posts.Id
                            WHERE PostMessages.MessageId IS NULL";
            var queryResult = connection.Query<PostMessage, Post, PostMessage>(
                sqlQuery,
                (postMessage, post) =>
                {
                    postMessage.Post = post;
                    return postMessage;
                }
                );
            return Ok(queryResult);
        }

        [HttpGet("messages")]
        public async Task<ActionResult<List<Post>>> GetPostsWithMessages()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var sqlQuery = @"SELECT *
                            FROM Posts
                            LEFT OUTER JOIN PostMessages ON PostMessages.PostId = Posts.Id
                            WHERE PostMessages.MessageId IS NOT NULL";
            var queryResult = connection.Query<PostMessage, Post, PostMessage>(
                sqlQuery,
                (postMessage, post) =>
                {
                    postMessage.Post = post;
                    return postMessage;
                }
                );
            return Ok(queryResult);
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<Post>>> GetUsers()
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var sqlQuery = @"SELECT *
                           FROM Posts p
                           JOIN Users u ON p.UserId = u.Id
                           ORDER BY p.CreatedDate ASC";
            var queryResult = connection.Query<Post, User, Post>(
                sqlQuery,
                (post, user) =>
                {
                    post.User = user;
                    return post;
                }
                );
            return Ok(queryResult);
        }

        [HttpGet("users/{email}")]
        public async Task<ActionResult<List<Post>>> GetUserPosts(string email)
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            var parameters = new { email = email };
            var exists = connection.ExecuteScalar<bool>("SELECT COUNT(1) FROM Users WHERE Email = @email", parameters);
            var sqlQuery = @"SELECT *
                           FROM Posts p
                           JOIN Users u ON p.UserId = u.Id
                           WHERE u.Email = @email
                           ORDER BY p.CreatedDate ASC";
            if (exists)
            {
                var queryResult = connection.Query<Post, User, Post>(
                sqlQuery,
                (post, user) =>
                {
                    post.User = user;
                    return post;
                },
                parameters
                );
                return Ok(queryResult);
            }
            return BadRequest();
        }

        [HttpPost()]
        public async Task<ActionResult<List<Post>>> CreatePost([FromBody] PostModelRequest post)
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            Post newPost = new Post
            {
                Userid = post.UserId,
                Content = post.Content
            };
            var requestParameters = new { userId = post.UserId, content = post.Content };
            var exists = await connection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM Users WHERE CAST(Id AS CHAR(256)) = CAST(@userId AS CHAR(256))", requestParameters);
            if (exists)
            {
                //var userId = connection.QueryAsync("SELECT Id FROM Users WHERE Email = @email", parameters);
                //var requestParameters = new { userId = post.UserId, content = post.Content };
                //return BadRequest();

                await connection.ExecuteAsync("INSERT INTO Posts (Content, UserId) VALUES (@content, @userId)", requestParameters);
                return Created(new Uri("api/posts", UriKind.Relative), newPost);
            }
            return BadRequest();
        }

        [HttpPut()]
        public async Task<ActionResult<List<Post>>> UpdatePost([FromBody] PutModelRequest post)
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            Post updatedPost = new Post
            {
                Content = post.Content,
                Id = post.Id
            };
            var requestParameters = new { content = post.Content };
            //var exists = connection.ExecuteScalar<bool>("SELECT COUNT(1) FROM Posts WHERE Id = @Id", requestParameters);
            await connection.ExecuteAsync("UPDATE Posts SET Content = @Content WHERE Id = @Id", post);
            //if(exists)
            return Ok(post);
            //return Created(new Uri("api/posts", UriKind.Relative), updatedPost);
        }

        [HttpDelete("{postId}")]
        public async Task<ActionResult<List<Post>>> DeletePost(Guid postId)
        {
            using var connection = new SqlConnection(this.config.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("DELETE FROM Posts WHERE Id = @Id", new {Id = postId});
            return Ok(await SelectAllPosts(connection));
        }

        private static async Task<IEnumerable<Post>> SelectAllPosts(SqlConnection connection)
        {
            return await connection.QueryAsync<Post>("SELECT * FROM Posts");
        }
    }
}
