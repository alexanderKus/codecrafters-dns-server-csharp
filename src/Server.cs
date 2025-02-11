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
    IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
    byte[] receivedData = udpClient.Receive(ref sourceEndPoint);
    string receivedString = Encoding.ASCII.GetString(receivedData); 
    Console.WriteLine($"Received {receivedData.Length} bytes from {sourceEndPoint}: [{receivedString}]");
    DnsMessage dnsMessage = new(receivedData, resolverUdpEndPoint);
    byte[] response = dnsMessage.GetResponse();
    udpClient.Send(response, response.Length, sourceEndPoint);
}

