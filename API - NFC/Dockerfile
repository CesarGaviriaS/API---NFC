# Etapa 1: Build del proyecto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos todo el proyecto
COPY . .

# Restauramos dependencias
RUN dotnet restore

# Publicamos en modo Release
RUN dotnet publish -c Release -o /app/publish


# Etapa 2: Imagen final para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copiamos lo publicado
COPY --from=build /app/publish .

# Render asigna el puerto con una variable llamada PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Exponemos un puerto (Render lo redirige internamente)
EXPOSE 8080

# El nombre EXACTO del DLL (incluye espacios)
ENTRYPOINT ["dotnet", "API - NFC.dll"]