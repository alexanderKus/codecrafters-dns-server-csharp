using System.Text;

namespace codecrafters_dns_server;

public class DnsDomain(string[] labels)
{
    public string[] Labels { get; } = labels;
    
    public int Write(Span<byte> buffer)
    {
        var totalLength = 0;
        foreach (var label in Labels)
        {
            totalLength += WriteLabel(label, buffer[totalLength..]);
        }
        buffer[totalLength] = 0;
        return totalLength + 1;
    }

    private int WriteLabel(string label, Span<byte> buffer)
    {
        // NOTE: can this be written more efficient?
        var labelByteLength = Encoding.UTF8.GetByteCount(label);
        buffer[0] = (byte)labelByteLength;
        Encoding.UTF8.GetBytes(label, buffer[1..]);
        return labelByteLength + 1;
    }
}