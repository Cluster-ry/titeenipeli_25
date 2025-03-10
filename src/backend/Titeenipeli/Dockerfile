FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/Titeenipeli/Titeenipeli.csproj", "backend/Titeenipeli/"]
COPY ["protobuf/", "protobuf/"]
RUN dotnet restore "backend/Titeenipeli/Titeenipeli.csproj"
COPY . .
WORKDIR "/src/backend/Titeenipeli"
RUN dotnet build "Titeenipeli.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Titeenipeli.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=pyroscope/pyroscope-dotnet:0.9.2-glibc /Pyroscope.Profiler.Native.so /dotnet/Pyroscope.Profiler.Native.so
COPY --from=pyroscope/pyroscope-dotnet:0.9.2-glibc /Pyroscope.Linux.ApiWrapper.x64.so /dotnet/Pyroscope.Linux.ApiWrapper.x64.so

ENTRYPOINT ["dotnet", "Titeenipeli.dll"]
