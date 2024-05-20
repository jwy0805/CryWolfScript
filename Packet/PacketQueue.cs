using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

public class PacketMessage
{
    public ushort Id { get; set; }
    public IMessage Message { get; set; }
}

public class PacketQueue
{
    public static PacketQueue Instance { get; } = new();

    private Queue<PacketMessage> _packetQueue = new();
    private object _lock = new();

    public void Push(ushort id, IMessage packet)
    {
        lock (_lock)
        {
            _packetQueue.Enqueue(new PacketMessage {Id = id, Message = packet});
        }
    }

    public PacketMessage Pop()
    {
        lock (_lock)
        {
            return _packetQueue.Count != 0 ? _packetQueue.Dequeue() : null;
        }
    }

    public List<PacketMessage> PopAll()
    {
        List<PacketMessage> list = new();
        lock (_lock)
        {
            while (_packetQueue.Count > 0) list.Add(_packetQueue.Dequeue());
        }

        return list;
    }
}
