param(
    [switch]$Force
)

if (-not $Force) {
	# Clear the console.
	& clear

	# Check if user really want to configure this node.
	Write-Host "WARNING: This script will completely delete and recreate your IPFS setup." -ForegroundColor Red
	Write-Host "All existing node folders and configurations in this directory will likely be lost." -ForegroundColor Red

	$confirmation = Read-Host "Type 'YES' to continue or anything else to cancel"

	if ($confirmation -ne "YES") {
    		Write-Output "Operation cancelled by user."
    		exit 1
	}


	# Clear the console.
	& clear
}



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



# Generate the swarm key.

# Create the swarm key.
$swarmKeyPath = Join-Path $currentPath "swarm.key"

# Check if there is a already a swarm key. if yes, remove it.
if (Test-Path $swarmKeyPath) {
	Write-Warning "swarm.key already exists. Deleting and replacing it."
	Remove-Item $swarmKeyPath -Force
}

# Define the content the swarm.key.
$swarmKeyContent = @"
/key/swarm/psk/1.0.0/
/base16/
575b7905c65933fa4b7f500ac4217e212f8f6917446170a762486c24110dde3b
"@

# Create the swarm key file with the pre define content.
[System.IO.File]::WriteAllText($swarmKeyPath, $swarmKeyContent, [System.Text.UTF8Encoding]::new($false))

# Output the location of the swarm key.
Write-Output "swarm.key created at: $swarmKeyPath"



# Create fresh directories for each node.


# Define the node with a unique port suffix to avoid port conflicts.
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
    	Write-Host "Typing 'YES' below WILL STOP all running nodes, DELETE their data, AND continue the configuration process." -ForegroundColor Red
    	Write-Host "Typing anything else will CANCEL the configuration process." -ForegroundColor Red
    
    	# Ask for explicit confirmation
    	$choice = Read-Host "Do you want to stop them and continue? Type 'YES' to stop, anything else to cancel"

    	if ($choice -ne "YES") {
        	Write-Output "Operation cancelled by user."
        	exit 1
    	}

    	$stopScript = Join-Path $currentPath "stop-ipfs-nodes.ps1"
    	if (Test-Path $stopScript) {
        	Write-Output "Stopping running nodes..."
        	& $stopScript -Force
        	Start-Sleep -Seconds 3
    	} else {
        	Write-Error "stop.ps1 not found. Cannot safely continue."
        	exit 1
    	}
}



# Create the node folder.
foreach ($nodeConfig in $nodeConfigs) {
	
	# Get the node path. 
	$nodePath = Join-Path $currentPath $nodeConfig.Name

	# Check if the node already exist. If yes, remove it.
	if (Test-Path $nodePath) {
		Write-Warning "$nodePath already exists. Deleting and replacing $($nodeConfig.Name)."
        	Remove-Item $nodePath -Recurse -Force
    	}
	
	# Create directory for node.
	New-Item -ItemType Directory -Path $nodePath | Out-Null
	Write-Output "Folder created: $($nodePath)."
}




# Initialise and configure each node.


