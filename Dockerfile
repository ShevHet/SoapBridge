FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY IcutechTestApi/IcutechTestApi.csproj ./
RUN dotnet restore IcutechTestApi.csproj

COPY IcutechTestApi/ ./
RUN dotnet publish IcutechTestApi.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 10000

ENTRYPOINT ["dotnet", "IcutechTestApi.dll"]

