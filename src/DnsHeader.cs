using System.Buffers.Binary;

namespace codecrafters_dns_server;

public class DnsHeader(byte[] data)
{
    public ushort Id { get; } = (ushort)((data[0] << 8) | data[1]);
    public bool IsResponse { get; private set; } = (data[2] & 0x80) != 0;
    public ushort OpCode { get; } = (ushort)((data[2] >> 3) & 0xF);
    public bool IsAuthoritative { get; } = (data[2] & 0x4) != 0;
    public bool IsTruncated { get; } = (data[2] & 0x2) != 0;
    public bool IsRecursionDesired { get; } = (data[2] & 0x1) != 0;
    public bool IsRecursionAvailable { get; } = (data[3] & 0xF) != 0;
    public ushort ResponseCode { get; private set; } = (ushort)(data[3] & 0xF);
    public ushort QuestionCount { get; set; } = (ushort)((data[4] << 8) | data[5]);
    public ushort AnswerCount { get; set; } = (ushort)((data[6] << 8) | data[7]);
    public ushort NameServerCount { get; set; } = (ushort)((data[8] << 8) | data[9]);
    public ushort AdditionalCount { get; set; } = (ushort)((data[10] << 8) | data[11]);

    public int Write(Span<byte> buffer)
    {
        IsResponse = true;
        ResponseCode = OpCode == 0 ? (ushort)0 : (ushort)4;
        BinaryPrimitives.WriteUInt16BigEndian(buffer, Id);
        ushort temp = (ushort)(((IsResponse ? 1 : 0) << 7 | (OpCode & 0xF) << 3 |
                               (IsAuthoritative ? 1 : 0) << 2 |
                               (IsTruncated ? 1 : 0) << 1 | (IsRecursionDesired ? 1 : 0)) << 8);
        ushort temp2 = (ushort)((IsRecursionAvailable ? 1 : 0) << 7 | (ResponseCode & 0xF));
        BinaryPrimitives.WriteUInt16BigEndian(buffer[2..], (ushort)(temp | temp2));
        BinaryPrimitives.WriteUInt16BigEndian(buffer[4..], QuestionCount);
        BinaryPrimitives.WriteUInt16BigEndian(buffer[6..], AnswerCount);
        BinaryPrimitives.WriteUInt16BigEndian(buffer[8..], NameServerCount);
        BinaryPrimitives.WriteUInt16BigEndian(buffer[10..], AdditionalCount);
        return 12;
    }

    public ReadOnlySpan<byte> MakeCopy()
    {
        Span<byte> buffer = new byte[12];
        Write(buffer);
        return buffer;
    }
}