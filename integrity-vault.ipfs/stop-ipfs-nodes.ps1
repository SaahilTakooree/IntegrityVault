param(
    [switch]$Force # Optional switch to skip confirmation prompt.
)


# If -Force is NOT provided, show warning and ask for confirmation.
if (-not $Force) {
	& clear # Clear the console for better visibility.

	Write-Host "WARNING: This will stop all running IPFS daemons." -ForegroundColor Red
    	$confirmation = Read-Host "Type 'YES' to continue or anything else to cancel"
	
	# Exit if user does not explicitly confirm.
    	if ($confirmation -ne "YES") {
        	Write-Output "Operation cancelled by user."
        	exit 1
    	}
}



# Get the current working directory.
$currentPath = Get-Location

# Check if the file is in the right directory.
if (-Not (Test-Path (Join-Path $currentPath "ipfs.exe"))) {
	Write-Error "ipfs.exe not found in $currentPath. You are in the wrong directory."
	exit 1
}

# Construct path to ipfs executable (assumes it's in current directory)
$ipfs = Join-Path $currentPath "ipfs.exe"


# Define node configurations.
$nodeConfigs = @(
	@{ Name = "node1"; PortSuffix = "2" },
    	@{ Name = "node2"; PortSuffix = "3" },
    	@{ Name = "node3"; PortSuffix = "4" }
)


# Loop through each node and attempt graceful shutdown via API.
foreach ($nodeConfig in $nodeConfigs) {
    	$p = $nodeConfig.PortSuffix

	# Build API address.
    	$apiAddr = "http://127.0.0.1:500$p"

	# Build paths for node directory and its IPFS repo.
    	$nodePath = Join-Path $currentPath $nodeConfig.Name
    	$nodeRepo = Join-Path $nodePath ".ipfs"

	# Set IPFS_PATH so commands target the correct repo.
    	$env:IPFS_PATH = $nodeRepo

    	Write-Output "Stopping $($nodeConfig.Name)."

	
    	try {
		# Attempt graceful shutdown via IPFS API.
        	& $ipfs --api $apiAddr shutdown 2>&1 | Out-Null
        	Write-Output "$($nodeConfig.Name) stopped."
    	} catch {
        	Write-Warning "Could not stop $($nodeConfig.Name) via API."
    	}
}


# Wait a moment for graceful shutdown, then force-kill any remaining ipfs.exe processes.
Start-Sleep -Seconds 3

# Check for any remaining ipfs.exe processes.
$remaining = Get-Process -Name "ipfs" -ErrorAction SilentlyContinue


if ($remaining) {
    	Write-Output "Force-killing remaining ipfs.exe processes..."
    	$remaining | Stop-Process -Force
    	Write-Output "Done."
} else {
    	Write-Output "All IPFS processes stopped cleanly."
}