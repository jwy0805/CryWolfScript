using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public abstract class PacketSession : Session
{
    public static readonly int HeaderSize = 2;
    public sealed override int OnRecv(ArraySegment<byte> buffer)
    {
        int processLen = 0;
        
        while (true)
        {
            if (buffer.Count < HeaderSize) break; // 최소한 헤더는 파싱할 수 있는지
            ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset); // 패킷이 완전체로 도착했는지
            if (buffer.Count < dataSize) break;
            OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize)); // 여기까지 왔으면 패킷 조립 가능
            
            processLen += dataSize;
            buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
        }
        
        return processLen;
    }

    public abstract void OnRecvPacket(ArraySegment<byte> buffer);
}

public abstract class Session
{ 
    private Socket _socket;
    private int _disconnected = 0;

    private RecvBuffer _recvBuffer = new RecvBuffer(65535);

    private object _lock = new object();
    private Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
    List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
    private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

    public abstract void OnConnected(EndPoint endPoint);
    public abstract void OnDisconnected(EndPoint endPoint); 
    public abstract int OnRecv(ArraySegment<byte> buffer);
    public abstract void OnSend(int numOfBytes);

    private void Clear()
    {
        lock (_lock)
        {
            _sendQueue.Clear();
            _pendingList.Clear();
        }
    } 
    
    public void Start(Socket socket)
    {
        _socket = socket;
        _recvArgs.Completed += OnRecvCompleted;
        _sendArgs.Completed += OnSendCompleted;
        
        RegisterRecv();
    }

    public void Send(List<ArraySegment<byte>> sendBufferList)
    {
        if (sendBufferList.Count == 0) return;
        
        lock (_lock)
        {
            foreach (var sendBuffer in sendBufferList)    
            {
                _sendQueue.Enqueue(sendBuffer);
            }
            if (_pendingList.Count == 0) RegisterSend();
        }
    }

    public void Send(ArraySegment<byte> sendBuffer)
    {
        lock (_lock)
        {
            _sendQueue.Enqueue(sendBuffer);
            if (_pendingList.Count == 0) RegisterSend();
        }    
    }
    
    public void Disconnect() 
    {
        if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;
        OnDisconnected(_socket.RemoteEndPoint);
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        Clear();
    }
    
    #region Network Communication

    private void RegisterSend()
    {
        if (_disconnected == 1) return;
        
        while (_sendQueue.Count > 0)
        {
            ArraySegment<byte> buffer = _sendQueue.Dequeue();
            _pendingList.Add(buffer);
        }
        _sendArgs.BufferList = _pendingList;

        try
        {
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false) OnSendCompleted(null, _sendArgs);
        }
        catch (Exception e)
        {
            Debug.Log($"RegisterSend Failed : {e}");
        }
    }

    private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
    {
        lock (_lock)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    _sendArgs.BufferList = null;
                    _pendingList.Clear();
                    OnSend(_sendArgs.BytesTransferred);
                    if (_sendQueue.Count > 0) RegisterSend();
                }
                catch (Exception e)
                {
                    Debug.Log($"OnSendCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
    }
    
    private void RegisterRecv()
    {
        if (_disconnected == 1) return;
        
        _recvBuffer.Clean();
        ArraySegment<byte> segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

        try
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false) OnRecvCompleted(null, _recvArgs);
        }
        catch (Exception e)
        {
            Console.WriteLine($"RegisterRecv Failed : {e}");
        }
    }

    private void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
    {
        // BytesTransferred -> 몇 바이트를 받았는가
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            try
            {
                // Write 커서 이동
                if (_recvBuffer.OnWrite(args.BytesTransferred) == false) 
                {
                    Disconnect();
                    return;
                }
                // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다 
                int processLen = OnRecv(_recvBuffer.ReadSegment);
                if (processLen < 0 || _recvBuffer.DataSize < processLen)
                {
                    Disconnect();
                    return;
                }
                // Read 커서 이동
                if (_recvBuffer.OnRead(processLen) == false)
                {
                    Disconnect();
                    return;
                }
                RegisterRecv();
            }
            catch (Exception e)
            {
                Console.WriteLine($"OnRecvCompleted Failed {e}");
            }
        }
        else
        {
            Disconnect();
        }
    }
    
    #endregion
}