using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Docker.DotNet;
using Docker.DotNet.Models;
using Google.Protobuf;
using UnityEngine;

public class NetworkManager
{
    private ServerSession _session = new();
    private readonly int _environment = 0;    // 0 => Local, 1 => Server

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

    public async void ConnectGameSession()
    {
        switch (_environment)
        {
            case 0:
                // DNS (Domain Name System)
                var host = Dns.GetHostName();
                var ipHost = await Dns.GetHostEntryAsync(host);
                // var ipAddress = ipHost.AddressList[0];
                // IPAddress? ipAddress = ipHost.AddressList.FirstOrDefault(ip => ip.ToString().Contains("172.30.1.20"));
                if (IPAddress.TryParse("172.24.0.2", out var ipAddress))
                {
                    Debug.Log(ipAddress);

                    var endPointLocal = new IPEndPoint(ipAddress, 7780);
                    new Connector().Connect(endPointLocal, () => _session);
                }
                break;
            case 1:
                var dockerUri = new Uri("unix:///User/jwy/.docker/run/docker.sock");
                var client = new DockerClientConfiguration(dockerUri).CreateClient();
                IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
                    new ContainersListParameters { All = true });

                foreach (var container in containers)
                {
                    if (container.Names[0].Contains("socket") == false) continue;
                    ContainerInspectResponse response = await client.Containers.InspectContainerAsync(container.ID);
                    foreach (var endPointSettings in response.NetworkSettings.Networks.Values)
                    {
                        Debug.Log($"Container ID: {container.ID}, IP Address: {endPointSettings.IPAddress}");
                        if (IPAddress.TryParse(endPointSettings.IPAddress, out var ipAddressLocalDocker))
                        {
                            var endPointLocalDocker = new IPEndPoint(ipAddressLocalDocker, 7780);
                            new Connector().Connect(endPointLocalDocker, () => _session);
                        }
                    }
                }
                
                
                break;
            case 2:
                break;
        }
    }
}
