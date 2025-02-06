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
        var labels = new string[10];
        var labelsIndex = 0;
        var lenIndex = 0;
        var sectionLength = 0;
        while (true)
        {
            var len = data[lenIndex];
            var labelIndex = lenIndex + 1;
            if (IsQuestionCompressed(len))
            {
                var offsetPtr = BinaryPrimitives.ReadUInt16BigEndian(data) & 0x3fff;
                var (_, domain) = ParseDnsDomain(buffer[offsetPtr..], buffer);
                return (2, domain);
            }
            if (len == 0)
            {
                sectionLength += 1;
                break;
            }
            var label = Encoding.ASCII.GetString(data[labelIndex..(labelIndex+len)]);
            labels[labelsIndex++] = label;
            lenIndex += len + 1;
            sectionLength = lenIndex;
        }

        return (sectionLength, new DnsDomain(labels.Where(x=> !string.IsNullOrEmpty(x)).ToArray()));
    }

    private static bool IsQuestionCompressed(byte value)
        => (value & CompressedQuestionMask) == CompressedQuestionMask;
}