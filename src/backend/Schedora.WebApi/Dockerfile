FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/backend/Schedora.WebApi/Schedora.WebApi.csproj", "Schedora.WebApi/"]
COPY ["src/backend/Schedora.Application/Schedora.Application.csproj", "Schedora.Application/"]
COPY ["src/backend/Schedora.Domain/Schedora.Domain.csproj", "Schedora.Domain/"]
COPY ["src/backend/Schedora.Infrastructure/Schedora.Infrastructure.csproj", "Schedora.Infrastructure/"]

RUN dotnet restore "Schedora.WebApi/Schedora.WebApi.csproj"

COPY . .
COPY src/backend ./

WORKDIR "/src/Schedora.WebApi"
RUN dotnet build "Schedora.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Schedora.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

COPY --from=build /src/Schedora.Infrastructure/EmailTemplates/ /app/publish/EmailTemplates/

USER root
RUN echo "Types: deb" > /etc/apt/sources.list.d/debian.sources && \
    echo "URIs: http://deb.debian.org/debian" >> /etc/apt/sources.list.d/debian.sources && \
    echo "Suites: bookworm bookworm-updates bookworm-security" >> /etc/apt/sources.list.d/debian.sources && \
    echo "Components: main non-free non-free-firmware" >> /etc/apt/sources.list.d/debian.sources && \
    echo "Signed-By: /usr/share/keyrings/debian-archive-keyring.gpg" >> /etc/apt/sources.list.d/debian.sources && \
    echo "deb http://deb.debian.org/debian bookworm main non-free non-free-firmware" > /etc/apt/sources.list.d/non-free.list && \
    echo "deb http://deb.debian.org/debian bookworm-updates main non-free non-free-firmware" >> /etc/apt/sources.list.d/non-free.list && \
    echo "deb http://security.debian.org/debian-security bookworm-security main non-free non-free-firmware" >> /etc/apt/sources.list.d/non-free.list && \
    apt-get update -y && \
    apt-get install -y --no-install-recommends ffmpeg && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Schedora.WebApi.dll"]