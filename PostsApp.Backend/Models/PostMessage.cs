namespace PostsApp.Backend.Models
{
    public class PostMessage
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public Guid MessageId { get; set; }
        public Post Post { get; set; }
        //
        public Message Message { get; set; }
    }
}
