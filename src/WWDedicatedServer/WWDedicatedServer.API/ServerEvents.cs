using System;
using WWDedicatedServer.Network;

namespace WWDedicatedServer.API;

public static class ServerEvents
{
	public static Action<Client> OnClientConnected;

	public static Action<int, Client, Packet> OnPacketRecived;

	public static Action OnServerStarted;
}
