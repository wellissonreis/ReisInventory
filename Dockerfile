# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# base runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app
ARG APP_UID=1000
RUN set -eux; \
	groupadd -g ${APP_UID} appuser || true; \
	useradd -u ${APP_UID} -g ${APP_UID} -m -s /bin/bash appuser || true; \
	true
USER appuser


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["reis-inventory.csproj", "."]
RUN dotnet restore "./reis-inventory.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./reis-inventory.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./reis-inventory.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
ARG APP_UID=1000
RUN set -eux; \
		apt-get update; \
		apt-get install -y --no-install-recommends netcat-openbsd; \
		rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish /app/
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh
RUN set -eux; \
		if ! id -u ${APP_UID} >/dev/null 2>&1; then \
			groupadd -g ${APP_UID} appuser || true; \
			useradd -u ${APP_UID} -g ${APP_UID} -m -s /bin/bash appuser || true; \
		fi
USER appuser
ENTRYPOINT ["./entrypoint.sh"]