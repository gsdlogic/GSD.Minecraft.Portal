# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
#FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled-extra AS base
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble AS base
USER $APP_UID
WORKDIR /app

# Install Bedrock dependencies
USER root
RUN apt-get update && apt-get install -y \
    libcurl4 \
    libssl3 \
    unzip \
 && rm -rf /var/lib/apt/lists/*
 USER app

EXPOSE 8080
EXPOSE 8081
EXPOSE 19132

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Source/GSD.Minecraft.Portal/GSD.Minecraft.Portal.csproj", "Source/GSD.Minecraft.Portal/"]
RUN dotnet restore "./Source/GSD.Minecraft.Portal/GSD.Minecraft.Portal.csproj"
COPY . .
WORKDIR "/src/Source/GSD.Minecraft.Portal"
RUN dotnet build "./GSD.Minecraft.Portal.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GSD.Minecraft.Portal.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GSD.Minecraft.Portal.dll"]