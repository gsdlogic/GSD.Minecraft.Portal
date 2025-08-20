#!/usr/bin/env bash
set -e

PORTAL_USER="mcportal"
APP_ROOT="/srv/mcportal"
SERVICE_NAME="mcportal"
SERVICE_FILE="Resources/Linux/mcportal.service"

echo "Creating portal user..."
id -u $PORTAL_USER &>/dev/null || sudo useradd -m -r -d $APP_ROOT -s /usr/sbin/nologin $PORTAL_USER

echo "Creating folder structure..."
sudo mkdir -p $APP_ROOT/{app,logs,backups,servers}

echo "Setting ownership and permissions..."
sudo chown -R $PORTAL_USER:$PORTAL_USER $APP_ROOT
sudo chmod -R 755 $APP_ROOT

echo "Deploying portal app..."
sudo rm -rf $APP_ROOT/app/*
sudo cp -r ./publish/linux/* $APP_ROOT/app/
sudo chown -R mcportal:mcportal $APP_ROOT/app

echo "Deploying service file..."

# Copy the service file into systemd's directory
sudo cp "$SERVICE_FILE" /etc/systemd/system/$SERVICE_NAME.service

# Reload systemd to pick up changes
sudo systemctl daemon-reload

# Enable service (start on boot)
sudo systemctl enable $SERVICE_NAME

# Start service now
sudo systemctl restart $SERVICE_NAME

echo "Service $SERVICE_NAME deployed and started."
