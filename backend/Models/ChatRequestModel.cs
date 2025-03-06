namespace Backend.Models
{
    public class ChatRequestModel
    {
        public string Message { get; set; }
        public int MaxTokens { get; set; } = 500;
        public double Temperature { get; set; } = 0.7;
    }
}
