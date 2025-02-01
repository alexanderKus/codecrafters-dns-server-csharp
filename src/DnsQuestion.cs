namespace codecrafters_dns_server;

public class DnsQuestion(DnsDomain name, ushort type, ushort cls)
{
    public DnsDomain Name { get; } = name;
    public ushort Type { get; } = type;
    public ushort Class { get; } = cls;

    public int Write(Span<byte> buffer)
    {
        var nameLength = Name.Write(buffer);
        buffer[nameLength + 1] = (byte)(Type >> 8);
        buffer[nameLength + 2] = (byte)Type;
        buffer[nameLength + 3] = (byte)(Class >> 8);
        buffer[nameLength + 4] = (byte)Class;
        return nameLength + 4;
    }
}