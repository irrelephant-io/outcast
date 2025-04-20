using System.Runtime.InteropServices;
using FluentAssertions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer;
using Irrelephant.Outcast.Networking.Tests.Fixtures;
using Irrelephant.Outcast.Networking.Transport;

namespace Irrelephant.Outcast.Networking.Tests;

public class TcpTransportSendReceiveTests(SendReceiveTestFixture fixture) : IClassFixture<SendReceiveTestFixture>
{
    private readonly ITransportHandler _serverHandler =
        TcpTransportHandler.FromSocketAsyncEventArgs(fixture.Server);

    private readonly ITransportHandler _clientHandler =
        TcpTransportHandler.FromSocketAsyncEventArgs(fixture.Client);

    [Fact]
    public async Task TestStuff()
    {
        const int messageCount = 100;

        var receivedNumbersSets = await Task.WhenAll(
            Task.Run(() => EmitAndReceiveSequentialMessagesAsync(_serverHandler, messageCount)),
            Task.Run(() => EmitAndReceiveSequentialMessagesAsync(_clientHandler, messageCount))
        );

        receivedNumbersSets[0].Should().BeEquivalentTo(receivedNumbersSets[1]);
    }

    private async Task<HashSet<int>> EmitAndReceiveSequentialMessagesAsync(
        ITransportHandler transportHandler,
        int desiredMessageCount
    )
    {
        int messagesSent = 0;

        byte[] randomJunk1 = new byte[256];
        Random.Shared.NextBytes(randomJunk1);

        byte[] randomJunk2 = new byte[256];
        Random.Shared.NextBytes(randomJunk2);

        var receivedNumbers = new HashSet<int>();

        while (messagesSent < desiredMessageCount)
        {
            transportHandler.Receive();

            while (transportHandler.InboundMessages.TryDequeue(out var message))
            {
                var firstInt = message.MessageValue[..sizeof(int)];
                receivedNumbers.Add(BitConverter.ToInt32(firstInt.Span));
            }

            // Simulate server processing time.
            await Task.Delay(Random.Shared.Next(0, 10));

            if (Random.Shared.NextDouble() > 0.5f && messagesSent < desiredMessageCount)
            {
                MemoryMarshal.Write(randomJunk1.AsMemory(0).Span, messagesSent++);
                var randomJunkSize = Random.Shared.Next(sizeof(int), randomJunk1.Length + 1);
                transportHandler.EnqueueOutboundMessage(
                    new TlvMessage(new TlvHeader(0, randomJunkSize), randomJunk1)
                );
            }

            if (Random.Shared.NextDouble() > 0.5f && messagesSent < desiredMessageCount)
            {
                MemoryMarshal.Write(randomJunk2.AsMemory(0).Span, messagesSent++);
                var randomJunkSize = Random.Shared.Next(sizeof(int), randomJunk2.Length + 1);
                transportHandler.EnqueueOutboundMessage(
                    new TlvMessage(new TlvHeader(0, randomJunkSize), randomJunk2)
                );
            }

            transportHandler.Transmit();
        }

        // Receive remaining messages
        while (receivedNumbers.Count < desiredMessageCount)
        {
            transportHandler.Receive();

            while (transportHandler.InboundMessages.TryDequeue(out var message))
            {
                var firstInt = message.MessageValue[..sizeof(int)];
                receivedNumbers.Add(BitConverter.ToInt32(firstInt.Span));
            }

            await Task.Delay(Random.Shared.Next(0, 10));
        }

        return receivedNumbers;
    }
}
