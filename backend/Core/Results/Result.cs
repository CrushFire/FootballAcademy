using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Results
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorMessage { get; private set; }
        public int StatusCode { get; private set; }

        private Result(bool isSuccess, T? data, string? errorMessage, int statusCode)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
        }

        public static Result<T> Success(T? data = default) =>
            new(true, data, null, 200);

        public static Result<T> Failure(string errorMessage, int statusCode = 400, T? data = default) =>
            new(false, data, errorMessage, statusCode);
    }
}
