FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Irrelephant.Outcast.Server/Irrelephant.Outcast.Server.csproj", "Irrelephant.Outcast.Server/"]
RUN dotnet restore "Irrelephant.Outcast.Server/Irrelephant.Outcast.Server.csproj"
COPY . .
WORKDIR "/src/Irrelephant.Outcast.Server"
RUN dotnet build "Irrelephant.Outcast.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Irrelephant.Outcast.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Irrelephant.Outcast.Server.dll"]
