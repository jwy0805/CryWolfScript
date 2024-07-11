using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Connector
{
    private Func<Session> _sessionFactory;

    public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
    {   // 이 시점에서 서버에 연결 시도
        for (int i = 0; i < count; i++)
        {
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectedCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;

            RegisterConnect(args);
        }
    }

    void RegisterConnect(SocketAsyncEventArgs args)
    {
        Socket socket = args.UserToken as Socket;
        if (socket == null) return;

        bool pending = socket.ConnectAsync(args);
        if (pending == false) OnConnectedCompleted(null, args);
    }

    void OnConnectedCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            Session session = _sessionFactory.Invoke();
            session.Start(args.ConnectSocket);
            session.OnConnected(args.RemoteEndPoint);
        }
        else
        {
            {
                Debug.Log($"OnConnectCompleted Fail: {args.SocketError}");
            }
        }
    }
}