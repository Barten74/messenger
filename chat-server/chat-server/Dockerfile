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
ENTRYPOINT ["dotnet", "chat-server.dll"]

WORKDIR /app
ADD bin/Debug/netcoreapp2.0/Swagger.xml Swagger.xml
ADD Views/Api/* Views/Api/
