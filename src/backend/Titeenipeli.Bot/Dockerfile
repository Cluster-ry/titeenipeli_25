FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Titeenipeli.Bot/Titeenipeli.Bot.csproj", "Titeenipeli.Bot/"]
COPY ["Titeenipeli.Common/Titeenipeli.Common.csproj", "Titeenipeli.Common/"]
RUN dotnet restore "Titeenipeli.Bot/Titeenipeli.Bot.csproj"
COPY . .
WORKDIR "/src/Titeenipeli.Bot"
RUN dotnet build "Titeenipeli.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Titeenipeli.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Titeenipeli.Bot.dll"]
