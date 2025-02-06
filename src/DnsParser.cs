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

    private static (int length, DnsDomain domain) ParseDnsDomain(Span<byte> data, Span<byte> buffer)
    {
        List<string> labels = [];
        var count = 0;
        while (buffer[0] != 0) {
            var strLen = data[0];
            if (IsQuestionCompressed(strLen))
            {
                var offsetPtr = BinaryPrimitives.ReadUInt16BigEndian(data) & 0x3fff;
                var (_, domain) = ParseDnsDomain(buffer[offsetPtr..], buffer);
                return (2, domain);
            }
            var label = Encoding.ASCII.GetString(data.Slice(1, strLen));
            labels.Add(label);
            count += 1 + strLen;
        }
        return (count + 1, new DnsDomain(labels.Where(x=> !string.IsNullOrEmpty(x)).ToArray()));
    }

    private static bool IsQuestionCompressed(byte value)
        => (value & CompressedQuestionMask) == CompressedQuestionMask;
}