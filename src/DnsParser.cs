using System.Buffers.Binary;
using System.Text;

namespace codecrafters_dns_server;

internal static class DnsParser
{
    public static (int len, DnsQuestion question) ParserDnsQuestion(Span<byte> data)
    {
        var (len, domain) = ParseDnsDomain(data);
        var type = BinaryPrimitives.ReadUInt16BigEndian(data[(len+1)..(len+3)]);
        var cls = BinaryPrimitives.ReadUInt16BigEndian(data[(len+3)..(len+5)]);
        var question = new DnsQuestion(domain, type, cls);
        return (len+4, question);
    }
    
    // public static (int len, DnsResourceRecords resourceRecords) ParserDnsResourceRecords(Span<byte> data)
    // {
    //     var (len, domain) = ParseDnsDomain(data);
    //     var type = BinaryPrimitives.ReadUInt16BigEndian(data[(len+1)..(len+3)]);
    //     var cls = BinaryPrimitives.ReadUInt16BigEndian(data[(len+3)..(len+5)]);
    //     var ttl = BinaryPrimitives.ReadUInt32BigEndian(data[(len+5)..(len+7)]);
    //     var length = BinaryPrimitives.ReadUInt16BigEndian(data[(len+9)..(len+11)]);
    //     var resourceRecords = new DnsResourceRecords(domain, type, cls, ttl, length, data: new Memory<byte>(data[(len+11)..].ToArray()));
    //     return (len, resourceRecords);
    // }

    private static (int length, DnsDomain domain)  ParseDnsDomain(Span<byte> data)
    {
        var offset = 0;
        var labels = new string[10];
        var index = 0;
        while (true)
        {
            var len = data[offset];
            if (len == 0) break;
            var label = Encoding.ASCII.GetString(data[(offset+1)..(offset+1+len)]);
            labels[index++] = label;
            offset += len+1;
        }

        return (offset, new DnsDomain(labels.Where(x=> !string.IsNullOrEmpty(x)).ToArray()));
    }
}