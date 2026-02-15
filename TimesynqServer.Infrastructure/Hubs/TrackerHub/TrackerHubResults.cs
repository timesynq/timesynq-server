namespace TimesynqServer.Infrastructure.Hubs.TrackerHub
{
    public readonly struct TrackerHubResult
    {
        public bool IsSuccessful { get; }
        public string? ErrorMessage { get; }
        
        private TrackerHubResult(bool isSuccessful, string? errorMessage)
        {
            IsSuccessful = isSuccessful;
            ErrorMessage = errorMessage;
        }

        public static TrackerHubResult Success() => new(true, null);
        public static TrackerHubResult Failure(string errorMessage) 
            => new(false, errorMessage ?? throw new ArgumentNullException(nameof(errorMessage)));
    }

    public readonly struct TrackerHubResult<T>
    {
        public bool IsSuccessful { get; }
        public string? ErrorMessage { get; }
        public T? Value { get; }

        private TrackerHubResult(T value)
        {
            IsSuccessful = true;
            ErrorMessage = null;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private TrackerHubResult(string errorMessage)
        {
            IsSuccessful = false;
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            Value = default;
        }

        public static TrackerHubResult<T> Success(T value) => new(value);
        public static TrackerHubResult<T> Failure(string errorMessage) => new(errorMessage);
        public static implicit operator TrackerHubResult<T>(T value) => Success(value);
    }
}