using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using WWDedicatedServer.API;

namespace WWDedicatedServer.Network;

public static class Server
{
	public static bool StopTheServer = false;

	public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();

	private static UdpClient _udpListener;

	public static int LatesNetId = 0;

	public static bool EnableFakePackets = false;

	public static Dictionary<string, string> NetworkVars = new Dictionary<string, string>();

	public static bool IgnoreIpCheck = false;

	public static int MaxPlayers { get; private set; }

	public static int Port { get; private set; }

	public static void Start(int _maxplayers, int _port)
	{
		MaxPlayers = _maxplayers;
		Port = _port;
		Logger.Info("SERVER", "Starting server...");
		InitializeServerData();
		_udpListener = new UdpClient(Port);
		_udpListener.BeginReceive(UDPReciveCallback, null);
		Logger.Info("SERVER", $"Server started on {Port} maxplayers: {MaxPlayers}");
		if (ServerEvents.OnServerStarted != null)
		{
			ServerEvents.OnServerStarted();
		}
	}

	private static void UDPReciveCallback(IAsyncResult ar)
	{
		try
		{
			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
			byte[] array = _udpListener.EndReceive(ar, ref remoteEP);
			_udpListener.BeginReceive(UDPReciveCallback, null);
			Packet packet = new Packet(array);
			int num = packet.ReadInt();
			if (num <= -1)
			{
				Logger.Info("SERVER", $"Incoming connection from {remoteEP}...");
				if (!AddClient(remoteEP))
				{
					Logger.Warning("SERVER", $"{remoteEP} failed to connect: Server Full!");
				}
			}
			else if (array.Length > 4)
			{
				Client.UDP udp = Clients[num].udp;
				if (udp.endPoint.ToString() == remoteEP.ToString())
				{
					udp.HandleData(packet, num);
				}
			}
		}
		catch (Exception arg)
		{
			Logger.Error("SERVER", $"RECIVE UDP CALLBACK ERROR: {arg}");
		}
	}

	private static bool AddClient(IPEndPoint clientEndPoint)
	{
		for (int i = 0; i < Clients.Count; i++)
		{
			Client client = Clients[i];
			if (!IgnoreIpCheck && client.ClientAddress == clientEndPoint.Address.ToString())
			{
				client.udp.Connect(clientEndPoint);
				return true;
			}
			if (!client.IsReady)
			{
				client.udp.Connect(clientEndPoint);
				return true;
			}
		}
		return false;
	}

	public static void SendUDPData(IPEndPoint _clientEndPoint, Packet packet)
	{
		try
		{
			if (_clientEndPoint != null)
			{
				_udpListener.BeginSend(packet.ToArray(), packet.Length(), _clientEndPoint, null, null);
			}
		}
		catch (Exception arg)
		{
			Logger.Error("SERVER", $"SEND UDP DATA ERROR: {arg}");
		}
	}

	private static void InitializeServerData()
	{
		for (int i = 0; i <= MaxPlayers; i++)
		{
			Clients.Add(i, new Client(i));
		}
	}
}
