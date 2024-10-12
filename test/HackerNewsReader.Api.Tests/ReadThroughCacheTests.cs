using System.Diagnostics;
using System.Threading.Channels;

namespace HackerNewsReader.Api.Tests;

public sealed class ReadThroughCacheTests
{
    private Channel<object> _sync;

    [SetUp]
    public void SetUp()
    {
        _sync = Channel.CreateUnbounded<object>();
    }
    
    [Test]
    public async Task ReturnsNewValuesWhenIntervalIsZeroAndFactoryIsSync([Values(true, false)]bool waitOnRefresh)
    {
        var value = 0;
        var cache = new ReadThroughCache<string>(() => Task.FromResult(Interlocked.Increment(ref value).ToString()),
            TimeSpan.Zero, TimeSpan.Zero, waitOnRefresh);
        
        Assert.AreEqual("1", await cache.Get());
        Assert.AreEqual("2", await cache.Get());
        Assert.AreEqual("3", await cache.Get());
        Assert.AreEqual("4", await cache.Get());
    }
    
    [Test]
    public async Task ReturnsNewValuesWhenIntervalIsZeroAndFactoryIsSync_Multiple([Values(true, false)]bool waitOnRefresh)
    {
        var value = 0;
        var cache = new ReadThroughCache<string>(() => Task.FromResult(Interlocked.Increment(ref value).ToString()),
            TimeSpan.Zero, TimeSpan.Zero, waitOnRefresh);

        var cached1 = cache.Get();
        var cached2 = cache.Get();
        var cached3 = cache.Get();
        var cached4 = cache.Get();
        
        Assert.AreEqual("1", await cached1);
        Assert.AreEqual("2", await cached2);
        Assert.AreEqual("3", await cached3);
        Assert.AreEqual("4", await cached4);
    }
    
    [Test]
    public async Task ReusesOldValueDuringRefresh()
    {
        var value = 0;
        var cache = new ReadThroughCache<string>(async () =>
            {
                await WaitOne();
                return Interlocked.Increment(ref value).ToString();
            },
            TimeSpan.Zero, TimeSpan.Zero, false);

        await AllowOne();
        Assert.AreEqual("1", await cache.Get());
        Assert.AreEqual("1", await cache.Get());
        Assert.AreEqual("1", await cache.Get());
        
        await AllowOne();
        await Task.Delay(100); // may be floppy but there is no easy way to sync with the factory method
        Assert.AreEqual("2", await cache.Get());
    }
    
    [Test]
    public async Task WaitsForNewValueDuringRefresh()
    {
        var value = 0;
        var cache = new ReadThroughCache<string>(async () =>
            {
                await Task.Delay(1);
                return Interlocked.Increment(ref value).ToString();
            },
            TimeSpan.Zero, TimeSpan.Zero, true);

        Assert.AreEqual("1", await cache.Get());
        Assert.AreEqual("2", await cache.Get());
        Assert.AreEqual("3", await cache.Get());
        Assert.AreEqual("4", await cache.Get());
    }
    
    [Test]
    public async Task ReusesOldValueOnFailure([Values(true, false)]bool waitOnRefresh)
    {
        var value = 0;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        var cache = new ReadThroughCache<string>(async () =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var v = Interlocked.Increment(ref value);

                if (v > 1)
                {
                    throw new Exception("test");
                }
                
                return v.ToString();
            },
            TimeSpan.Zero, TimeSpan.Zero, waitOnRefresh);

        Assert.AreEqual("1", await cache.Get());
        Assert.AreEqual("1", await cache.Get());
        await Task.Delay(10);
        Assert.AreEqual("1", await cache.Get());
        Assert.AreEqual("1", await cache.Get());
    }
    
    [Test]
    public async Task ReusesCachedWithinWindow()
    {
        var value = 0;
        var cache = new ReadThroughCache<string>(() => Task.FromResult(Interlocked.Increment(ref value).ToString()),
            TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100), true);

        Assert.AreEqual("1", await cache.Get());
        Assert.AreEqual("1", await cache.Get());
        await Task.Delay(10);
        Assert.AreEqual("1", await cache.Get());
        await Task.Delay(500);
        Assert.AreEqual("2", await cache.Get());
    }
    
    [Test]
    public async Task RefreshesFailureUsingFailureInterval()
    {
        var sw = Stopwatch.StartNew();
        var refreshes = new List<TimeSpan>();
        var value = 0;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        var cache = new ReadThroughCache<string>(async () =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                refreshes.Add(sw.Elapsed);
                sw.Restart();
                
                var v = Interlocked.Increment(ref value);

                if (v > 1)
                {
                    throw new Exception("test");
                }
                
                return v.ToString();
            },
            TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(10), false);

        Assert.AreEqual("1", await cache.Get());
        await Task.Delay(200);
        
        Assert.AreEqual("1", await cache.Get());
        await Task.Delay(20);
        Assert.AreEqual("1", await cache.Get());
        
        // should not trigger refresh
        Assert.AreEqual("1", await cache.Get());
        
        await Task.Delay(20);
        Assert.AreEqual("1", await cache.Get());
        
        // remove initial fetch and first refresh
        refreshes.RemoveRange(0, 2);
        
        Assert.AreEqual(2, refreshes.Count);
        Assert.Less(refreshes[0].TotalMilliseconds, 25);
        Assert.Greater(refreshes[0].TotalMilliseconds, 15);
        Assert.Less(refreshes[1].TotalMilliseconds, 25);
        Assert.Greater(refreshes[1].TotalMilliseconds, 15);
    }

    private async Task WaitOne()
    {
        await _sync.Reader.ReadAsync();
    }

    private async Task AllowOne()
    {
        await _sync.Writer.WriteAsync(new object());
    }
}