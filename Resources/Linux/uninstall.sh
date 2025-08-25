#!/usr/bin/env bash

# Exit on error
set -e

# Set working directory to script path
cd "$(dirname "$0")"

# Environment variables
APP_DIR="/opt/gsd/mcportal"

# Remove application directory
echo "Uninstalling application..."
rmdir "$APP_DIR"

# All done
echo "All done."
