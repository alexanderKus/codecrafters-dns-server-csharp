using System.Buffers.Binary;
using System.Text;

namespace codecrafters_dns_server;

internal static class DnsParser
{
    private static byte CompressedQuestionMask { get; } = 0xc0;
    
    public static (int len, DnsQuestion question) ParserDnsQuestion(Span<byte> data, Span<byte> buffer)
    {
        var (len, domain) = ParseDnsDomain(data, buffer);
        var type = BinaryPrimitives.ReadUInt16BigEndian(data[len..(len+2)]);
        var cls = BinaryPrimitives.ReadUInt16BigEndian(data[(len+2)..(len+4)]);
        var question = new DnsQuestion(domain, type, cls);
        return (len+4, question);
    }
    
    public static (int len, DnsResourceRecords question) ParserDnsResourceRecord(Span<byte> data, Span<byte> buffer)
    {
        var (len, domain) = ParseDnsDomain(data, buffer);
        var type = BinaryPrimitives.ReadUInt16BigEndian(data[len..(len+2)]);
        var cls = BinaryPrimitives.ReadUInt16BigEndian(data[(len+2)..(len+4)]);
        var ttl = BinaryPrimitives.ReadUInt32BigEndian(data[(len + 4)..(len + 8)]);
        var length = BinaryPrimitives.ReadUInt16BigEndian(data[(len+8)..(len+10)]);
        var d = data[(len + 10)..].ToArray();
        var question = new DnsResourceRecords(domain, type, cls, ttl, length, d);
        return (len+10+length, question);
    }

    private static (int length, DnsDomain domain) ParseDnsDomain(Span<byte> data, Span<byte> buffer)
    {
        List<string> labels = [];
        var index = 0;
        var wasCompressed = false;
        while (data[index] != 0) 
        {
            var strLen = data[index];
            if (IsQuestionCompressed(strLen))
            {
                var offsetPtr = BinaryPrimitives.ReadUInt16BigEndian(data[index..]) & 0x3fff;
                var (_, domain) = ParseDnsDomain(buffer[offsetPtr..], buffer);
                labels.AddRange(domain.Labels);
                index += 2;
                wasCompressed = true;
            }
            else
            {
                var label = Encoding.ASCII.GetString(data[index..].Slice(1, strLen));
                labels.Add(label);
                index += 1 + strLen;
            }
        }
        return (wasCompressed ? index : index + 1, new DnsDomain(labels.ToArray()));
    }

    private static bool IsQuestionCompressed(byte value)
        => (value & CompressedQuestionMask) == CompressedQuestionMask;
}