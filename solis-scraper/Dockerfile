ARG BUILD_FROM

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/SolisScraper/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY src/SolisScraper ./
RUN dotnet publish -c Release -o out

FROM $BUILD_FROM

RUN apk add --no-cache \
    ca-certificates \
    \
    # .NET dependencies
    krb5-libs \
    icu-libs \
    libgcc \
    libintl \
    libssl3 \
    libstdc++ \
    zlib

ENV \
    # Configure web servers to bind to port 80 when present
    ASPNETCORE_URLS=http://+:80 \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true \
    # Set the invariant mode since ICU package isn't included (see https://github.com/dotnet/announcements/issues/20)
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

# Install dotnet runtime
RUN \
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x ./dotnet-install.sh && \
    ./dotnet-install.sh --runtime aspnetcore --channel 8.0 --install-dir /usr/share/dotnet && \
    ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet


WORKDIR /app

# Copy data from build-env
COPY --from=build-env /app/out .

# Copy data for add-on
COPY rootfs /
RUN chmod a+x ./run.sh

CMD [ "./run.sh" ]