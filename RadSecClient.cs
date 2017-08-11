using Flexinets.Radius.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            var stream = _client.GetStream();
            var packetBytes = packet.GetBytes(_dictionary);
            await stream.WriteAsync(packetBytes, 0, packetBytes.Length);

            if (RadiusPacket.TryParsePacketFromStream(stream, out var responsePacket, _dictionary, packet.SharedSecret))
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
    }
}
