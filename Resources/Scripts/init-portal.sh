#!/usr/bin/env bash
set -e

PORTAL_USER="mcportal"
APP_ROOT="/srv/mcportal"

echo "Creating portal user..."
id -u $PORTAL_USER &>/dev/null || sudo useradd -m -r -d $APP_ROOT -s /usr/sbin/nologin $PORTAL_USER

echo "Creating folder structure..."
sudo mkdir -p $APP_ROOT/{app,logs,backups,servers}

echo "Setting ownership and permissions..."
sudo chown -R $PORTAL_USER:$PORTAL_USER $APP_ROOT
sudo chmod -R 755 $APP_ROOT

echo "Initialization complete."
