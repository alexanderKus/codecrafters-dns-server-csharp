using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_dns_server;

Console.WriteLine("Logs from your program will appear here!");

IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
int port = 2053;
IPEndPoint udpEndPoint = new IPEndPoint(ipAddress, port);
UdpClient udpClient = new UdpClient(udpEndPoint);
IPEndPoint? resolverUdpEndPoint = null!;

if (args is ["--resolver", _])
{
    var resolverAddressParts = args[1].Split(':');
    var resolverIpAddress = IPAddress.Parse(resolverAddressParts[0]);
    var resolverPort = int.Parse(resolverAddressParts[1]);
    Console.WriteLine($"Got resolver. ip: {resolverIpAddress}, port: {resolverPort}");
    resolverUdpEndPoint = new IPEndPoint(resolverIpAddress, resolverPort);
}

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
byte[] test =
[
    0x12, 0x34, // Transaction ID (example)
    0x01, 0x00, // Flags (Standard query)
    0x00, 0x02, // Questions (2)
    0x00, 0x00, // Answer RRs
    0x00, 0x00, // Authority RRs
    0x00, 0x00, // Additional RRs

// Question 1: abc.longassdomainname.com (Uncompressed)
    0x03, 0x61, 0x62, 0x63, // "abc"
    0x06, 0x6c, 0x6f, 0x6e, 0x67, 0x61, 0x61,// "longass"
    0x0a, 0x64, 0x6f, 0x6d, 0x61, 0x69, 0x6e, 0x6e, 0x61, 0x6d, 0x65, // "domainname"
    0x03, 0x63, 0x6f, 0x6d, // "com"
    0x00, // End of domain name
    0x00, 0x01, // Type A (IPv4 address)
    0x00, 0x01, // Class IN

// Question 2: abc.longassdomainname.com (Uncompressed)
    0x03, 0x61, 0x62, 0x63, // "abc"
    0x06, 0x6c, 0x6f, 0x6e, 0x67, 0x61, 0x61,// "longass"
    0x0a, 0x64, 0x6f, 0x6d, 0x61, 0x69, 0x6e, 0x6e, 0x61, 0x6d, 0x65, // "domainname"
    0x03, 0x63, 0x6f, 0x6d, // "com"
    0x00, // End of domain name
    0x00, 0x01, // Type A (IPv4 address)
    0x00, 0x01, // Class IN
];
    byte[] test =
    [
        0x15,
        0x54,
        0x01,
        0x00,
        0x00,
        0x02,
        0x00,
        0x00,
        0x00,
        0x00,
        0x00,
        0x00,
        0x03,
        0x61,
        0x62,
        0x63,
        0x11,
        0x6C,
        0x6F,
        0x6E,
        0x67,
        0x61,
        0x73,
        0x73,
        0x64,
        0x6F,
        0x6D,
        0x61,
        0x69,
        0x6E,
        0x6E,
        0x61,
        0x6D,
        0x65,
        0x03,
        0x63,
        0x6F,
        0x6D,
        0x00,
        0x00,
        0x01,
        0x00,
        0x01,
        0x03,
        0x64,
        0x65,
        0x66,
        0xC0,
        0x10,
        0x00,
        0x01,
        0x00,
        0x01
    ];
    */

    Console.WriteLine("*******************************");
    foreach (byte b in receivedData)
    {
        Console.Write($"{b:X2},");
    }
    Console.WriteLine("\n*******************************");
    Console.WriteLine($"Received {receivedData.Length} bytes from {sourceEndPoint}: [{receivedString}]");
    DnsMessage dnsMessage = new(receivedData, resolverUdpEndPoint);
    // DnsMessage dnsMessage = new(test);
    byte[] response = dnsMessage.GetResponse();
    udpClient.Send(response, response.Length, sourceEndPoint);
}

