#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM 469214988751.dkr.ecr.eu-central-1.amazonaws.com/availia-aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build

ARG Configuration="Release"

ARG CACHEBUST=1
RUN echo "CACHEBUST: $CACHEBUST"

ENV DOTNET_CLI_TELEMETRY_OPTOUT=true \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
WORKDIR /src

COPY ["src/Directory.Build.props", "src/"]
COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Messaging.Contracts/Messaging.Contracts.csproj", "src/Messaging.Contracts/"]

RUN dotnet restore "src/Api/Api.csproj"

COPY . .

RUN dotnet build "src/Api/Api.csproj" --configuration $Configuration -o /app/build

FROM build AS publish

ARG Configuration="Release"
ARG MENTORHUB_PRIVATE_NUGET_REPOSITORY_URL

RUN dotnet publish "src/Api/Api.csproj" --configuration $Configuration --output /app/publish

FROM build as unittest
WORKDIR /src/tests/

FROM build as integrationtest
WORKDIR /src/tests/

FROM base AS final

LABEL org.opencontainers.image.title="SearchService.Api" \
    org.opencontainers.image.description="" \
    org.opencontainers.image.documentation="" \
    org.opencontainers.image.source="" \
    org.opencontainers.image.url="" \
    org.opencontainers.image.vendor=""

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SearchService.Api.dll"]
