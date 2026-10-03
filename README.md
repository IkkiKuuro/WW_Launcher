# WW Launcher

Um sistema para criar multiplayer em jogos single-player feitos com Unity.

Nem todos os jogos do launcher estao funcionando corretamente no momento.

Discord: https://discord.gg/F2sHzGC3kb

Download: https://github.com/zheka-100500/WW_Launcher/raw/main/Launcher.zip

## Ori Coop Plus

O Ori Coop Plus adiciona multiplayer cooperativo ao **Ori and the Blind Forest:
Definitive Edition**. O mod possui um modulo cliente, carregado pelo jogo, e um
modulo servidor, carregado pelo servidor dedicado. Eles devem ser colocados em
pastas diferentes.

## Requisitos

- Ori and the Blind Forest: Definitive Edition instalado pela Steam.
- Windows.
- .NET SDK para compilar o projeto.
- Visual Studio 2022 ou VS Code com suporte a projetos .NET.
- Acesso a uma pasta do jogo semelhante a:

  ```text
  D:\SteamLibrary\steamapps\common\Ori DE\
  ```

Os caminhos podem ser diferentes conforme a instalacao da Steam.

## 1. Compilar o mod

1. Feche o jogo e qualquer servidor dedicado em execucao.
2. Abra a solucao:

   ```text
   src\OriCoopPlus\OriCoopPlus.sln
   ```

3. Selecione a configuracao **Release**.
4. Use **Build > Build Solution** ou pressione `Ctrl+Shift+B`.
5. Confirme que a compilacao terminou sem erros.

Tambem e possivel compilar pelo PowerShell na raiz do repositorio:

```powershell
dotnet build .\src\OriCoopPlus\OriCoopPlus.sln --configuration Release
dotnet build .\src\WWDedicatedServer\WWDedicatedServer.csproj --configuration Release
```

Os arquivos gerados ficam em:

```text
src\OriCoopPlus\OriCoopClient\bin\Release\net461\ORIDEClientModule.dll
src\OriCoopPlus\OriCoopServer\bin\Release\netstandard2.0\ORIDEServerModule.dll
src\WWDedicatedServer\bin\Release\netcoreapp5.0\WWDedicatedServer.exe
```

## 2. Instalar os modulos

Considere `<ORI_DIR>` como a pasta de instalacao do jogo. Copie os arquivos
para estes destinos:

```text
ORIDEClientModule.dll
  de: src\OriCoopPlus\OriCoopClient\bin\Release\net461\
  para: <ORI_DIR>\ClientModules\

ORIDEServerModule.dll
  de: src\OriCoopPlus\OriCoopServer\bin\Release\netstandard2.0\
  para: <ORI_DIR>\Server\ServerModules\
```

O modulo cliente **nao** deve ser colocado em `ServerModules`. Essa pasta deve
conter somente o modulo do servidor:

```text
<ORI_DIR>\Server\ServerModules\ORIDEServerModule.dll
```

Copie tambem o servidor dedicado compilado para `<ORI_DIR>\Server\`. Para
manter os arquivos correspondentes entre si, copie estes arquivos da pasta
`src\WWDedicatedServer\bin\Release\netcoreapp5.0\`:

```text
WWDedicatedServer.exe
WWDedicatedServer.dll
WWDedicatedServer.deps.json
WWDedicatedServer.runtimeconfig.json
WWDedicatedServer.pdb
```

Antes de substituir uma DLL, feche o jogo e o servidor. O Windows nao permite
substituir uma DLL que ainda esteja sendo usada pelo processo do jogo.

A estrutura final esperada e:

```text
<ORI_DIR>\
├── ClientModules\
│   └── ORIDEClientModule.dll
└── Server\
    ├── WWDedicatedServer.exe
    ├── WWDedicatedServer.dll
    ├── WWDedicatedServer.deps.json
    ├── WWDedicatedServer.runtimeconfig.json
    └── ServerModules\
        └── ORIDEServerModule.dll
