namespace HackerNewsReader.Api;

/// <summary>
/// Cache that lazily fetches new values based on configured intervals.
/// During refresh it may return previously cached value (see <paramref>waitOnRefresh</paramref>).
/// Since the refresh is lazy, triggered by invocation, the interval may be higher.
/// Returns cached value in case of consecutive refresh failures. 
/// </summary>
/// <param name="factory">Factory that creates the cached value</param>
/// <param name="interval">Refresh interval for successfully fetched values</param>
/// <param name="failureInterval">Refresh interval for failed fetch attempts</param>
/// <param name="waitOnRefresh">Waits for the result of new refresh when set to true, returns cached value otherwise</param>
/// <typeparam name="T">Type of the cached value</typeparam>
public sealed class ReadThroughCache<T>(
    Func<Task<T>> factory,
    TimeSpan interval,
    TimeSpan failureInterval,
    bool waitOnRefresh) where T : class
{
    private volatile Cached? _cached;

    public async Task<T> Get()
    {
        var cached = _cached;
        if (cached == null)
        {
            return await TriggerRefresh(cached).Task.Value;
        }
            
        var task = cached.Task.Value;
        if (task.IsCompleted)
        {
            var expiration = task.IsFaulted
                ? cached.Timespan.Add(failureInterval)
                : cached.Timespan.Add(interval);

            if (expiration <= DateTime.UtcNow)
            {
                cached = TriggerRefresh(cached);
                task = cached.Task.Value;
            }
        }
                
        if (task.IsCompletedSuccessfully)
        {
            return await task;
        }

        if (!waitOnRefresh && cached.Previous != null)
        {
            return cached.Previous;
        }

        try
        {
            return await task;
        }
        catch when (cached.Previous != null)
        {
            return cached.Previous;
        }
    }

    private Cached TriggerRefresh(Cached? previous)
    {
        var newCached = new Cached
        {
            Timespan = DateTime.UtcNow,
            Previous = previous == null
                ? null
                : previous.Task.Value.IsCompletedSuccessfully
                    ? previous.Task.Value.Result
                    : previous.Previous,
            Task = new Lazy<Task<T>>(factory)
        };

        var original = Interlocked.CompareExchange(ref _cached, newCached, previous);
        var result = original == previous
            ? newCached
            : original!;

        // resolve the Lazy value so the task is started
        _ = result.Task.Value;
        
        return result;
    }

    private sealed class Cached
    {
        public DateTime Timespan { get; init; }
        public T? Previous { get; init; }
        public Lazy<Task<T>> Task { get; init; }
    }
}