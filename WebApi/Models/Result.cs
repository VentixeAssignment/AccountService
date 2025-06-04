namespace WebApi.Models;

public class Result<T>
{
    public bool Succeeded { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }   
}
