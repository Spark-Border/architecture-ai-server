namespace ArchitectureAI.Common.Common.Responses;

public class Response<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static Response<T> Success(T data, string message = "Success", int statusCode = 200)
    {
        return new Response<T>
        {
            Data = data,
            Message = message,
            StatusCode = statusCode,
        };
    }

    public static Response<T> Failure(string message, int statusCode = 400)
    {
        return new Response<T> { Message = message, StatusCode = statusCode };
    }
}
