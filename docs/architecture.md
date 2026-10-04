# Arquitetura

## Visao geral

O Ori Coop Plus e dividido em tres partes:

```text
OriDE.exe
  └─ ClientModules\ORIDEClientModule.dll
       └─ Harmony + MPGameManager + UI + sincronizacao

WWDedicatedServer.exe
  └─ Server\ServerModules\ORIDEServerModule.dll
       └─ comandos + configuracao + handlers do protocolo
```

O projeto `WWDedicatedServer` fornece a infraestrutura comum: servidor UDP,
clientes conectados, pacotes, eventos, carregamento dinamico de modulos e
comandos de console. O modulo Ori usa essa API sem incluir o executavel do
servidor dentro da DLL do mod.

## Projetos

| Projeto | Target | Saida | Responsabilidade |
| --- | --- | --- | --- |
| `OriCoopClient` | `.NET Framework 4.6.1` | `ORIDEClientModule.dll` | codigo executado dentro do Ori |
| `OriCoopServer` | `.NET Standard 2.0` | `ORIDEServerModule.dll` | plugin carregado pelo dedicado |
| `OriCoopShared` | arquivos compartilhados | incorporado nos dois modulos | enums, configuracao e contrato comum |
| `WWDedicatedServer` | `.NET Core 5.0` | `WWDedicatedServer.exe` | transporte UDP, ciclo de vida e console |

O cliente referencia DLLs instaladas pelo jogo em `oriDE_Data\Managed`. Esses
caminhos sao configurados atualmente no `.csproj` e precisam ser ajustados
quando a instalacao do jogo estiver em outro local.

## Ciclo de inicializacao

### Cliente

1. O loader do jogo encontra `ORIDEClientModule.dll` em `ClientModules`.
2. `ORIDEClientModule.OnEnable` carrega ou cria `MPSettings.json`.
3. O cliente registra callbacks de conexao e recebimento de pacotes.
4. Harmony aplica os patches do mod.
5. Um `MPGameManager` persistente e criado com `DontDestroyOnLoad`.
6. A conexao e a sincronizacao passam a depender do estado do save/cena.

### Servidor

1. `Program` escolhe maximo de jogadores e porta.
2. `Server.Start` abre o listener UDP e cria os slots de clientes.
3. O servidor registra os comandos de infraestrutura.
4. DLLs de `ServerModules` sao carregadas por reflexao.
5. `ORIDEServerModule.OnEnable` zera as opcoes cooperativas, registra handlers
   e registra os comandos do Ori.

## Estado e responsabilidades

- O **servidor** e a autoridade para configuracao, IDs, nomes recebidos,
  teleporte e distribuicao das mensagens.
- O **cliente** cria/atualiza representacoes remotas, aplica efeitos no mundo
  Unity e controla a interface F8.
- O **codigo compartilhado** define os identificadores que precisam ser iguais
  nos dois lados.
- A camada de jogo e acessada por assemblies externos; alteracoes nesses
  assemblies podem quebrar compilacao ou comportamento em runtime.

## Compatibilidade

Cliente e servidor devem ser distribuidos como um par. O contrato de
`PacketType`, a ordem dos campos e os recursos de configuracao precisam
permanecer compatíveis. Ao mudar um pacote, compile e teste os dois modulos.
