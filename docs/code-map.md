# Mapa do codigo

## Ori Coop Plus

| Caminho | Papel |
| --- | --- |
| `src/OriCoopPlus/OriCoopBepInEx/Plugin/OriCoopPlugin.cs` | ponto de entrada BepInEx, configuracao e ciclo de vida |
| `src/OriCoopPlus/OriCoopBepInEx/Domain/` | DTOs e contratos sem dependencia de Unity |
| `src/OriCoopPlus/OriCoopBepInEx/Networking/NetworkService.cs` | adaptador para o protocolo UDP legado |
| `src/OriCoopPlus/OriCoopBepInEx/Patches/` | gatilhos Harmony e leitura do estado de Sein |
| `src/OriCoopPlus/OriCoopServer/OriCoopServerModule.cs` | ponto de entrada do plugin servidor |
| `src/OriCoopPlus/OriCoopServer/NetworkHandler.cs` | entrada e distribuicao de pacotes |
| `src/OriCoopPlus/OriCoopServer/ServerConfig.cs` | estado e broadcast de configuracao |
| `src/OriCoopPlus/OriCoopServer/Commands/` | comandos especificos do Ori |
| `src/OriCoopPlus/OriCoopServer/DummyManager.cs` | bot e eventos de teste |
| `src/OriCoopPlus/OriCoopShared/` | contrato cliente-servidor |

## Servidor dedicado próprio

| Caminho | Papel |
| --- | --- |
| `src/OriCoopDedicatedServer/OriCoopDedicatedServer/Program.cs` | argumentos e ciclo de vida do servidor |
| `src/OriCoopDedicatedServer/OriCoopDedicatedServer.Core/Network/Server.cs` | listener UDP e slots |
| `src/OriCoopDedicatedServer/OriCoopDedicatedServer.Core/Network/Client.cs` | estado de cada cliente |
| `src/OriCoopDedicatedServer/OriCoopDedicatedServer.Core/Network/Packet.cs` | serializacao de pacotes |
| `src/OriCoopDedicatedServer/OriCoopDedicatedServer.Core/CommandSystem/` | parser e registro de comandos |
| `src/OriCoopDedicatedServer/OriCoopDedicatedServer.Core/API/` | API própria, eventos e tipos comuns |
| `src/OriCoopDedicatedServer/OriCoopDedicatedServer/Game/` | regras, comandos e handlers do Ori compilados no servidor |

## Fontes de catalogo

- `AllGames.txt`: jogos reconhecidos pelo launcher.
- `AllMods.txt`: identificadores e pastas de modulos.
- `WWGames.txt`: jogos com status adicional no catalogo.
- `CHANGELOGS/`: historico parcial por jogo.
