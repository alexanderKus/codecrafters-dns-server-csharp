using System.Buffers.Binary;

namespace codecrafters_dns_server;

public class DnsResourceRecords(DnsDomain name, ushort type, ushort cls, uint ttl, ushort length, Memory<byte> data)
{
    public DnsDomain Name { get; } = name;
    public ushort Type { get; } = type;
    public ushort Class { get; } = cls;
    public uint Ttl { get; } = ttl;
    public ushort Length { get; } = length;
    public Memory<byte> Data { get; } = data;

    public int Write(Span<byte> buffer)
    {
        var nameLength = Name.Write(buffer);
        BinaryPrimitives.WriteUInt16BigEndian(buffer[nameLength..], Type);
        BinaryPrimitives.WriteUInt16BigEndian(buffer[(nameLength+2)..], Class);
        BinaryPrimitives.WriteUInt32BigEndian(buffer[(nameLength+4)..], Ttl);
        BinaryPrimitives.WriteUInt16BigEndian(buffer[(nameLength+8)..], Length);
        Data.Span.CopyTo(buffer[(nameLength+10)..]);
        return nameLength + 10 + buffer.Length;
    }
}