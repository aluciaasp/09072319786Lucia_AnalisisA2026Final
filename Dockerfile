# ---------- Etapa de compilación ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar el csproj y restaurar (mejor caché de capas)
COPY src/EnviosRapidosGT.Api/EnviosRapidosGT.Api.csproj src/EnviosRapidosGT.Api/
RUN dotnet restore src/EnviosRapidosGT.Api/EnviosRapidosGT.Api.csproj

# Copiar el resto del código y publicar
COPY . .
RUN dotnet publish src/EnviosRapidosGT.Api/EnviosRapidosGT.Api.csproj -c Release -o /app/publish

# ---------- Etapa de ejecución ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render inyecta la variable PORT; la app la lee en Program.cs.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

ENTRYPOINT ["dotnet", "EnviosRapidosGT.Api.dll"]
