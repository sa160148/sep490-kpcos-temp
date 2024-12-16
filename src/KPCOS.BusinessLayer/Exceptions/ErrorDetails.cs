namespace KPCOS.BusinessLayer.Exceptions;

public class ErrorDetails
{
    public DateTime timestamp { get; set; }
    public int Status { get; set; }
    public string Path { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; }
}