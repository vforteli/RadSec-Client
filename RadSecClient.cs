using Flexinets.Radius.Core;
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Flexinets.Radius
{
    public class RadSecClient
    {
        private readonly RadiusDictionary _dictionary;
        private readonly TcpClient _client;
        private readonly X509CertificateCollection _certs = new X509CertificateCollection();
        private readonly Boolean _trustServerCertificate;


        /// <summary>
        /// Create a radius client which sends and receives responses on localEndpoint
        /// </summary>
        /// <param name="localEndpoint"></param>
        /// <param name="clientCertificate"></param>
        /// <param name="trustServerCertificate">If set to true, server certificate will not be validated. This can be useful for testing with self signed certificates</param>
        public RadSecClient(RadiusDictionary dictionary, X509Certificate clientCertificate, Boolean trustServerCertificate = false)
        {
            _dictionary = dictionary;
            _client = new TcpClient();
            _certs.Add(clientCertificate);
            _trustServerCertificate = trustServerCertificate;
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
            await sslStream.AuthenticateAsClientAsync("radsecserver", _certs, SslProtocols.Tls12, true);
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


        /// <summary>
        /// Validate server certificate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private Boolean ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return _trustServerCertificate || sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}
