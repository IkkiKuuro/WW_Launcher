# Operacao, build e diagnostico

## Pre-requisitos

- Windows.
- Ori DE Definitive Edition instalado pela Steam.
- .NET SDK compativel com os projetos.
- Visual Studio 2022 ou VS Code com suporte a .NET.
- Caminho correto para `oriDE_Data\Managed`.

## Build

Na raiz do repositorio:

```powershell
dotnet build .\src\OriCoopPlus\OriCoopPlus.sln --configuration Release
dotnet build .\src\WWDedicatedServer\WWDedicatedServer.csproj --configuration Release
```

Saidas esperadas:

```text
src\OriCoopPlus\OriCoopClient\bin\Release\net461\ORIDEClientModule.dll
src\OriCoopPlus\OriCoopServer\bin\Release\netstandard2.0\ORIDEServerModule.dll
src\WWDedicatedServer\bin\Release\netcoreapp5.0\WWDedicatedServer.exe
```

## Instalacao

Com `<ORI_DIR>` apontando para a pasta do jogo:

```text
<ORI_DIR>\ClientModules\ORIDEClientModule.dll
<ORI_DIR>\Server\WWDedicatedServer.exe
<ORI_DIR>\Server\WWDedicatedServer.dll
<ORI_DIR>\Server\WWDedicatedServer.deps.json
<ORI_DIR>\Server\WWDedicatedServer.runtimeconfig.json
<ORI_DIR>\Server\ServerModules\ORIDEServerModule.dll
```

Nao coloque a DLL cliente em `ServerModules`. Feche `OriDE.exe` e o servidor
antes de substituir DLLs.

## Inicializacao

Interativa:

```powershell
Set-Location "<ORI_DIR>\Server"
.\WWDedicatedServer.exe
```

Pressione Enter para os padroes: quatro jogadores e porta `7777`.

Automatica:

```powershell
Set-Location "<ORI_DIR>\Server"
.\WWDedicatedServer.exe --auto
```

## Comandos do mod

| Comando | Funcao |
| --- | --- |
| `/coop` | mostra/configura `tp`, `abilities`, `story`, `world`, `doors` e `names` |
| `/tp <origem> <destino>` | teleporta a origem ate o destino; alias `/teleport` |
| `/clientcolors` | alterna cores de clientes; aliases `cc`, `clientc`, `ccolors` |
| `/entitysync` | alterna sincronizacao de entidades; aliases `es`, `sync` |
| `/dummy` | controla o bot de teste; aliases `bot`, `testbot` |
| `/fakeplayer` | alterna o jogador falso avancado; aliases `fakepl`, `fp` |

Exemplos:

```text
/coop tp on
/coop names on
/coop tp off
/tp Player_A Bot_Amigo
```

Use `/coop` sem argumentos para consultar o estado atual.

## Checklist de teste manual

1. Compile as tres saidas em Release.
2. Instale cliente, servidor e plugin a partir do mesmo build.
3. Inicie o servidor e confirme `Plugin loaded`.
4. Abra um save controlavel e pressione `F8`.
5. Conecte um cliente local; depois repita com dois clientes.
6. Teste nome, cor, posicao, desconexao e reconexao.
7. Ative uma opcao por vez e teste o efeito correspondente.
8. Teste `/dummy` e remova o bot ao terminar.

## Diagnostico rapido

| Sintoma | Verificacoes |
| --- | --- |
| Erro sobre `UnityEngine`, `WWClient` ou `Assembly-CSharp` | DLL cliente esta em `ClientModules`, nao em `ServerModules`; confirme as DLLs do jogo |
| F8 nao abre | save controlavel, DLL correta, reinicio do jogo e log de carregamento |
| Jogador sem nome | cliente/servidor da mesma versao e `/coop names on` |
| Teleporte indisponivel | `/coop tp on`, jogador destino conectado e posicao ja recebida |
| DLL nao pode ser copiada | encerre o jogo e `WWDedicatedServer.exe` |
| Servidor cheio | reduza conexoes ou inicie com maximo entre 1 e 10 |

## Itens ainda a confirmar

- comportamento de `AutoConnect` em todas as cenas;
- persistencia das opcoes do servidor entre reinicios (o codigo atual as
  redefine ao carregar o modulo);
- matriz de compatibilidade entre versoes do Ori, Unity e assemblies;
- cobertura real de sincronizacao de inimigos e entidades em partidas longas.
