using System;

namespace fathom_end_to_end_testing_suite.Infrastructure
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string Error { get; }
        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, string error)
        {
            if (isSuccess && error != string.Empty)
                throw new InvalidOperationException();
            if (!isSuccess && error == string.Empty)
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Fail(string message)
        {
            return new Result(false, message);
        }

        public static Result<T> Fail<T>(string message)
        {
            return new Result<T>(default(T), false, message);
        }

        public static Result<T> Try<T>(Func<T> f, string message = null)
        {
            T result = default(T);

            try
            {
                result = f();
            }
            catch (Exception e)
            {
                return Result.Fail<T>(message != null ? $"{message}. {e.GetAllMessages()}" : e.GetAllMessages());
            }

            return Result.Ok(result);
        }

        public static Result Try(Action f, string message = null)
        {
            try
            {
                f();
            }
            catch (Exception e)
            {
                return Result.Fail(message != null ? $"{message}. {e.GetAllMessages()}" : e.GetAllMessages());
            }

            return Result.Ok();
        }


        public static Result Ok()
        {
            return new Result(true, string.Empty);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(value, true, string.Empty);
        }

        public static Result Combine(params Result[] results)
        {
            foreach (Result result in results)
            {
                if (result.IsFailure)
                    return result;
            }

            return Ok();
        }
    }

    public class Result<T> : Result
    {
        protected readonly T _value;

        public virtual T Value
        {
            get
            {
                if (!IsSuccess)
                    throw new InvalidOperationException();

                return _value;
            }
        }

        protected internal Result(T value, bool isSuccess, string error)
            : base(isSuccess, error)
        {
            _value = value;
        }
    }
}
