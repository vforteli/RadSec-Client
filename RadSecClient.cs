using Flexinets.Radius.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Flexinets.Radius
{
    public class RadSecClient
    {
        private readonly RadiusDictionary _dictionary;
        private readonly TcpClient _client;


        /// <summary>
        /// Create a radius client which sends and receives responses on localEndpoint
        /// </summary>
        /// <param name="localEndpoint"></param>
        /// <param name="dictionary"></param>
        public RadSecClient(RadiusDictionary dictionary)
        {
            _dictionary = dictionary;
            _client = new TcpClient();
        }


        /// <summary>
        /// Send a packet with specified timeout
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="remoteEndpoint"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<IRadiusPacket> SendPacketAsync(IRadiusPacket packet, IPEndPoint remoteEndpoint, TimeSpan timeout)
        {
            await _client.ConnectAsync(remoteEndpoint.Address, remoteEndpoint.Port);
            var sslStream = new SslStream(_client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate));
            await sslStream.AuthenticateAsClientAsync("radsecserver");
            var packetBytes = packet.GetBytes(_dictionary);
            await sslStream.WriteAsync(packetBytes, 0, packetBytes.Length);

            if (RadiusPacket.TryParsePacketFromStream(sslStream, out var responsePacket, _dictionary, packet.SharedSecret))
            {
                _client.Close();
                return responsePacket;
            }

            _client.Close();
            return null;
        }


        /// <summary>
        /// Send a packet with default timeout of 3 seconds
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="remoteEndpoint"></param>
        /// <returns></returns>
        public async Task<IRadiusPacket> SendPacketAsync(IRadiusPacket packet, IPEndPoint remoteEndpoint)
        {
            return await SendPacketAsync(packet, remoteEndpoint, TimeSpan.FromSeconds(3));
        }


        public static Boolean ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;    // todo much secure, such hack            
        }
    }
}
