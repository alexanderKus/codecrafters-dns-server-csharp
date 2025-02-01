using System.Text;

namespace codecrafters_dns_server;

public class DnsDomain(string domain)
{
    public string[] Labels { get; } = domain.Split('.');
    
    public int Write(Span<byte> buffer)
    {
        var totalLength = 0;
        foreach (var label in Labels)
        {
            totalLength += WriteLabel(label, buffer[totalLength..]);
        }
        totalLength++;
        buffer[totalLength] = 0;
        return totalLength;
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