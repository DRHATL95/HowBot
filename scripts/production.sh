if command -v docker &> /dev/null && command -v dotnet &> /dev/null; then
    echo "Docker and .NET are installed!"
    
    LAVANODE_STATUS="$(docker ps -q -f name=lavanode)"
    
    echo "LAVANODE_STATUS: $(LAVANODE_STATUS)"
    
    if [ -z "$LAVANODE_STATUS" ]; then
        echo "Starting lavalink docker container..."
        docker run -d -p 2333:2333 --name lavanode fredboat/lavalink:latest
        docker cp ~/Code/backups/application.yml.example lavanode:/opt/Lavalink/application.yml
        docker container restart lavanode
    else
        echo "Lava Node is already running! No need to run."
    fi  
    
    # Define environment variables
    DiscordTokenProd=$(DiscordTokenProd)
    DiscordLavalinkServerPassword=$(DiscordLavalinkServerPassword)
    YoutubeToken=$(Youtube)
    
    # Export environment variables separately to avoid masking the return values
    export DiscordTokenProd
    export DiscordLavalinkServerPassword
    export YoutubeToken
    
    # Change to project directory
    cd "$(DeploymentPath)" || exit
    
    # Give the worker executable permissions
    chmod 777 ./Howbot.Worker || exit
    
    # Run the worker using nohup to prevent the process from being killed when the SSH session ends
    nohup ./Howbot.Worker > ~/Code/Production/HowBot/Howbot.Worker.log 2>&1 &
    
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