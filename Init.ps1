$containerName = "blazoroauth_sql_server"
$hostName = "localhost,3433"
$databaseName = "BlazorOAuth"
$username = "sa"
$password = "_Password1!" 

# Check if the container exists (running or stopped)
$containerExists = docker ps -a --format '{{.Names}}' | Where-Object {$_ -eq $containerName}

if ($containerExists) {
    # Check if the container is running
    $sqlServerIsRunning = docker ps --format '{{.Names}}' | Where-Object {$_ -eq $containerName}
    
    if ($sqlServerIsRunning) {
        Write-Host "SQL Server container is already running."
    } else {
        Write-Host "Starting the existing SQL Server container.."
        docker start $containerName

        # Check if the docker start command was successful
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Docker start command failed with exit code $LASTEXITCODE"
            Exit
        }

        Write-Host "SQL Server container started."
    }
} else {
    # If the container doesn't exist, create and start a new one
    # Pull SQL Server Docker image if not already present
    $imageExists = docker images --format '{{.Repository}}' | Where-Object {$_ -like "*mssql*"}
    if (-not $imageExists) {
        Write-Host "Downloading SQL Server container image.."
        docker pull mcr.microsoft.com/mssql/server
        Write-Host "Downloaded SQL Server container image."
    }

    # Create and start a new SQL Server container
    Write-Host "Creating and starting a new SQL Server container.."
    docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=$password" -p 3433:1433 -d --name $containerName mcr.microsoft.com/mssql/server

    # Check if the docker run command was successful
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Docker run command failed with exit code $LASTEXITCODE"
        Exit
    }

    Write-Host "SQL Server container started."
}
	
# Wait a moment for the container to initialize
Write-Host "Initializing SQL server container. Please wait.."
Start-Sleep -Seconds 30

# Connect and execute a SQL query to check if the database exists
Write-Host "Checking SQL server connection.."
try {
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection
    $sqlConnection.ConnectionString = "Server=$hostName;Database=master;User ID=$username;Password=$password"
    $sqlConnection.Open()  # Try opening the connection

    # Create the SQL command
    $sqlCommand = $sqlConnection.CreateCommand()
    $sqlCommand.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '$databaseName'"
    $databaseExists = $sqlCommand.ExecuteScalar()

    # If the database doesn't exist, create it
    if ($databaseExists -eq 0) {
        Write-Host "Creating database '$databaseName'.."
        $queryCreateDatabase = "CREATE DATABASE [$databaseName]"
        $sqlCommand.CommandText = $queryCreateDatabase
        $sqlCommand.ExecuteNonQuery() > $null
        Write-Host "Database '$databaseName' created."

        # Set sa password
        Write-Host "Setting up sa account.."
        $setSaPasswordQuery = "ALTER LOGIN sa WITH PASSWORD = '$password';"
        $sqlCommand.CommandText = $setSaPasswordQuery
        $sqlCommand.ExecuteNonQuery() > $null
        Write-Host "Setting up sa account done."
    } else {
        Write-Host "SQL server connection test successful."
    }
}
catch {
    # Handle the SQL connection error
    Write-Host "Error: Failed to connect to SQL Server. $_"
    Exit 1  # Exit the script with error
}
finally {
    # Close the SQL connection if it's open
    if ($sqlConnection.State -eq 'Open') {
        $sqlConnection.Close()
    }
}

Write-Host "All done. SQL Server and database is now up and running."

# Pause to prevent the window from closing
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
