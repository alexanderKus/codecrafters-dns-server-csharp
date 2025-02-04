namespace codecrafters_dns_server;

public class DnsMessage
{
    public DnsHeader Header { get; }
    public List<DnsQuestion> Question { get; private set; } = [];
    public List<DnsResourceRecords> Answer { get; private set; } = [];

    public DnsMessage(byte[] data)
    {
        Header = new DnsHeader(data[..12]);
        var offset = 12;
        for (var i = 0; i < Header.QuestionCount; i++)
        {
            var (len, question) = DnsParser.ParserDnsQuestion(data.AsSpan()[offset..], data);
            offset += len+1;
            AddDnsQuestion(question);
            AddDnsResourceRecord(new DnsResourceRecords(
                name: question.Name, question.Type, cls: question.Class, ttl: 60, length:4, data: new Memory<byte>([8,8,8,8])));
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
        var memory = new Memory<byte>(new byte[1024]);
        var offset = Header.Write(memory[..12].Span);
        offset = Question.Aggregate(offset, (current, question) 
            => current + question.Write(memory[current..].Span));
        offset = Answer.Aggregate(offset, (current, answer) 
            => current + answer.Write(memory[current..].Span));
        // NOTE: Is this copy need?
        return memory[..offset].ToArray();
    }
}