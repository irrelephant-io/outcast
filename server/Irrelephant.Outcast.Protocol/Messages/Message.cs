namespace Irrelephant.Outcast.Protocol.Messages;

public record Message;

public record ConnectRequest(string Name) : Message;
