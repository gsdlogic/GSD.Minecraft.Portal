#!/usr/bin/env bash

# Exit on error
set -e

# Require script to run under sudo
if [[ $EUID -ne 0 ]]; then
  echo "Please run as root: sudo $0"
  exit 1
fi

# Set working directory to script path
cd "$(dirname "$0")"

# Environment variables
SOURCE_DIR="../../GSD.Minecraft.Portal/Source/GSD.Minecraft.Portal"
APP_DIR="/opt/gsd/mcportal"

# Install .NET 9 SDK
if ! command -v dotnet &> /dev/null || ! dotnet --list-sdks | grep -q '^9.0'; then
    echo "Installing .NET 9 SDK..."

    wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    sudo dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb

    sudo apt-get update -y
    sudo apt-get install -y dotnet-sdk-9.0
else
    echo ".NET 9 SDK already installed."
fi

echo "Publishing .NET app..."

# Clear application directory
rm -rf "$APP_DIR"/*

# Publish application
dotnet publish "$SOURCE_DIR"/GSD.Minecraft.Portal.csproj -c Release -o "$APP_DIR"

echo "Publish complete."

# All done
echo "All done."