foreach ($nodeConfig in $nodeConfigs) {
	$p = $nodeConfig.PortSuffix
	$nodePath = Join-Path $currentPath $nodeConfig.Name
	$nodeRepo = Join-Path $nodePath ".ipfs"

	# Set IPFS_PATH so commands operate on this node's repository.
	$env:IPFS_PATH = $nodeRepo
	
	Write-Output "Initialising IPFS repo in $($nodeConfig.Name)."

	# Initialise IPFS repository.
	& $ipfs init
	
	#
	Write-Output "IPFS repo initialised for $($nodeConfig.Name) at $($nodeRepo)."

    	# Copy swarm.key.
    	$destSwarmKey = Join-Path $nodeRepo "swarm.key"
    	Copy-Item -Path $swarmKeyPath -Destination $destSwarmKey -Force
    	Write-Output "swarm.key copied to $nodeRepo"

    	# Read only PeerID and PrivKey from the generated config.
    	$configPath = Join-Path $nodeRepo "config"
    	$existingJson = Get-Content $configPath -Raw | ConvertFrom-Json
    	$peerID = $existingJson.Identity.PeerID
    	$privKey = $existingJson.Identity.PrivKey
	
	#
    	Write-Output "Configuring $($nodeConfig.Name)."
	
	# Overwrite config with custom settings.
    $configJson = "{
  `"Identity`": {
    `"PeerID`": `"$peerID`",
    `"PrivKey`": `"$privKey`"
  },
  `"Datastore`": {
    `"StorageMax`": `"10GB`",
    `"StorageGCWatermark`": 90,
    `"GCPeriod`": `"1h`",
    `"Spec`": {
      `"mounts`": [
        {
          `"mountpoint`": `"/blocks`",
          `"path`": `"blocks`",
          `"prefix`": `"flatfs.datastore`",
          `"shardFunc`": `"/repo/flatfs/shard/v1/next-to-last/2`",
          `"sync`": false,
          `"type`": `"flatfs`"
        },
        {
          `"compression`": `"none`",
          `"mountpoint`": `"/`",
          `"path`": `"datastore`",
          `"prefix`": `"leveldb.datastore`",
          `"type`": `"levelds`"
        }
      ],
      `"type`": `"mount`"
    },
    `"HashOnRead`": false,
    `"BloomFilterSize`": 0,
    `"BlockKeyCacheSize`": null
  },
  `"Addresses`": {
    `"Swarm`": [
      `"/ip4/127.0.0.1/tcp/400$p`",
      `"/ip6/::/tcp/400$p`",
      `"/ip4/127.0.0.1/udp/400$p/webrtc-direct`",
      `"/ip4/127.0.0.1/udp/400$p/quic-v1`",
      `"/ip4/127.0.0.1/udp/400$p/quic-v1/webtransport`",
      `"/ip6/::/udp/400$p/webrtc-direct`",
      `"/ip6/::/udp/400$p/quic-v1`",
      `"/ip6/::/udp/400$p/quic-v1/webtransport`"
    ],
    `"Announce`": [],
    `"AppendAnnounce`": [],
    `"NoAnnounce`": [],
    `"API`": `"/ip4/127.0.0.1/tcp/500$p`",
    `"Gateway`": `"/ip4/127.0.0.1/tcp/808$p`"
  },
  `"Mounts`": {
    `"IPFS`": `"/ipfs`",
    `"IPNS`": `"/ipns`",
    `"MFS`": `"/mfs`",
    `"FuseAllowOther`": false
  },
  `"Discovery`": {
    `"MDNS`": {
      `"Enabled`": false
    }
  },
  `"Routing`": {
    `"Type`": `"dht`"
  },
  `"Ipns`": {
    `"RepublishPeriod`": `"`",
    `"RecordLifetime`": `"`",
    `"ResolveCacheSize`": 128
  },
  `"Bootstrap`": [],
  `"Gateway`": {
    `"HTTPHeaders`": {},
    `"RootRedirect`": `"`",
    `"NoFetch`": false,
    `"NoDNSLink`": false,
    `"DeserializedResponses`": null,
    `"AllowCodecConversion`": null,
    `"DisableHTMLErrors`": null,
    `"PublicGateways`": null,
    `"ExposeRoutingAPI`": null
  },
  `"API`": {
    `"HTTPHeaders`": {}
  },
  `"Swarm`": {
    `"AddrFilters`": null,
    `"DisableBandwidthMetrics`": false,
    `"DisableNatPortMap`": true,
    `"RelayClient`": {},
    `"RelayService`": {},
    `"Transports`": {
      `"Network`": { `"Websocket`": false },
      `"Security`": {},
      `"Multiplexers`": {}
    },
    `"ConnMgr`": {},
    `"ResourceMgr`": {}
  },
  `"AutoNAT`": {},
  `"AutoTLS`": { `"Enabled`": false },
  `"Pubsub`": {
    `"Router`": `"`",
    `"DisableSigning`": false,
    `"Enabled`": true
  },
  `"Peering`": {
    `"Peers`": null
  },
  `"DNS`": {
    `"Resolvers`": {}
  },
  `"Migration`": {},
  `"AutoConf`": { `"Enabled`": false },
  `"Provide`": {
    `"Strategy`": `"all`",
    `"DHT`": {
        `"Interval`": `"12h`"
    }
  },
  `"Provider`": {},
  `"Reprovider`": {},
  `"HTTPRetrieval`": {},
  `"Experimental`": {
    `"FilestoreEnabled`": false,
    `"UrlstoreEnabled`": false,
    `"Libp2pStreamMounting`": false,
    `"P2pHttpProxy`": false,
    `"OptimisticProvide`": false,
    `"OptimisticProvideJobsPoolSize`": 0
  },
  `"Plugins`": {
    `"Plugins`": null
  },
  `"Pinning`": {
    `"RemoteServices`": {}
  },
  `"Import`": {
    `"CidVersion`": null,
    `"UnixFSRawLeaves`": null,
    `"UnixFSChunker`": null,
    `"HashFunction`": null,
    `"UnixFSFileMaxLinks`": null,
    `"UnixFSDirectoryMaxLinks`": null,
    `"UnixFSHAMTDirectoryMaxFanout`": null,
    `"UnixFSHAMTDirectorySizeThreshold`": null,
    `"UnixFSHAMTDirectorySizeEstimation`": null,
    `"UnixFSDAGLayout`": null,
    `"BatchMaxNodes`": null,
    `"BatchMaxSize`": null,
    `"FastProvideRoot`": null,
    `"FastProvideWait`": null
  },
  `"Version`": {},
  `"Internal`": {},
  `"Bitswap`": {}
}"
	
	# Write new config to file.
    	[System.IO.File]::WriteAllText($configPath, $configJson, [System.Text.UTF8Encoding]::new($false))
	
	# Verify swarm.key.
    	$destContent = Get-Content (Join-Path $nodeRepo "swarm.key") -Raw
    	if ($destContent -notmatch "psk/1.0.0") {
        	Write-Error "swarm.key verification failed for $($nodeConfig.Name)!"
        	exit 1
    	}
	
   	Write-Output "swarm.key verified for $($nodeConfig.Name)."
    	Write-Output "$($nodeConfig.Name) configured."
}



