namespace PostsApp.Backend.Models
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid Userid { get; set; }
        public User User { get; set; }
        public List<PostMessage> PostMessages { get; set; }
    }
}
