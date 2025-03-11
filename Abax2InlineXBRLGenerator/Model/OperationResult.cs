namespace AbaxXBRL.Aplanet.Integration.Model;

/// <summary>
/// Represents the result of an operation.
/// </summary>
/// <typeparam name="T">the type of the result</typeparam>
public class OperationResultDTO<T>
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }
    /// <summary>
    /// The message returned by the operation.
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// The result of the operation.
    /// </summary>
    public T? Result { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResultDTO{T}"/> class.
    /// </summary>
    /// <param name="isSuccess">whether the operation was successful</param>
    /// <param name="message">message returned by the operation</param>
    /// <param name="result">result of the operation</param>
    public OperationResultDTO(bool isSuccess, string message, T result)
    {
        IsSuccess = isSuccess;
        Message = message;
        Result = result;
    }
}
