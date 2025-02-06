using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_dns_server;

Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
// Resolve UDP address
IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
int port = 2053;
IPEndPoint udpEndPoint = new IPEndPoint(ipAddress, port);
UdpClient udpClient = new UdpClient(udpEndPoint);

while (true)
{
    // Receive data
    IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
    byte[] receivedData = udpClient.Receive(ref sourceEndPoint);
    string receivedString = Encoding.ASCII.GetString(receivedData);
    /*
    byte[] test =
    [
        0x12, 0x34, // Transaction ID
        0x01, 0x00, // Flags (Standard query)
        0x00, 0x02, // Questions (2)
        0x00, 0x00, // Answer RRs
        0x00, 0x00, // Authority RRs
        0x00, 0x00, // Additional RRs

// Question 1: hello.world.com (Uncompressed)
        0x05, 0x68, 0x65, 0x6c, 0x6c, 0x6f, // "hello"
        0x05, 0x77, 0x6f, 0x72, 0x6c, 0x64, // "world"
        0x03, 0x63, 0x6f, 0x6d, // "com"
        0x00, // End of domain name
        0x00, 0x01, // Type A
        0x00, 0x01, // Class IN

// Question 2: world.com (Compressed)
        0xC0, 0x0C, // Pointer to "world.com" (starts at byte 0x0C)
        0x00, 0x01, // Type A
        0x00, 0x01, // Class IN
    ];
    */
    Console.WriteLine($"Received {receivedData.Length} bytes from {sourceEndPoint}: {receivedString}");
    DnsMessage dnsMessage = new(receivedData);
    byte[] response = dnsMessage.GetResponse();
    udpClient.Send(response, response.Length, sourceEndPoint);
}