# Start Daemons.

$jobs = @()
foreach ($nodeConfig in $nodeConfigs) {
	$nodePath = Join-Path $currentPath $nodeConfig.Name
	$nodeRepo = Join-Path $nodePath ".ipfs"
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
}


# Wait for all daemons to be ready by watching for "Daemon is ready" in job output.
Write-Output "Waiting for daemons to come online."

foreach ($nodeConfig in $nodeConfigs) {
	$ready = $false
	$jobEntry = $jobs | Where-Object { $_.Config.Name -eq $nodeConfig.Name }

	# Wait up to 60 seconds for readiness message.
	for ($i = 0; $i -lt 60; $i++) {
		$output = Receive-Job -Job $jobEntry.Job -Keep
		if ($output -match "Daemon is ready") {
			Write-Output "$($nodeConfig.Name) is ready."
			$ready = $true
			break
		}
        	Start-Sleep -Seconds 1
    	}
	
	# Fail if node never became ready.
	if (-not $ready) {
		Write-Error "$($nodeConfig.Name) did not come online within 60 seconds."
		Write-Output "Daemon output for $($nodeConfig.Name)."
		Receive-Job -Job $jobEntry.Job -Keep | ForEach-Object { Write-Output $_ }
		exit 1
	}
}



# Collect peer IDs and swarm addresses.
$peerAddresses = @{}
foreach ($nodeConfig in $nodeConfigs) {
	$p = $nodeConfig.PortSuffix
	$apiAddr = "/ip4/127.0.0.1/tcp/500$p"

	# Query node identity via API.
	$idJson = & $ipfs --api $apiAddr id 2>&1 | Where-Object { $_ -notmatch "^Error" } | ConvertFrom-Json
	$peerId = $idJson.ID

	# Build full multiaddr for swarm connection.
	$swarmAddr = "/ip4/127.0.0.1/tcp/400$p/p2p/$peerId"
	$peerAddresses[$nodeConfig.Name] = $swarmAddr
	Write-Output "$($nodeConfig.Name) peer address: $swarmAddr"
}


$success = $true
# Connect all nodes to each other.
foreach ($nodeConfig in $nodeConfigs) {
	$p = $nodeConfig.PortSuffix
	$apiAddr = "/ip4/127.0.0.1/tcp/500$p"
	foreach ($target in $nodeConfigs) {
		if ($target.Name -eq $nodeConfig.Name) {
			continue
		}
		$addr = $peerAddresses[$target.Name]
        	Write-Output "Connecting $($nodeConfig.Name) -> $($target.Name) ($addr)"

		# Attempt connection.
        	& $ipfs --api $apiAddr swarm connect $addr
        	if ($LASTEXITCODE -ne 0) {
            		Write-Warning "Connection from $($nodeConfig.Name) to $($target.Name) failed."
			$success = $false
        	}
    	}
}


if ($success) {
	Write-Output "Stopping the nodes that were just created and started for configuration..."
}


$stopScript = Join-Path $currentPath "stop-ipfs-nodes.ps1"

if (Test-Path $stopScript) {
	& $stopScript -Force
} else {
    	Write-Warning "stop-kubo.ps1 not found. Nodes will keep running."
}


# Inform user that the configuration was successfull.
Write-Output "Configuration complete. You can now start up the nodes by running start-ipfs-nodes.ps1."