# --- FASE 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solución
COPY API_NFC.sln ./

# Copiar proyecto completo
COPY API_NFC/ ./API_NFC/

# Restaurar dependencias
RUN dotnet restore API_NFC/API_NFC.csproj

# Publicar en modo Release
RUN dotnet publish API_NFC/API_NFC.csproj -c Release -o /app/publish


# --- FASE 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "API_NFC.dll"]