using UnityEngine;
using WWClient.Network;

namespace MP_Client
{
    public static class MPExtensions
    {
        public static Color32 ReadColor32(ref Packet packet)
        {
            byte r = packet.ReadByte();
            byte g = packet.ReadByte();
            byte b = packet.ReadByte();
            return new Color32(r, g, b, byte.MaxValue);
        }

        public static void WriteColor32(Packet packet, Color32 color)
        {
            packet.Write(color.r);
            packet.Write(color.g);
            packet.Write(color.b);
        }
    }
}
