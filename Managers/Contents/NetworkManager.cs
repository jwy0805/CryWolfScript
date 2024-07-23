using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Docker.DotNet;
using Docker.DotNet.Models;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;

public class NetworkManager
{
    private readonly ServerSession _session = new();
    // private readonly int _environment = 0;    // 0 => Local, 1 => Server

    public void Send(IMessage packet)
    {
        _session.Send(packet);
    }

    public void Update()
    {
        List<PacketMessage> list = PacketQueue.Instance.PopAll();
        foreach (var packet in list)
        {
            Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
            handler?.Invoke(_session, packet.Message);
        }
    }

    public async void ConnectGameSession(bool test = false)
    {
        // DNS (Domain Name System)
        var host = Dns.GetHostName();
        var ipHost = await Dns.GetHostEntryAsync(host); 
        var ipAddress = ipHost.AddressList.FirstOrDefault(ip => ip.ToString().Contains("172."));
        if (ipAddress == null) return;
        Debug.Log(ipAddress);
        var endPointLocal = new IPEndPoint(ipAddress, 7777);
        new Connector().Connect(endPointLocal, () => _session, test);
    }
    
    public void Disconnect()
    {
        _session.Disconnect();
    }
}
