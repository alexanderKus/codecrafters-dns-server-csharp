using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace codecrafters_dns_server;

public class DnsMessage
{
    public DnsHeader Header { get; }
    public List<DnsQuestion> Question { get; private set; } = [];
    public List<DnsResourceRecords> Answer { get; private set; } = [];
    private readonly IPEndPoint? _resolverUdpEndPoint = null!;
    private readonly UdpClient? _resolverUdpClient = null!;

    public DnsMessage(byte[] data, IPEndPoint? resolverUdpEndPoint)
    {
        if (resolverUdpEndPoint is not null)
        {
            _resolverUdpEndPoint = resolverUdpEndPoint;
            _resolverUdpClient = new UdpClient();
        }
        Header = new DnsHeader(data[..12]);
        var offset = 12;
        for (var i = 0; i < Header.QuestionCount; i++)
        {
            var (len, question) = DnsParser.ParserDnsQuestion(data.AsSpan()[offset..], data);
            offset += len;
            AddDnsQuestion(question);
            if (_resolverUdpEndPoint is not null)
            {
                Span<byte> buffer = new byte[1024];
                var headerCopy = Header.MakeCopy();
                headerCopy.CopyTo(buffer);
                var length = question.Write(buffer[12..]);
                _resolverUdpClient.Send(buffer[..length].ToArray(), length, _resolverUdpEndPoint);
                var resolverResult = _resolverUdpClient.Receive(ref _resolverUdpEndPoint).AsSpan();
                var (questionLength, _) = DnsParser.ParserDnsQuestion(resolverResult[12..], resolverResult);
                var (_, resolverResourceRecords) = DnsParser.ParserDnsResourceRecord(resolverResult[(questionLength + 12)..], resolverResult);
                Console.WriteLine(JsonSerializer.Serialize(resolverResourceRecords));
                AddDnsResourceRecord(resolverResourceRecords);
            }
            else
            {
                AddDnsResourceRecord(new DnsResourceRecords(
                    name: question.Name, question.Type, cls: question.Class, ttl: 60, length:4, data: new Memory<byte>([8,8,8,8])));
            }
        }
    }
    public void AddDnsQuestion(DnsQuestion question)
    {
        Question.Add(question);
    }

    public void AddDnsResourceRecord(DnsResourceRecords resourceRecords)
    {
        Answer.Add(resourceRecords);
        Header.AnswerCount += 1;
    }
    public byte[] GetResponse()
    {
        var memory = new Memory<byte>(new byte[4096]);
        var offset = Header.Write(memory[..12].Span);
        offset = Question.Aggregate(offset, (current, question) 
            => current + question.Write(memory[current..].Span));
        offset = Answer.Aggregate(offset, (current, answer) 
            => current + answer.Write(memory[current..].Span));
        // NOTE: Is this copy need?
        return memory[..offset].ToArray();
    }
}