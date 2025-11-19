FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["IcutechTestApi.csproj", "./"]
RUN dotnet restore "IcutechTestApi.csproj"

COPY . .
WORKDIR "/src"
RUN dotnet build "IcutechTestApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IcutechTestApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IcutechTestApi.dll"]

