using System.Text.Json.Serialization;

namespace Atelier.Common;

public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    [JsonIgnore]
    public ServiceErrorType ErrorType { get; set; } = ServiceErrorType.None;
    public IList<string>? Errors { get; set; }
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data, string message = "") =>
        new() { IsSuccess = true, Data = data, Message = message };
    public static ServiceResult<T> Fail(string message, ServiceErrorType errorType, IList<string>? errors = null) =>
        new() { Message = message, ErrorType = errorType, Errors = errors };
}
