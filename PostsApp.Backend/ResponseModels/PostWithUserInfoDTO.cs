using PostsApp.Backend.Models;

namespace PostsApp.Backend.ResponseModels
{
    public class PostWithUserInfoDTO
    {
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public UserDTO User { get; set; }
    }
}
