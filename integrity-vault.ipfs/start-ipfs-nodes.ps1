# Clear the terminal.
& clear

# Get current directory

# Get the current directory the file is in.
$currentPath = Get-Location

# Output the current directory that the file is in.
Write-Output "Current directory: $currentPath"

# Check if the file is in the right directory.
if (-Not (Test-Path (Join-Path $currentPath "ipfs.exe"))) {
	Write-Error "ipfs.exe not found in $currentPath. You are in the wrong directory."
	exit 1
}


$nodeDirs = @("node1", "node2", "node3")
foreach ($dir in $nodeDirs) {
    $nodePath = Join-Path $currentPath $dir
    if (-Not (Test-Path $nodePath)) {
        Write-Host "ERROR: $dir folder does not exist." -ForegroundColor Red
        Write-Host "Please run the setup-ipfs-nodes.ps1 script to create the necessary directories, and then rerun the start-ipfs-nodes.ps1 script." -ForegroundColor Red
        exit 1
    }
}



# Node configuration.
$nodeConfigs = @(
    	@{ Name = "node1"; PortSuffix = "2" },
    	@{ Name = "node2"; PortSuffix = "3" },
    	@{ Name = "node3"; PortSuffix = "4" }
)

# Path to IPFS executable.
$ipfs = Join-Path $currentPath "ipfs.exe"


# Check if there are any node running.
$nodesRunning = $false
foreach ($nodeConfig in $nodeConfigs) {
    	$p = $nodeConfig.PortSuffix
    	$apiAddr = "/ip4/127.0.0.1/tcp/500$p"
    	$response = & $ipfs --api $apiAddr id 2>$null
    	if ($LASTEXITCODE -eq 0) {
        	Write-Warning "$($nodeConfig.Name) appears to be running."
        	$nodesRunning = $true
    	}
}


# Stop running nodes if user confirms.
if ($nodesRunning) {
    	Write-Host "One or more IPFS nodes are currently running." -ForegroundColor Yellow
    	Write-Host "Typing 'YES' below WILL RESTART all nodes." -ForegroundColor Red
    	Write-Host "Typing anything else will CANCEL the start up process." -ForegroundColor Red
    
    	# Ask for explicit confirmation
    	$choice = Read-Host "Do you want to stop them and continue? Type 'YES' to continue with restart, anything else to cancel the operation."

    	if ($choice -ne "YES") {
        	Write-Output "Operation cancelled by user."
        	exit 1
    	}
	
	Write-Output "Stopping all IPFS daemons."
	$stopScript = Join-Path $currentPath "stop-ipfs-nodes.ps1"
	& $stopScript -Force
}



# Start nodes.


Write-Host "Starting IPFS nodes."


$jobs = @()
foreach ($nodeConfig in $nodeConfigs) {
	$p = $nodeConfig.PortSuffix
	$nodePath = Join-Path $currentPath $nodeConfig.Name
	$nodeRepo = Join-Path $nodePath ".ipfs"
	$apiPort = "500$p"
    	$gatewayPort = "808$p"
    	$swarmPort = "400$p"

	Write-Output "Starting daemon for $($nodeConfig.Name)."

	# Start each node as a background job.
	$job = Start-Job -ScriptBlock {
		param($ipfsExe, $repoPath)
		$env:IPFS_PATH = $repoPath
		$env:IPFS_TELEMETRY = "off"

		# Start daemon and capture output.
		& $ipfsExe daemon 2>&1
	} -ArgumentList $ipfs, $nodeRepo

	# Store job reference for later monitoring.
	$jobs += [PSCustomObject]@{ Config = $nodeConfig; Job = $job; Repo = $nodeRepo }

	# Print node details.
    	Write-Host "$($node.Name) started with:"
    	Write-Host "	API: http://127.0.0.1:$apiPort"
    	Write-Host "	Gateway: http://127.0.0.1:$gatewayPort"
    	Write-Host " 	Swarm: /ip4/127.0.0.1/tcp/$swarmPort"
	Write-Host " "
}


# Wait for all daemons to be ready.
Write-Host "Waiting for daemons to come online."
foreach ($nodeConfig in $nodeConfigs) {
    	$ready = $false
    	$jobEntry = $jobs | Where-Object { $_.Config.Name -eq $nodeConfig.Name }
    	for ($i = 0; $i -lt 60; $i++) {
        	$output = Receive-Job -Job $jobEntry.Job -Keep
        	if ($output -match "Daemon is ready") {
           		Write-Output "$($nodeConfig.Name) is ready."
            		$ready = $true
            		break
        	}
        	Start-Sleep -Seconds 1
    	}
    	if (-not $ready) {
        	Write-Error "$($nodeConfig.Name) did not come online within 60 seconds."
        	exit 1
    	}
}


# Re-peer all nodes with each other.
Write-Host "Re-connecting peers."
$peerAddresses = @{}
foreach ($nodeConfig in $nodeConfigs) {
    	$p = $nodeConfig.PortSuffix
    	$apiAddr = "/ip4/127.0.0.1/tcp/500$p"
    	$idJson = & $ipfs --api $apiAddr id 2>&1 | Where-Object { $_ -notmatch "^Error" } | ConvertFrom-Json
    	$peerId = $idJson.ID
    	$peerAddresses[$nodeConfig.Name] = "/ip4/127.0.0.1/tcp/400$p/p2p/$peerId"
    	Write-Output "$($nodeConfig.Name): $($peerAddresses[$nodeConfig.Name])"
}

foreach ($nodeConfig in $nodeConfigs) {
    	$p = $nodeConfig.PortSuffix
    	$apiAddr = "/ip4/127.0.0.1/tcp/500$p"
    	foreach ($target in $nodeConfigs) {
        	if ($target.Name -eq $nodeConfig.Name) { continue }
        	$addr = $peerAddresses[$target.Name]
        	Write-Output "Connecting $($nodeConfig.Name) -> $($target.Name)"
        	& $ipfs --api $apiAddr swarm connect $addr
    }
}


Write-Host "`All nodes started." -ForegroundColor Green

# Important user instructions.
Write-Host "IMPORTANT:" -ForegroundColor Yellow
Write-Host "- Closing this terminal MAY NOT stop the IPFS nodes." -ForegroundColor Yellow
Write-Host "- To stop all nodes, you can either run 'stop-ipfs-nodes.ps1' manually" -ForegroundColor Yellow
Write-Host "- Or type 'YES' below to stop the nodes safely." -ForegroundColor Yellow
Write-Host " "


# Loop until the user types 'YES'.
while ($true) {
    $choice = Read-Host "Do you want to stop all nodes? Type 'YES' to stop"

    if ($choice -eq "YES") {
        Write-Output "Stopping all IPFS daemons."
        $stopScript = Join-Path $currentPath "stop-ipfs-nodes.ps1"
        
        if (Test-Path $stopScript) {
            & $stopScript -Force
        } else {
            Write-Warning "stop-ipfs-nodes.ps1 not found. Nodes will keep running."
        }
        break  # Exit the loop after stopping the nodes.
    } else {
        Write-Host "You must type 'YES' to stop the nodes. Try again." -ForegroundColor Yellow
    }
}

