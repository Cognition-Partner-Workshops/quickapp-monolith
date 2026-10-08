# Multi-stage build: Angular client -> ASP.NET Core publish -> slim runtime image.
ARG NODE_IMAGE=node:22-bookworm-slim

FROM ${NODE_IMAGE} AS client
WORKDIR /src/quickapp.client
COPY ["quickapp.client/package.json", "quickapp.client/package-lock.json", "./"]
RUN npm ci --no-audit --no-fund
COPY ["quickapp.client/", "./"]
RUN npx ng build --configuration production

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["QuickApp.Core/QuickApp.Core.csproj", "QuickApp.Core/"]
COPY ["QuickApp.Server/QuickApp.Server.csproj", "QuickApp.Server/"]
RUN dotnet restore "QuickApp.Server/QuickApp.Server.csproj" -p:SkipSpaBuild=true

COPY ["QuickApp.Core/", "QuickApp.Core/"]
COPY ["QuickApp.Server/", "QuickApp.Server/"]
# Copied into wwwroot before publish so the files are included in the static web assets manifest.
COPY --from=client ["/src/quickapp.client/dist/quickapp.client/browser/", "QuickApp.Server/wwwroot/"]
RUN dotnet publish "QuickApp.Server/QuickApp.Server.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish --no-restore \
    -p:SkipSpaBuild=true -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true
COPY --from=build /app/publish .
RUN mkdir -p /app/Logs && chown $APP_UID /app/Logs
USER $APP_UID
ENTRYPOINT ["dotnet", "QuickApp.Server.dll"]
