#!/bin/bash

if ( ( command -v docker &> /dev/null && command -v dotnet &> /dev/null ) ); then
    echo "Docker and .NET are installed!"
    
    if [ -z "${DiscordTokenProd}" ]; then
        echo "DiscordTokenProd is not set. Setting now.."
        
        # Set the DiscordTokenProd environment variable to the value in the release pipelines variable group
        DiscordTokenProd=$1 || exit
        export DiscordTokenProd
    fi
    
    if [ -z "${DiscordLavalinkServerPassword}" ]; then
        echo "DiscordLavalinkServerPassword is not set. Setting now.."
        
        # Set the DiscordLavalinkServerPassword environment variable to the value in the release pipelines variable group
        DiscordLavalinkServerPassword=$2 || exit
        export DiscordLavalinkServerPassword
    fi
    
    if [ -z "${YoutubeToken}" ]; then
        echo "YoutubeToken is not set. Setting now.."
        
        # Set the YoutubeToken environment variable to the value in the release pipelines variable group
        YoutubeToken=$3 || exit 
        export YoutubeToken
    fi
        
    # Check if the Lava Node is running
    if [ "$(docker ps -q -f name=lavanode)" ]; then
        echo "Starting lavalink docker container..."
        # Start the Lava Node container on port 2333
        docker run -d -p 2333:2333 --name lavanode fredboat/lavalink:latest
        # Copy the backups/application.yml.example file to the container
        docker cp ~/Code/backups/application.yml.example lavanode:/opt/Lavalink/application.yml
        # Restart the container to apply the changes
        docker container restart lavanode
    else
        echo "Lava Node is already running! No need to run."
    fi    
    
    echo "Checking if Howbot.Worker already is running..."
    
    # Check if the "Howbot.Worker" process is running
    if ( ( pgrep "Howbot.Worker" > /dev/null ) ) then
        echo "Process 'Howbot.Worker' is running"
        # Stop the "Howbot.Worker" process
        pkill "Howbot.Worker"
        echo "Stopped process 'Howbot.Worker'"
    fi
    
    echo "Changing to project directory..."
    
    # Change to project directory
    cd Code/Production/HowBot || exit
    
    echo "Giving Howbot.Worker executable permissions..."
    
    # Give the worker executable permissions
    chmod +x ./Howbot.Worker || exit
    
    echo "Starting Howbot.Worker..."
    
    # Run the worker using nohup to prevent the process from being killed when the SSH session ends
    nohup ./Howbot.Worker > ~/Code/Production/HowBot/Howbot.Worker.log 2>&1 &
    
    echo "Howbot.Worker is now running!"
else
    # Check if Docker is installed
    if ( ( command -v docker &> /dev/null ) )
    then
        echo "Docker is installed"
    else
        echo "Docker is not installed"
    fi
    
    # Check if .NET is installed
    if ( ( command -v dotnet &> /dev/null ) )
    then
        echo ".NET is installed"
    else
        echo ".NET is not installed"
    fi
fi