namespace codecrafters_dns_server;

public class DnsHeader(byte[] data)
{
    public ushort Id { get; } = 1234; //(ushort)((data[0] << 8) | data[1]);
    public bool IsResponse { get; private set; } = (data[2] & 0x80) != 0;
    // NOTE: is this need to be int ???
    public int OpCode { get; } = (data[2] >> 3) & 0xF0;
    public bool IsAuthoritative { get; } = (data[2] & 0x4) != 0;
    public bool IsTruncated { get; } = (data[2] & 0x2) != 0;
    public bool IsRecursionDesired { get; } = (data[2] & 0x1) != 0;
    public bool IsRecursionAvailable { get; } = (data[3] & 0xF) != 0;
    // NOTE: are those need to be int ???
    public int ResponseCode { get; } = data[3] & 0xF;
    public int QuestionCount { get; set; } = (data[4] << 8) | data[5];
    public int AnswerCount { get; set; } = (data[6] << 8) | data[7];
    public int NameServerCount { get; set; } = (data[8] << 8) | data[9];
    public int AdditionalCount { get; set; } = (data[10] << 8) | data[11];

    public int Write(Span<byte> buffer)
    {
        IsResponse = true;
        buffer[0] = (byte)(Id >> 8);
        buffer[1] = (byte)(Id & 0xFF);
        buffer[2] = (byte)((IsResponse ? 1 : 0) << 7 | (OpCode & 0xF) << 3 |
                           (IsAuthoritative ? 1 : 0) << 2 |
                           (IsTruncated ? 1 : 0) << 1 | (IsRecursionDesired ? 1 : 0));
        buffer[3] = (byte)((IsRecursionAvailable ? 1 : 0) << 7 | (ResponseCode & 0xF));
        buffer[4] = (byte)(QuestionCount >> 8);
        buffer[5] = (byte)(QuestionCount & 0xFF);
        buffer[6] = (byte)(AnswerCount >> 8);
        buffer[7] = (byte)(AnswerCount & 0xFF);
        buffer[8] = (byte)(NameServerCount >> 8);
        buffer[9] = (byte)(NameServerCount & 0xFF);
        buffer[10] = (byte)(AdditionalCount >> 8);
        buffer[11] = (byte)(AdditionalCount & 0xFF);
        return 12;
    }
}