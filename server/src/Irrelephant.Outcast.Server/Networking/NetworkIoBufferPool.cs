using System.Net.Sockets;
using Irrelephant.Outcast.Server.Configuration;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Server.Networking;

public class NetworkIoBufferPool(IOptions<OutcastNetworkingOptions> options)
{
    /// <summary>
    /// Controls simultaneous access to critical resources.
    /// </summary>
    private readonly Lock _concurrencyLock = new();

    /// <summary>
    /// Internal buffer to be used as a basis for buffer allocation.
    /// </summary>
    private readonly byte[] _internalBuffer = new byte[
        options.Value.MaxSimultaneousConnectionRequests
        * options.Value.BufferSize
    ];

    /// <summary>
    /// Contains available offsets to be used for buffers.
    /// </summary>
    private readonly Stack<int> _freeBufferOffsets =
        new(
            Enumerable.Range(0, options.Value.MaxSimultaneousConnectionRequests)
                .Select(socketIdx => socketIdx * options.Value.BufferSize)
                .Reverse()
        );

    /// <summary>
    /// Assigns a buffer from a buffer pool to be used by the async socket handler.
    /// </summary>
    /// <param name="args">Async socket for which to allocate the buffer.</param>
    /// <exception cref="InvalidOperationException">
    /// Is thrown when attempting to assign a buffer but there are no buffers available in the pool.
    /// </exception>
    public void Assign(SocketAsyncEventArgs args)
    {
        lock (_concurrencyLock) {
            if (_freeBufferOffsets.Count > 0)
            {
                args.SetBuffer(_internalBuffer, _freeBufferOffsets.Pop(), options.Value.BufferSize);
            }
            else
            {
                throw new InvalidOperationException("Unable to allocate a buffer for socket.");
            }
        }
    }

    /// <summary>
    /// Releases a buffer from the async socket handler.
    /// </summary>
    /// <param name="args">Async socket for which to release the buffer.</param>
    public void Release(SocketAsyncEventArgs args)
    {
        lock (_concurrencyLock)
        {
            _freeBufferOffsets.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
