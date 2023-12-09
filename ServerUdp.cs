using System.Net;
using System.Net.Sockets;

namespace TucanNet;

public class ServerUdp
{
    private readonly List<IPEndPoint> _clients;
    private readonly UdpClient _udpClient;
    private bool _isRunning;
    
    public ServerUdp(int port, int clientCount = int.MaxValue, bool waitForClients = false)
    {
        _clients = new List<IPEndPoint>();
        _udpClient = new UdpClient(port);
        
        var clientsThread = new Thread(() =>
        {
            HandleClient(clientCount, waitForClients);
        });
        clientsThread.Start();
    }
    
    public Action<Packet>? ReceivePacket { get; set; }
    
    public Action<IPEndPoint>? ClientConnect { get; set; }
    
    public Action<IPEndPoint>? ClientDisconnect { get; set; }
    
    public void Stop()
    {
        _isRunning = false;
        _udpClient.Close();
    }
    
    private void HandleClient(int clientCount, bool waitForClients)
    {
        _isRunning = true;
        while (_isRunning)
        {
            var clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            
            byte[] receiveBuffer;
            
            try
            {
                receiveBuffer = _udpClient.Receive(ref clientEndPoint);
            }
            catch
            {
                continue;
            }

            if (receiveBuffer.Length > 0)
            {
                using var packet = new Packet();
                packet.WriteBytes(receiveBuffer);
                ReceivePacket?.Invoke(packet);
            }

            if (!_clients.Contains(clientEndPoint))
            {
                if (_clients.Count >= clientCount)
                {
                    continue;
                }
                
                ClientConnect?.Invoke(clientEndPoint);
                _clients.Add(clientEndPoint);
                continue;
            }

            if (waitForClients && _clients.Count < clientCount)
            {
                continue;
            }

            foreach (var client in _clients)
            {
                if (client.Equals(clientEndPoint))
                {
                    continue;
                }

                try
                {
                    _udpClient.Send(receiveBuffer, receiveBuffer.Length, client);
                }
                catch
                {
                    ClientDisconnect?.Invoke(client);
                    _clients.Remove(client);
                }
            }
        }
        _udpClient.Close();
    }
}