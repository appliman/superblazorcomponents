# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:11.0.0 AS base
# Upgrade perl-base and remove the unused Pebble binary inherited from the base image.
RUN apt-get update \
    && apt-get install -y --no-install-recommends --only-upgrade perl-base \
    && rm -rf /var/lib/apt/lists/* \
    && rm -f /usr/bin/pebble
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:11.0.100 AS build
# The SDK stage invokes dotnet directly and does not need Pebble.
RUN rm -f /usr/bin/pebble
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DemoWebSite/DemoWebSite.csproj", "src/DemoWebSite/"]
COPY ["src/SuperBlazorComponents/SuperBlazorComponents.csproj", "src/SuperBlazorComponents/"]
COPY ["src/SuperBlazorComponents.DataGridExporter/SuperBlazorComponents.DataGridExporter.csproj", "src/SuperBlazorComponents.DataGridExporter/"]
RUN dotnet restore "./src/DemoWebSite/DemoWebSite.csproj"
COPY . .
WORKDIR "/src/src/DemoWebSite"
RUN dotnet build "./DemoWebSite.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DemoWebSite.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DemoWebSite.dll"]
