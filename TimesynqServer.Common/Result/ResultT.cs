namespace TimesynqServer.Common.Result
{
    public abstract class Result<T>
    {
        public static Result<T> Success(T value) => new SuccessResult(value);
        public static Result<T> Failure(DomainError error) => new ErrorResult(error);

        public abstract TOut Match<TOut>
        (
            Func<T, TOut> onSuccess,
            Func<DomainError, TOut> onFailure
        );

        private sealed class SuccessResult(T value) : Result<T>
        {
            public T Value => value ??
                throw new ArgumentNullException(nameof(value));

            public override TOut Match<TOut>
            (
                Func<T, TOut> onSuccess,
                Func<DomainError, TOut> onFailure
            ) => onSuccess(Value);
        }

        private sealed class ErrorResult(DomainError error) : Result<T>
        {
            public DomainError Error => error ??
                throw new ArgumentNullException(nameof(error));

            public override TOut Match<TOut>
            (
                Func<T, TOut> onSuccess,
                Func<DomainError, TOut> onFailure
            ) => onFailure(Error);
        }
    }
}
