namespace Irrelephant.Outcast.Server.Utility;

public static class ReaderWriterLockExtensions
{
    public static ReadLock LockForRead(this ReaderWriterLockSlim rwLock) => new(rwLock);

    public static WriteLock LockForWrite(this ReaderWriterLockSlim rwLock) => new(rwLock);
}

public readonly struct ReadLock : IDisposable
{
    private readonly ReaderWriterLockSlim _rwLock;

    public ReadLock(ReaderWriterLockSlim rwLock)
    {
        _rwLock = rwLock;
        rwLock.EnterReadLock();
    }

    public void Dispose()
    {
        _rwLock.ExitReadLock();
    }
}

public readonly struct WriteLock : IDisposable
{
    private readonly ReaderWriterLockSlim _rwLock;

    public WriteLock(ReaderWriterLockSlim rwLock)
    {
        _rwLock = rwLock;
        rwLock.EnterWriteLock();
    }

    public void Dispose()
    {
        _rwLock.ExitWriteLock();
    }
}
