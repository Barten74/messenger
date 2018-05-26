# Server - 4000 port
# Swagger available at http://localhost:4000/swagger/
cd chat-server/chat-server
dotnet build
# Unix:
docker build -t chat-server . #sudo on unix!
# Windows:
#docker run -it --rm --name chat-server_instance chat-server
docker run -it --rm -p 0.0.0.0:4000:80 chat-server

# Client - 80 port
cd ../..
cd chat-client/chat-client
# Unix:
sudo docker build -t chat-client .
# Windows:
#docker run -it --rm --name chat-client_instance chat-client
docker run -it --rm -p 0.0.0.0:80:80 chat-client