FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyHttpServer/MyHttpServer.csproj", "MyHttpServer/"]
COPY ["HttpServerLibrary/HttpServerLibrary.csproj", "HttpServerLibrary/"]
COPY ["ORMLibrary/ORMLibrary.csproj", "ORMLibrary/"]
RUN dotnet restore "MyHttpServer/MyHttpServer.csproj"
COPY . .
WORKDIR "/src/MyHttpServer"
RUN dotnet build "MyHttpServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MyHttpServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyHttpServer.dll"]
