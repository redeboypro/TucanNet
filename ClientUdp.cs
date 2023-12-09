using System.Net;
using System.Net.Sockets;

namespace TucanNet;

public class ClientUdp
{
    private readonly Packet _packet;
    private readonly UdpClient _udpClient;
    private readonly IPEndPoint _serverEndPoint;

    private bool _isConnected;

    public ClientUdp(string address, int port)
    {
        _packet = new Packet();
        _udpClient = new UdpClient();
        _serverEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
        _isConnected = true;

        var localEndpoint = new IPEndPoint(IPAddress.Any, 0);
        _udpClient.Client.Bind(localEndpoint);
        
        Send();

        var receiveThread = new Thread(ReceivePackets);
        receiveThread.Start();
    }

    public Action<Packet>? ReceivePacket { get; set; }

    public UdpClient GetUdpClient()
    {
        return _udpClient;
    }
    
    public IPEndPoint GetServerEndPoint()
    {
        return _serverEndPoint;
    }

    public int GetBufferSize()
    {
        return _packet.BufferSize;
    }

    public void WriteBytesToBuffer(IEnumerable<byte> data)
    {
        _packet.WriteBytes(data);
    }

    public void WriteInt16ToBuffer(short data)
    {
        _packet.WriteInt16(data);
    }

    public void WriteInt32ToBuffer(int data)
    {
        _packet.WriteInt32(data);
    }

    public void WriteInt64ToBuffer(long data)
    {
        _packet.WriteInt64(data);
    }

    public void WriteSingleToBuffer(float data)
    {
        _packet.WriteSingle(data);
    }

    public void WriteStringToBuffer(string? data)
    {
        _packet.WriteString(data);
    }

    public void WriteInt16ArrayToBuffer(params short[] data)
    {
        _packet.WriteInt16Array(data);
    }

    public void WriteInt32ArrayToBuffer(params int[] data)
    {
        _packet.WriteInt32Array(data);
    }

    public void WriteInt64ArrayToBuffer(params long[] data)
    {
        _packet.WriteInt64Array(data);
    }

    public void WriteSingleArrayToBuffer(params float[] data)
    {
        _packet.WriteSingleArray(data);
    }

    public void WriteStringArrayToBuffer(params string[] data)
    {
        _packet.WriteStringArray(data);
    }
    
    public void ClearBuffer()
    {
        _packet.Clear();
    }

    public void Send()
    {
        var buffer = _packet.ToArray();
        _udpClient.Send(buffer, buffer.Length, _serverEndPoint);
    }

    public void Disconnect()
    {
        _isConnected = false;
        _udpClient.Close();
    }

    private void ReceivePackets()
    {
        while (_isConnected)
        {
            var serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
            var receiveBytes = _udpClient.Receive(ref serverEndpoint);
            
            using var packet = new Packet();
            packet.WriteBytes(receiveBytes);
            
            ReceivePacket?.Invoke(packet);
        }
    }
}