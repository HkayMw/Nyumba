#!/bin/bash

TEMPLATE="appsettings.json.template"
OUTPUT="appsettings.json"

read -p "Enter MySQL host (default: localhost): " HOST
HOST=${HOST:-localhost}

read -p "Enter MySQL port (default: 3306): " PORT
PORT=${PORT:-3306}

read -p "Enter MySQL server version (default: 8.0.36): " MYSQL_VERSION
MYSQL_VERSION=${MYSQL_VERSION:-8.0.36}

read -p "Enter database name: " DB
read -p "Enter MySQL username: " USER
read -s -p "Enter MySQL password: " PASSWORD
echo

read -s -p "Enter JWT signing key (32+ characters): " JWT_KEY
echo

read -p "Seed sample users and properties on startup? (y/N): " SEED_DATA
case "$SEED_DATA" in
    [yY]|[yY][eE][sS]) SEED_DATA_ON_STARTUP=true ;;
    *) SEED_DATA_ON_STARTUP=false ;;
esac

if [ ${#JWT_KEY} -lt 32 ]; then
    echo "JWT signing key must be at least 32 characters."
    exit 1
fi

if [ ! -f "$TEMPLATE" ]; then
    echo "Template file $TEMPLATE not found!"
    exit 1
fi

cp "$TEMPLATE" "$OUTPUT"

replace_placeholder() {
    local placeholder="$1"
    local value="$2"
    local escaped_value
    escaped_value=$(printf '%s' "$value" | sed 's/[\/&]/\\&/g')
    sed -i "s/$placeholder/$escaped_value/g" "$OUTPUT"
}

replace_placeholder "__HOST__" "$HOST"
replace_placeholder "__PORT__" "$PORT"
replace_placeholder "__DB__" "$DB"
replace_placeholder "__USER__" "$USER"
replace_placeholder "__PASSWORD__" "$PASSWORD"
replace_placeholder "__MYSQL_VERSION__" "$MYSQL_VERSION"
replace_placeholder "__JWT_KEY__" "$JWT_KEY"
replace_placeholder "__SEED_DATA_ON_STARTUP__" "$SEED_DATA_ON_STARTUP"

echo "$OUTPUT created successfully."
