#!/bin/bash

set -e

if command -v docker &> /dev/null && command -v dotnet &> /dev/null; then
    echo "Docker and .NET are installed!"
    
    # Check docker if howbot is already running
    HOWBOT_STATUS="$(docker ps -q -f name=howbot)"
    
    if [ -z "$HOWBOT_STATUS" ]; then
        echo "Starting howbot docker compose w/ Lavalink..."
        docker compose -f docker-compose.yml up -d
    else
        echo "Howbot is already running! No need to run."
    fi
    
    # Change to project directory
    cd "$(DeploymentPath)"
    
    echo  "Giving Howbot.Worker permissions..."
    
    # Give the worker executable permissions
    chmod 777 ./Howbot.Worker
    
    echo "Successfully gave Howbot.Worker permissions!"
    
    echo "Howbot.Worker is now running!"
else
    # Check if Docker is installed
    if command -v docker &> /dev/null
    then
        echo "Docker is installed"
    else
        echo "Docker is not installed"
    fi
    
    # Check if .NET is installed
    if command -v dotnet &> /dev/null
    then
        echo ".NET is installed"
    else
        echo ".NET is not installed"
    fi
fi