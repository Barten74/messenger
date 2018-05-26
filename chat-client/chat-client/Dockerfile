FROM microsoft/aspnetcore-build:2.0 AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

#ADD bin/Debug/netcoreapp2.0/Swagger.xml Swagger.xml
# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# build runtime image
FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY --from=build-env /app/out .
# EXPOSE 4000
ENTRYPOINT ["dotnet", "chat-client.dll"]

WORKDIR /app
COPY wwwroot/Index.html wwwroot/Index.html
COPY wwwroot/chat/Index.html wwwroot/chat/Index.html
COPY wwwroot/js/site.js wwwroot/js/site.js