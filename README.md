# RadSecClient
Eventually a working RadSec Radius client for testing purposes

# Usage

```
var dictionary = new RadiusDictionary(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\radius.dictionary");
var cert = new X509Certificate("clientCertificatePath", "certificatePassword");
var client = new RadSecClient(dictionary, cert);
var packet = new RadiusPacket(PacketCode.AccessRequest, 0, "xyzzy5461");
packet.AddAttribute("User-Name", "nemo");
packet.AddAttribute("User-Password", "arctangent");
packet.AddAttribute("NAS-IP-Address", IPAddress.Parse("192.168.1.16"));
packet.AddAttribute("NAS-Port", 3);
var responsePacket = await client.SendPacketAsync(packet, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2083));
Console.WriteLine(responsePacket.Code);
```
