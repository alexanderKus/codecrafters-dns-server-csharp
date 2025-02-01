using System.Buffers.Binary;

namespace codecrafters_dns_server;

public class DnsQuestion(DnsDomain name, ushort type, ushort cls)
{
    public DnsDomain Name { get; } = name;
    public ushort Type { get; } = type;
    public ushort Class { get; } = cls;

    public int Write(Span<byte> buffer)
    {
        var nameLength = Name.Write(buffer);
        BinaryPrimitives.WriteUInt16BigEndian(buffer[nameLength..], Type);
        BinaryPrimitives.WriteUInt16BigEndian(buffer[(nameLength+2)..], Class);
        return nameLength + 4;
    }
}