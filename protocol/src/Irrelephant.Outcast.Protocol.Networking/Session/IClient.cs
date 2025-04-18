namespace Irrelephant.Outcast.Protocol.Networking.Session;

public interface IClient : IDisposable
{
    Guid SessionId { get; set; }

    string ClientName { get; set; }

    public IMessageHandler MessageHandler { get; }

    public IProtocolHandler ProtocolHandler { get; }
}
