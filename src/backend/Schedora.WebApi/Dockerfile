FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Schedora.WebApi/Schedora.WebApi.csproj", "Schedora.WebApi/"]
COPY ["Schedora.Application/Schedora.Application.csproj", "Schedora.Application/"]
COPY ["Schedora.Domain/Schedora.Domain.csproj", "Schedora.Domain/"]
COPY ["Schedora.Infrastructure/Schedora.Infrastructure.csproj", "Schedora.Infrastructure/"]

RUN dotnet restore "Schedora.WebApi/Schedora.WebApi.csproj"

COPY . .

WORKDIR "/src/Schedora.WebApi"
RUN dotnet build "Schedora.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Schedora.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

COPY --from=build /src/Schedora.Infrastructure/EmailTemplates/ /app/publish/EmailTemplates/

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Schedora.WebApi.dll"]