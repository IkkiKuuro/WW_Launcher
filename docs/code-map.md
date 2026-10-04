# Mapa do codigo

## Ori Coop Plus

| Caminho | Papel |
| --- | --- |
| `src/OriCoopPlus/OriCoopClient/ORIDEClientModule.cs` | ponto de entrada do cliente, settings e Harmony |
| `src/OriCoopPlus/OriCoopClient/MPGameManager.cs` | estado multiplayer, callbacks e leitura de pacotes |
| `src/OriCoopPlus/OriCoopClient/OriMPPlayer.cs` | representacao de jogador remoto |
| `src/OriCoopPlus/OriCoopClient/UI/CoopHUD.cs` | HUD/painel F8 |
| `src/OriCoopPlus/OriCoopClient/UI/FloatingNameTag.cs` | nomes sobre jogadores |
| `src/OriCoopPlus/OriCoopClient/Sync/PlayerPos.cs` | dados de posicao |
| `src/OriCoopPlus/OriCoopClient/Sync/EntitySync.cs` | sincronizacao adicional de entidades |
| `src/OriCoopPlus/OriCoopClient/Sync/WorldSyncManager.cs` | portas, alavancas e eventos do mundo |
| `src/OriCoopPlus/OriCoopClient/Data/SeinPickupMPProcessor.cs` | interceptacao de pickups |
| `src/OriCoopPlus/OriCoopClient/Data/SeinSpiritMP.cs` | comportamento multiplayer do Spirit |
| `src/OriCoopPlus/OriCoopClient/Data/SeinStompMP.cs` | comportamento multiplayer do Stomp |
| `src/OriCoopPlus/OriCoopClient/Patches/` | patches Harmony sobre o jogo |
| `src/OriCoopPlus/OriCoopServer/OriCoopServerModule.cs` | ponto de entrada do plugin servidor |
| `src/OriCoopPlus/OriCoopServer/NetworkHandler.cs` | entrada e distribuicao de pacotes |
| `src/OriCoopPlus/OriCoopServer/ServerConfig.cs` | estado e broadcast de configuracao |
| `src/OriCoopPlus/OriCoopServer/Commands/` | comandos especificos do Ori |
| `src/OriCoopPlus/OriCoopServer/DummyManager.cs` | bot e eventos de teste |
| `src/OriCoopPlus/OriCoopShared/` | contrato cliente-servidor |

## Servidor comum

| Caminho | Papel |
| --- | --- |
| `src/WWDedicatedServer/WWDedicatedServer/Program.cs` | argumentos, inicializacao e carregamento de plugins |
| `src/WWDedicatedServer/WWDedicatedServer.Network/Server.cs` | listener UDP e slots |
| `src/WWDedicatedServer/WWDedicatedServer.Network/Client.cs` | estado de cada cliente |
| `src/WWDedicatedServer/WWDedicatedServer.Network/Packet.cs` | serializacao de pacotes |
| `src/WWDedicatedServer/WWDedicatedServer.CommandSystem/` | parser e registro de comandos |
| `src/WWDedicatedServer/WWDedicatedServer.API/` | API de modulos, eventos e tipos comuns |

## Fontes de catalogo

- `AllGames.txt`: jogos reconhecidos pelo launcher.
- `AllMods.txt`: identificadores e pastas de modulos.
- `WWGames.txt`: jogos com status adicional no catalogo.
- `CHANGELOGS/`: historico parcial por jogo.
