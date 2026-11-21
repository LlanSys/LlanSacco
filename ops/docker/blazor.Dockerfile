# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Global.json", "."]
COPY ["nuget.config", "."]
COPY ["src/Shared/LS.SharedKernel/LS.SharedKernel.csproj", "src/Shared/LS.SharedKernel/"]
COPY ["src/Shared/LS.SharedKernel.Validation/LS.SharedKernel.Validation.csproj", "src/Shared/LS.SharedKernel.Validation/"]
COPY ["src/Frontend/Shared/LS.UI.Rcl/LS.UI.Rcl.csproj", "src/Frontend/Shared/LS.UI.Rcl/"]
COPY ["src/Frontend/Web/LS.UI.Blazor/LS.UI.Blazor.csproj", "src/Frontend/Web/LS.UI.Blazor/"]

RUN dotnet restore "src/Frontend/Web/LS.UI.Blazor/LS.UI.Blazor.csproj"

COPY ["src/Shared/", "src/Shared/"]
COPY ["src/Frontend/", "src/Frontend/"]

RUN dotnet publish "src/Frontend/Web/LS.UI.Blazor/LS.UI.Blazor.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

# Static web assets can reference framework files from the SDK package cache.
# Copy them into the runtime image so containerized Blazor can serve /_framework/*.
RUN framework_asset="$(find / -path '*/_framework/blazor.web.js' -print -quit)" \
    && test -n "$framework_asset" \
    && framework_asset_dir="$(dirname "$framework_asset")" \
    && mkdir -p /app/publish/wwwroot/_framework \
    && cp "$framework_asset_dir"/blazor.web* /app/publish/wwwroot/_framework/

FROM runtime AS final
WORKDIR /app
RUN mkdir -p /var/llansacco/dataprotection \
    && chown -R $APP_UID:$APP_UID /var/llansacco
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "LS.UI.Blazor.dll"]
