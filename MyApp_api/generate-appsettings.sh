#!/bin/bash

TEMPLATE="appsettings.json.template"
OUTPUT="appsettings.json"

read -p "Enter MySQL host (default: localhost): " HOST
HOST=${HOST:-localhost}

read -p "Enter MySQL port (default: 3306): " PORT
PORT=${PORT:-3306}

read -p "Enter database name: " DB
read -p "Enter MySQL username: " USER
read -s -p "Enter MySQL password: " PASSWORD
echo

if [ ! -f "$TEMPLATE" ]; then
    echo "Template file $TEMPLATE not found!"
    exit 1
fi

cp "$TEMPLATE" "$OUTPUT"

sed -i "s/__HOST__/$HOST/" "$OUTPUT"
sed -i "s/__PORT__/$PORT/" "$OUTPUT"
sed -i "s/__DB__/$DB/" "$OUTPUT"
sed -i "s/__USER__/$USER/" "$OUTPUT"
sed -i "s/__PASSWORD__/$PASSWORD/" "$OUTPUT"

echo "$OUTPUT created successfully."