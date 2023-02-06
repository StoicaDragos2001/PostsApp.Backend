namespace PostsApp.Backend.RequestModels
{
    public class PostModelRequest
    {
        public string Content { get; set; }
        public Guid UserId { get; set; }
    }
}
