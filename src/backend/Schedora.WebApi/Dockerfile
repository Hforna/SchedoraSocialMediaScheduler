FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM debian:bookworm AS ffmpeg-build

RUN apt-get update && apt-get install -y \
    build-essential \
    yasm \
    pkg-config \
    git \
    libx264-dev \
    libx265-dev \
    libvpx-dev \
    libmp3lame-dev \
    libopus-dev \
    ca-certificates

WORKDIR /ffmpeg

RUN git clone https://git.ffmpeg.org/ffmpeg.git . && \
    git checkout n7.0

RUN ./configure \
    --enable-shared \
    --disable-static \
    --disable-debug \
    --disable-doc \
    --disable-programs \
    --enable-gpl \
    --enable-libx264 \
    --enable-libx265 \
    --enable-libvpx \
    --enable-libmp3lame \
    --enable-libopus

RUN make -j$(nproc) && make install && ldconfig

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/backend/Schedora.WebApi/Schedora.WebApi.csproj", "Schedora.WebApi/"]
COPY ["src/backend/Schedora.Application/Schedora.Application.csproj", "Schedora.Application/"]
COPY ["src/backend/Schedora.Domain/Schedora.Domain.csproj", "Schedora.Domain/"]
COPY ["src/backend/Schedora.Infrastructure/Schedora.Infrastructure.csproj", "Schedora.Infrastructure/"]

RUN dotnet restore "Schedora.WebApi/Schedora.WebApi.csproj"

COPY src/backend ./

WORKDIR /src/Schedora.WebApi
RUN dotnet publish "Schedora.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
COPY  src/backend/Schedora.Infrastructure/EmailTemplates/ /app/publish/EmailTemplates/

FROM base AS final
USER root
RUN apt-get update && apt-get install -y --no-install-recommends ffmpeg
WORKDIR /app
COPY --from=ffmpeg-build /usr/local/lib /usr/local/lib
COPY --from=ffmpeg-build /usr/local/include /usr/local/include
ENV LD_LIBRARY_PATH=/usr/local/lib
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Schedora.WebApi.dll"]


