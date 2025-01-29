namespace codecrafters_dns_server;

// NOTE: can this be struct ???
public class DnsMessage(byte[] data)
{
    public ushort Id { get; } = (ushort)((data[0] << 8) | data[1]);
    public bool IsResponse { get; private set; } = (data[2] & 0x80) != 0;
    // NOTE: is this need to be int ???
    public int OpCode { get; } = (data[2] >> 3) & 0xF0;
    public bool IsAuthoritative { get; } = (data[2] & 0x4) != 0;
    public bool IsTruncated { get; } = (data[2] & 0x2) != 0;
    public bool IsRecursionDesired { get; } = (data[2] & 0x1) != 0;
    public bool IsRecursionAvailable { get; } = (data[3] & 0xF) != 0;
    // NOTE: are those need to be int ???
    public int ResponseCode { get; } = data[3] & 0xF;
    public int QuestionCount = (data[4] << 8) | data[5];
    public int AnswerCount = (data[6] << 8) | data[7];
    public int NameServerCount = (data[8] << 8) | data[9];
    public int AdditionalCount = (data[10] << 8) | data[11];

    public byte[] GetResponse()
    {
        var data = new byte[12];
        IsResponse = true;
        
        data[0] = (byte)(Id >> 8);
        data[1] = (byte)(Id & 0xFF);
        data[2] = (byte)((IsResponse ? 1 : 0) << 7 | (OpCode & 0xF) << 3 |
                         (IsAuthoritative ? 1 : 0) << 2 |
                         (IsTruncated ? 1 : 0) << 1 | (IsRecursionDesired ? 1 : 0));
        data[3] = (byte)((IsRecursionAvailable ? 1 : 0) << 7 | (ResponseCode & 0xF));
        data[4] = (byte)(QuestionCount >> 8);
        data[5] = (byte)(QuestionCount & 0xFF);
        data[6] = (byte)(AnswerCount >> 8);
        data[7] = (byte)(AnswerCount & 0xFF);
        data[8] = (byte)(NameServerCount >> 8);
        data[9] = (byte)(NameServerCount & 0xFF);
        data[10] = (byte)(AdditionalCount >> 8);
        data[11] = (byte)(AdditionalCount & 0xFF);
        return data;
    }
}