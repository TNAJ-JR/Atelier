using System.Text.Json.Serialization;

namespace Atelier.Common
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message = string.Empty;
        [JsonIgnore]
        public ServiceErrorType ErrorType { get; set; } = ServiceErrorType.None;
        public IList<string>? Errors { get; set; }
        public T Data { get; set; }

        public new static ServiceResult<T> Ok(T data, string message = "")
            => new() { IsSuccess = true, Data = data, Message = message };

        public new static ServiceResult<T> Fail(string message, ServiceErrorType errorType, IList<string>? errors = null)
            => new() { IsSuccess = false, Message = message, ErrorType = errorType, Errors = errors };
    }
}
