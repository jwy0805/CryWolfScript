using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;

public class NetworkManager
{
    private ServerSession _session = new();
    private int _sessionId;
    private const string LocalPort = "7270";
    private const string Address = "hamonstudio.net";
    
    public Env Environment => Env.Local;
    public bool IsFriendlyMatchHost { get; set; }

    public int SessionId
    {
        get => _sessionId;
        set
        {
            _sessionId = value;

            if (_sessionId == -1) return;
            
            var uiArray = GameObject.FindGameObjectsWithTag("UI");

            foreach (var ui in uiArray)
            {
                if (ui.TryGetComponent(out UI_TutorialMainPopup uiTutorialMainPopup))
                {
                    uiTutorialMainPopup.StartTutorial(_sessionId);
                    return;
                }
            }
            
            if (GameObject.FindWithTag("UI").TryGetComponent(out UI_MatchMaking uiMatchMaking))
            {
                uiMatchMaking.StartMatchMaking(_sessionId);
                return;
            }
            
            if (GameObject.FindWithTag("UI").TryGetComponent(out UI_SinglePlay uiSinglePlay))
            {
                uiSinglePlay.StartSinglePlay(_sessionId);
                return;
            }
        }
    }
    
    public string BaseUrl
    {
        get
        {
            return Managers.Network.Environment switch
            {
                Env.Dev => $"https://{Address}",
                _ => $"https://localhost:{LocalPort}"
            };
        }
    }
    
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

    public async Task ConnectGameSession(bool test = false)
    {
        // DNS (Domain Name System)
        string host;
        int port;
        IPHostEntry ipHost;
        IPAddress ipAddress;
        
        switch (Environment)
        {
            case Env.Local:
                host = Dns.GetHostName();
                port = 7777;
                ipHost = await Dns.GetHostEntryAsync(host);
                ipAddress = ipHost.AddressList.FirstOrDefault(ip => ip.ToString().Contains("172."));
                break;
            
            case Env.Dev:
                host = "hamonstudio.net";
                port = 7780;
                ipHost = await Dns.GetHostEntryAsync(host);
                ipAddress = ipHost.AddressList.FirstOrDefault();
                break;
            
            case Env.Stage:
            case Env.Prod:
            default:
                return;
        }
        
        if (ipAddress == null) return;
        Debug.Log(ipAddress);
        var endPointLocal = new IPEndPoint(ipAddress, port);
        _session = new ServerSession();
        new Connector().Connect(endPointLocal, () => _session, test);
    }
    
    // Disconnect TCP Connection
    public void Disconnect()
    {
        _session.Disconnect();
        _sessionId = -1;
    }
}
