### How to run

#### Using nix
```
nix run github:MariuszTrybus/dotnet-test-hackernews#hackernews-reader
```

#### Using docker compose
```
git clone https://github.com/MariuszTrybus/dotnet-test-hackernews.git
cd dotnet-test-hackernews
docker compose up --build
```

#### Using dotnet run

Requires `dotnet sdk 8` and optionally `just`
```
git clone https://github.com/MariuszTrybus/dotnet-test-hackernews.git
cd dotnet-test-hackernews
just run # or dotnet run --project src/HackerNewsReader.Api/HackerNewsReader.Api.csproj
```

### How to call the api
```
curl -v 'localhost:5000/beststories?n=200'
```

### Assumptions:
- HackerNewsApi `/beststories` returns story ids in descending score order
- we expect lots of concurrent requests to the api
- we focus on limiting the number of requests to the upstream

### Implementation decisions:
- read through cache - the implementation is more complex than side cache but guarantees single execution even in 
highly concurrent/parallel scenarios
- caching in ConcurrentDictionary vs MemoryCache - the number and size of cached items is small so
we should be able to fit all 200 in memory, additionally it gives us better control on the eviction strategy
so we can use features like fallback to previous value and background refresh
- ignoring stories that failed fetch - I think it's better (in this case) to return partial and slightly incorrect view
rather than returning an error
- integration test against HackerNewsApi - the API should be stable but I believe it's a good practice
to have some integration tests to check our assumptions about upstream contract and have a way
to make sure it's kept over time. Those tests might be run separately so upstream failure doesn't impact
build process and also to limit the number of executions (ie executing them only on nightly build)
- CancellationTokens are not used because the caching scope is small and the impact (int this case) would be minor

### Possible improvements:
- currently the cached data is stored in memory but if we wanted to scale out or often restart the service
it would be good to store the cache outside
- better cache eviction (low memory usage, good cache hit ration) - current implementation has very simple
cache eviction policy that removes all cached stories for ids that are no longer returned in the beststories set.
It's not an optimal solution if there is a possibility that evicted story goes back to the beststories set
on a subsequent request. Also the current implementation introduces a race condition which may cause unneeded fetch
of an evicted cached story with id that was just removed from beststories but is required by a request that
uses outdated view of the beststories set
- better rate limiting on the upstream - currently the load on the upstream is reduced by using read through cache
that guarantees that there is only one outgoing call to the upstream for every resource within cache window.
Additionally the maximum number of open connections to the upstream server is limited.
A leaky-bucket rate limit algorithm could be used to generate better load distribution on the upstream
- better handling of slow "sync" refresh requests - currently the used cache allows us to use previously fetched value
in case of a upstream failure, additionally refreshes for stories are executed in the background (to improve response time)
and stale responses are returned for the duration.
Cache of list of best stories is configured differently - it waits for new value - slow upstream will directly impact
the response time, it might be good to have a way to fallback to the cached value if the request duration exceeds a threshold

### Out of scope:
- Metrics and tracing
- Custom exceptions and custom error handling
- Authentication and authorization
- Rate limiting (upstream and downstream)
- Configuration management
- HTTPS
- Tweaks to the hosting configuration
- Health checks

### Notes
- tested only on Linux