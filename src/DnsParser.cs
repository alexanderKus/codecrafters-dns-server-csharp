using System.Buffers.Binary;
using System.Text;

namespace codecrafters_dns_server;

internal static class DnsParser
{
    private static byte CompressedQuestionMask { get; } = 0xc0;
    
    public static (int len, DnsQuestion question) ParserDnsQuestion(Span<byte> data, Span<byte> buffer)
    {
        var (len, domain) = ParseDnsDomain(data, buffer);
        var type = BinaryPrimitives.ReadUInt16BigEndian(data[(len+1)..(len+3)]);
        var cls = BinaryPrimitives.ReadUInt16BigEndian(data[(len+3)..(len+5)]);
        var question = new DnsQuestion(domain, type, cls);
        return (len+5, question);
    }

    private static (int length, DnsDomain domain) ParseDnsDomain(Span<byte> data, Span<byte> buffer)
    {
        var offset = 0;
        var labels = new string[10];
        var index = 0;
        while (true)
        {
            var len = data[offset];
            if (IsQuestionCompressed(len))
            {
                var offsetPtr = BinaryPrimitives.ReadUInt16BigEndian(data) & 0x3fff;
                var (_, domain) = ParseDnsDomain(buffer[offsetPtr..], buffer);
                // NOTE: actually the length of Question is 2, but we later adds 1, so 1 needs to be subtracted. 
                return (1, domain);
            }
            if (len == 0) break;
            var label = Encoding.ASCII.GetString(data[(offset+1)..(offset+1+len)]);
            labels[index++] = label;
            offset += len+1;
        }

        return (offset, new DnsDomain(labels.Where(x=> !string.IsNullOrEmpty(x)).ToArray()));
    }

    private static bool IsQuestionCompressed(byte value)
        => (value & CompressedQuestionMask) == CompressedQuestionMask;
}