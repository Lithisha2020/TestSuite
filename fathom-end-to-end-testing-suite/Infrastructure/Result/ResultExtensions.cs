using System;

namespace fathom_end_to_end_testing_suite.Infrastructure
{
    public static class ResultExtensions
    {

        public static Result<K> OnSuccess<T, K>(this Result<T> result, Func<T, K> func)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error);

            return Result.Ok(func(result.Value));
        }

        public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, string errorMessage)
        {
            if (result.IsFailure)
                return result;

            if (!predicate(result.Value))
                return Result.Fail<T>(errorMessage);

            return result;
        }

        public static Result<K> Map<T, K>(this Result<T> result, Func<T, K> func)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error);

            return Result.Ok(func(result.Value));
        }

        public static Result<K> AndThen<T, K>(this Result<T> result, Func<T, Result<K>> func)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error);

            return func(result.Value);
        }

        public static Result AndThen<T>(this Result<T> result, Func<T, Result> func)
        {
            if (result.IsFailure)
                return Result.Fail(result.Error);

            return func(result.Value);
        }

        public static Result AndThen(this Result result, Func<Result> func)
        {
            if (result.IsFailure)
                return Result.Fail(result.Error);

            return func();
        }

        public static Result<T> AndThen<T>(this Result result, Func<Result<T>> func)
        {
            if (result.IsFailure)
                return Result.Fail<T>(result.Error);

            return func();
        }

        public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsSuccess)
            {
                action(result.Value);
            }

            return result;
        }

        public static Result<T> OnFailure<T>(this Result<T> result, Action<string, T> action)
        {
            if (result.IsFailure)
            {
                action(result.Error, default(T));
            }

            return result;
        }

        public static T OnBoth<T>(this Result result, Func<Result, T> func)
        {
            return func(result);
        }

        public static Result OnSuccess(this Result result, Action action)
        {
            if (result.IsSuccess)
            {
                action();
            }

            return result;
        }

        public static Result<K> OnSuccess<K>(this Result result, Func<K> func)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error);

            return Result.Ok(func());
        }

        public static Result OnFailure(this Result result, Action<string> action)
        {
            if (result.IsFailure)
            {
                action(result.Error);
            }

            return result;
        }

        public static Result<K> OnFailure<K>(this Result result, Func<string, Result<K>> func)
        {
            if (result.IsFailure)
                return func(result.Error);

            return Result.Ok(default(K));
        }

        public static Result<T> As<T>(this Result result)
        {
            if (result.IsFailure)
                return Result.Fail<T>(result.Error);

            return Result.Ok(default(T));
        }

        public static Result<T> Do<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsFailure)
                return result;

            action(result.Value);

            return result;
        }

        public static Result AsResult<T>(this Result<T> result)
        {
            if (result.IsFailure)
                return Result.Fail(result.Error);

            return Result.Ok();
        }

    }
}
