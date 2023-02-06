namespace PostsApp.Backend.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public PostMessage PostMessage { get; set; }
    }
}
