namespace EcommerceAPI.DTOs
{
    public class ImportResultDto
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string Message { get; set; } = string.Empty;
    }
}
