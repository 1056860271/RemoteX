﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Core
{
    public enum ConnectionType { Bluetooth, UDP, TCP};
    public enum ConnectionEstablishState { Created = 0 ,Succeeded = 2, Failed = 6, Connecting = 1, Abort = 4, Disconnected = 5, Cancelled = 3}
    public delegate void MessageHandler(IConnection connection ,byte[] message);
    public delegate void ConnectionHandler(IConnection connection, ConnectionEstablishState connectionEstablishState);
    /// <summary>
    /// 这个是对所有类型连接的抽象
    /// 可以包括本地连接，TCP连接，蓝牙连接
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// 描述连接类型
        /// </summary>
        ConnectionType ConnectionType { get; }
        ConnectionEstablishState ConnectionEstablishState { get; }
        event ConnectionHandler OnConnectionEstalblishResult;
        event MessageHandler OnReceiveMessage;
        
        Task SendAsync(byte[] message);
        void Send(byte[] message);
        void Cancel();

    }

    public interface IClientConnection:IConnection
    {
        /// <summary>
        /// 建立连接（务必实现异步）
        /// 仅在连接成功时返回，否则一直阻塞
        /// </summary>
        Task<ConnectionEstablishState> ConnectAsync();
        void Connect();

        /// <summary>
        /// 若迟迟没有返回连接成功的结果，可以调用这个中止连接
        /// </summary>
        void AbortConnecting();
        
    }

    public interface IServerConnection : IConnection
    {
        
        void StartServer();

        /// <summary>
        /// 可以通过这串代码直接连接到这个Server
        /// </summary>
        string ConnectCode { get; }
    }


}