```

## 3. Abrir o servidor

1. Abra:

   ```text
   <ORI_DIR>\Server\
   ```

2. Execute `WWDedicatedServer.exe`.
3. Quando for solicitado o numero maximo de jogadores, digite um valor de `1`
   a `10`, ou pressione `Enter` para usar `4`.
4. Quando for solicitada a porta, digite `7777`, ou pressione `Enter` para
   usar a porta padrao.
5. Aguarde mensagens semelhantes a:

   ```text
   Server started on 7777 maxplayers: 4
   Plugin loaded: Ori and the Blind Forest: DE - Coop Plus Server
   ```

6. Mantenha a janela do servidor aberta enquanto estiver jogando.

Para iniciar automaticamente com 4 jogadores e porta 7777:

```powershell
Set-Location "<ORI_DIR>\Server"
.\WWDedicatedServer.exe --auto
```

Se o arquivo `Iniciar_Servidor.bat` nao estiver configurado para a sua
instalacao, execute o `WWDedicatedServer.exe` diretamente usando os passos
acima.

## 4. Entrar pelo jogo

1. Inicie o jogo normalmente.
2. Carregue um save em que o Ori ja esteja controlavel. O multiplayer nao pode
   ser testado no menu ou durante o prologo com a Naru.
3. O modulo cliente deve carregar automaticamente a partir de `ClientModules`.
4. Pressione `F8` para abrir o painel **Ori Coop Plus**.
5. Informe o IP e a porta do servidor. Para um servidor no mesmo computador,
   use:

   ```text
   IP: 127.0.0.1
   Porta: 7777
   ```

6. Clique em **Conectar ao Servidor**.
7. Para testar com outro computador, use o IP do computador que esta
   executando o servidor e libere a porta `7777` no firewall.

No painel F8 e possivel salvar o nick, conectar ou desconectar e ver os
jogadores conectados. Quando o servidor permitir teleporte, cada jogador tera
um botao **Teleportar ate <nome>**.

O atalho `T` alterna o teleporte para o proximo jogador disponivel.

Por seguranca, todas as opcoes cooperativas iniciam desligadas a cada
inicializacao do servidor. Para usar um recurso, ative-o explicitamente pelos
comandos do servidor.

## 5. Comandos do servidor

Os comandos sao digitados na janela do `WWDedicatedServer.exe`.

```text
/coop
/coop tp on
/coop tp off
/coop names on
/coop names off
/tp <jogador-origem> <jogador-destino>
/clientcolors
/entitysync
/dummy
/fakeplayer
```

Use `/coop` sem argumentos para ver o estado das configuracoes. O comando
`/dummy` ativa ou desativa o bot de teste `Bot_Amigo`. O bot deve aparecer no
painel F8 com o nome recebido do servidor.

O comando `/clientcolors` alterna as cores personalizadas dos clientes e
`/entitysync` alterna a sincronizacao de entidades. Ambos iniciam desligados.

### Teleporte do host

O host pode teleportar um jogador ate outro usando o nick exato:

```text
/tp NickOrigem NickDestino
```

Por exemplo, para teleportar `Player_A` ate `Bot_Amigo`:

```text
/tp Player_A Bot_Amigo
```

Tambem e possivel usar o ID numerico do jogador:

```text
/tp 1 Bot_Amigo
```

O jogador de origem precisa estar conectado e o servidor precisa ter recebido
ao menos uma atualizacao da posicao do jogador de destino. O comando tambem
pode ser escrito como `/teleport`.

## Solucao de problemas

### O servidor reclama de `UnityEngine`, `WWClient` ou `Assembly-CSharp`

`ORIDEClientModule.dll` foi colocado na pasta errada. Remova-o de:

```text
<ORI_DIR>\Server\ServerModules\
```

Coloque-o em:

```text
<ORI_DIR>\ClientModules\
```

Em `ServerModules` deixe apenas `ORIDEServerModule.dll`.

### O F8 nao abre

- Feche o jogo e substitua novamente `ORIDEClientModule.dll`.
- Confirme que a DLL esta em `<ORI_DIR>\ClientModules\`.
- Reinicie o jogo depois da substituicao.
- Teste dentro de um save em que o Ori esteja controlavel.
- Verifique se o modulo cliente aparece no log de carregamento do jogo.

### O jogador aparece sem nome

- Confirme que o servidor e o `ORIDEServerModule.dll` foram atualizados juntos.
- Use `/coop names on` no servidor.
- Reconecte os jogadores depois de alterar a configuracao.

### O teleporte esta desativado

Execute no servidor:

```text
/coop tp on
```

Depois reconecte o cliente ou aguarde a sincronizacao da configuracao e abra o
F8 novamente.

### O Windows nao deixa copiar a DLL

O jogo ou o servidor ainda esta usando o arquivo. Feche `OriDE.exe` e
`WWDedicatedServer.exe` pelo Gerenciador de Tarefas e tente copiar novamente.
