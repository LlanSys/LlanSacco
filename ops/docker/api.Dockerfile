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
COPY ["src/Backend/Domain/LS.Domain/LS.Domain.csproj", "src/Backend/Domain/LS.Domain/"]
COPY ["src/Backend/Application/LS.Application/LS.Application.csproj", "src/Backend/Application/LS.Application/"]
COPY ["src/Backend/Infrastructure/LS.Infrastructure/LS.Infrastructure.csproj", "src/Backend/Infrastructure/LS.Infrastructure/"]
COPY ["src/Backend/Persistence/LS.Persistence/LS.Persistence.csproj", "src/Backend/Persistence/LS.Persistence/"]
COPY ["src/Backend/Api/LS.Api/LS.Api.csproj", "src/Backend/Api/LS.Api/"]

RUN dotnet restore "src/Backend/Api/LS.Api/LS.Api.csproj"

COPY ["src/Shared/", "src/Shared/"]
COPY ["src/Backend/", "src/Backend/"]

RUN dotnet publish "src/Backend/Api/LS.Api/LS.Api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "LS.Api.dll"]
