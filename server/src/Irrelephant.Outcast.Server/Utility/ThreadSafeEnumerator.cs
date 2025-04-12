using System.Collections;

namespace Irrelephant.Outcast.Server.Utility;

public class ThreadSafeEnumerator<TItem> : IEnumerator<TItem>
{
    private readonly IEnumerator<TItem> _inner;
    private readonly ReaderWriterLockSlim _rwLock;

    public ThreadSafeEnumerator(IEnumerator<TItem> inner, ReaderWriterLockSlim rwLock)
    {
        _inner = inner;
        _rwLock = rwLock;

        _rwLock.EnterReadLock();
    }

    public void Dispose()
    {
        _rwLock.ExitReadLock();
    }

    public bool MoveNext() => _inner.MoveNext();

    public void Reset() => _inner.Reset();

    TItem IEnumerator<TItem>.Current => _inner.Current;

    object? IEnumerator.Current => _inner.Current;
}
