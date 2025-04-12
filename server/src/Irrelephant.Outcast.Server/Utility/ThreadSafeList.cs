using System.Collections;

namespace Irrelephant.Outcast.Server.Utility;

public class ThreadSafeList<TItem>(List<TItem> inner, ReaderWriterLockSlim rwLock) : IList<TItem>
{
    public IEnumerator<TItem> GetEnumerator() => new ThreadSafeEnumerator<TItem>(inner.GetEnumerator(), rwLock);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(TItem item)
    {
        using (rwLock.LockForWrite())
        {
            inner.Add(item);
        }
    }

    public void Clear()
    {
        using (rwLock.LockForWrite())
        {
            inner.Clear();
        }
    }

    public bool Contains(TItem item)
    {
        using (rwLock.LockForRead())
        {
            return inner.Contains(item);
        }
    }

    public void CopyTo(TItem[] array, int arrayIndex)
    {
        using (rwLock.LockForRead())
        {
            inner.CopyTo(array, arrayIndex);
        }
    }

    public bool Remove(TItem item)
    {
        using (rwLock.LockForWrite())
        {
            return inner.Remove(item);
        }
    }

    public int Count
    {
        get
        {
            using (rwLock.LockForRead())
            {
                return inner.Count;
            }
        }
    }

    public bool IsReadOnly => false;

    public int IndexOf(TItem item)
    {
        using (rwLock.LockForRead())
        {
            return inner.IndexOf(item);
        }
    }

    public void Insert(int index, TItem item)
    {
        using (rwLock.LockForWrite())
        {
            inner.Insert(index, item);
        }
    }

    public void RemoveAt(int index)
    {
        using (rwLock.LockForWrite())
        {
            inner.RemoveAt(index);
        }
    }

    public TItem this[int index]
    {
        get
        {
            using (rwLock.LockForRead())
            {
                return inner[index];
            }
        }
        set {
            using (rwLock.LockForWrite())
            {
                inner[index] = value;
            }
        }
    }
}
