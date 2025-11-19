# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["IcutechTestApi.csproj", "./"]
RUN dotnet restore "IcutechTestApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "IcutechTestApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "IcutechTestApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Render automatically sets PORT environment variable
# ASP.NET Core will use it automatically, but we can also set ASPNETCORE_URLS explicitly
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IcutechTestApi.dll"]

