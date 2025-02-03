namespace codecrafters_dns_server;

// NOTE: can this be struct ???
public class DnsMessage
{
    public DnsHeader Header { get; } = null!;
    public List<DnsQuestion> Question { get; private set; } = [];
    public List<DnsResourceRecords> Answer { get; private set; } = [];

    public DnsMessage(byte[] data)
    {
        Header = new DnsHeader(data[..12]);
        AddDnsQuestion(new DnsQuestion(
            name: new DnsDomain("codecrafters.io"), type: 1, cls: 1));
        AddDnsResourceRecord(new DnsResourceRecords(
            name: new DnsDomain("codecrafters.io"), type: 1, cls: 1, ttl: 60, length:4, data: new Memory<byte>([8,8,8,8])));
    }
    public void AddDnsQuestion(DnsQuestion question)
    {
        Question.Add(question);
    }

    public void AddDnsResourceRecord(DnsResourceRecords resourceRecords)
    {
        Answer.Add(resourceRecords);
    }
    public byte[] GetResponse()
    {
        var memory = new Memory<byte>(new byte[1024]);
        var offset = Header.Write(memory[..12].Span);
        offset = Question.Aggregate(offset, (current, question) 
            => current + question.Write(memory[current..].Span));
        offset = Answer.Aggregate(offset, (current, answer) 
            => current + answer.Write(memory[current..].Span));
        // NOTE: Is this copy need?
        return memory[..(12 + offset)].ToArray();
    }
}