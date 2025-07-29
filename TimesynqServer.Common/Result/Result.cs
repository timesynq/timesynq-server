namespace TimesynqServer.Common.Result
{
    public abstract class Result
    {
        public static Result Success() => new SuccessResult();
        public static Result Failure(DomainError error) => new ErrorResult(error);

        public abstract TOut Match<TOut>
        (
            Func<TOut> onSuccess,
            Func<DomainError, TOut> onFailure
        );

        private sealed class SuccessResult : Result
        {
            public override TOut Match<TOut>
            (
                Func<TOut> onSuccess,
                Func<DomainError, TOut> onFailure
            ) => onSuccess();
        }


        private sealed class ErrorResult(DomainError error) : Result
        {
            public DomainError Error => error ?? 
                throw new ArgumentNullException(nameof(error));

            public override TOut Match<TOut>
            (
                Func<TOut> onSuccess,
                Func<DomainError, TOut> onFailure
            ) => onFailure(Error);
        }
    }
}
