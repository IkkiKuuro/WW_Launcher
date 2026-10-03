using System.Collections.Generic;
using WWDedicatedServer.CommandSystem;

namespace ORIDEServerModule.Commands
{
    public class AllowCustomColorsCmd : ConsoleCommand
    {
        public string Command => "clientcolors";
        public string[] Aliases => new string[3] { "cc", "clientc", "ccolors" };
        public string Description => "Enable or Disable client colors";

        public bool Execute(List<string> arguments, out string response)
        {
            ServerConfig.ClientColors = !ServerConfig.ClientColors;
            response = $"SET USE CLIENT COLORS TO: {ServerConfig.ClientColors}";
            return true;
        }
    }
}
