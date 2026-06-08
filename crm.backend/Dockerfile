FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY crm.backend.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish crm.backend.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/uploads /app/logs \
    && chown -R app:app /app

COPY --from=build --chown=app:app /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER app

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=5 \
  CMD curl --fail --silent http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "crm.backend.dll"]
