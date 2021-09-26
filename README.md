<h1 align="center">Desafio Técnico TakeBlip</h1>

# Sobre o projeto
> O desafio proposto foi construir um servidor e cliente de bate papo utilizando-se um ou mais protocolos de rede como TCP, Websockets ou HTTP e que implemente as seguintes funcionalidades básicas: registro de apelido, envio de mensagem pública para a sala e Envio de mensagem pública para um usuário.
> Utilizando-se da plataforma .Net Core 5.0 (C#, Console Application), foram implementados dois serviços (Cliente e Servidor) que podem ser executados conforme orientações abaixo.

# Executando o projeto
```bash
# Clone este repositório
$ git clone https://github.com/rafaelpereirasantiago/chat.git

# Entre no diretório "chat"
$ cd chat

# Entre no diretório "Server"
$ cd Server

# Inicialize o servidor
$ dotnet run

* Por padrão o servidor utilizará a porta 8002 para escutar conexões TCP, 8003 para conexões HTTP e 8004 para conexões WebSockets.

# Entre no diretório "Client"
$ cd ../Client

# Inicialize o cliente TCP
$ dotnet run -- -protocol=tcp -callbackPort=8005
* Este client utilizará a porta 8005 para escutar mensagens de broadcast do servidor

# Inicialize o cliente HTTP
$ dotnet run -- -protocol=http -callbackPort=8006
* Este client utilizará a porta 8006 para escutar mensagens de broadcast do servidor

# Inicialize o cliente WebSocket
$ dotnet run -- -protocol=websocket -callbackPort=8007
* Este client utilizará a porta 8007 para escutar mensagens de broadcast do servidor
```

# Executando testes
```bash
# Entre no diretório "ClientTests"
$ cd ClientTests

# Execut os testes do cliente
$ dotnet test

# Entre no diretório "ServerTests"
$ cd ServerTests

# Execut os testes do servidor
$ dotnet test
```

# Parâmetros da aplicação

## Parametros para execução do servidor
* Define a porta para executar conexões TCP: -tcpPort=[porta]
* Define a porta para executar conexões HTTP: -httpPort=[porta]
* Define a porta para executar conexões WebSockets: -websocketPort=[porta]

```bash
# Exemplo de execução:
dotnet run -- -tcpPort=8002 -httpPort=8003 -websocketPort=8004
```

## Parametros para execução do cliente
Define o tipo de conexão a ser realizada com o servidor: -protocol=[tcp,http,websocket]
Define o host do servidor: -server=[host]
Define a porta utilizada para escutar mensagens de broadcast do servidor: -callbackPort=[port]

```bash
# Exemplo de execução:
dotnet run -- -protocol=tcp -server=127.0.0.1 -callbackPort=8005
```