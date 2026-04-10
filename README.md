# READ THIS FIRST: Critical Project Notice

## Blockchain Sync & Data Integrity Warning
This project uses a shared test account and a public blockchain (Sepolia). Because the blockchain is a "single source of truth," if Person A tests the system, the blockchain moves to State Z. If Person B then starts with a fresh local database, their system will try to write State Z again, but the blockchain will reject it as a duplicate or out-of-sync operation.
* The Fix: If you encounter sync issues, you must follow Section 4: Fresh Start to redeploy your own contract and reset your local database to ensure they are synchronised.

## Pre-configured Account Credentials
* Default Password: All pre-seeded users (Super Admin, Admins, Doctors, Patient, and External Provider) use the password: `Qwerty!2`
* Test ETH Balance: Before starting, open MetaMask. If the balance is 0 ETH, you must mine/request test ETH (see "How to Get Sepolia Test ETH" Section). Others may have exhausted the funds.

## How to Find Usernames
Since passwords are encrypted in the database, use these two methods to find login handles:
1.  Frontend Method: Login as Super Admin (`username: superadmin` / `password: Qwerty!2`). Navigate to the Admin Section to view the list of registered Admins. Then log in the the Admin account to view the Doctor, Patient, and External Provider accociated with that account.
2.  Database Method: Open SSMS and run:
    ```sql
    SELECT Username, Email, Role FROM [IntegrityVaultDb].[dbo].[Users];
    ```

## Login Issues? (IP Restriction Security)
If you are certain the username and password are correct but receive "Invalid Credentials," it is likely due to the system's IP-based security. To simulate a secure hospital environment, users (Doctors/Admins/External Provider) can only log in from authorised IP addresses.
* The Fix: 1.  Go to [https://whatismyipaddress.com/](https://whatismyipaddress.com/) and copy your current IPv4 or IPv6.
    2.  Login as Super Admin.
    3.  Navigate to the Hospital section.
    4.  Add your current IP address to the authorised list for that specific hospital by clicking the edit button.
* *Note: Because most home internet uses dynamic IPs, you may need to repeat this if ip changes mid testing*

---
---

<br><br><br><br>

# Table of Possible Action (Quick Start Guide)

## Want to run the project for the first time?

Follow:
* Section 1: Setup Environment (complete all steps)
* Section 2: Run the Project

---

## Have already set up the project and just want to run it again?

Follow:

* Section 2: Run the Project

---

## Want to stop the project safely?

Follow:

* Section 3: Stop the Project

---

## Want to completely reset everything and start fresh?

Follow:

* Section 4: Fresh Start (Complete Reset)

---

## Only want to view blockchain transactions or contract activity?

Follow:
* "Monitoring Blockchain Activity" section

---

## Want to add a new Hospital?

Follow:
* "Add Hospital" section

## Want Sepolia ETH (test funds)?

Follow:

* "How to Get Sepolia Test ETH" section

---

## Want to create the Super Admin account?

Follow:

* Section 4 -> Step 8 (Initialise the System)

---

## Want to deploy the smart contract again?

Follow:

* Section 4 -> Step 5 (Deploy Blockchain Contracts) part e (Can look in IntegrityVault.Api -> appsetting.json for the superadmin blockcahin address.)
* Then update:
* Section 4 -> Step 6 (Backend Configuration) only update contract address when following the step nothing else

### Important After Redeployment

After redeploying the contract:

You must also ensure system consistency:

#### 1. Update backend config

* Replace only:

  ```
  ContractAddress
  ```

#### 2. Reset database (if mismatch occurs)

If previous blockchain data exists:

* Go to:

  * Section 4 (Fresh Start) -> Step 1: Clean and Re-run Setup ( Case 2 part a)

* Re-run:

  * Migration steps (Section 1 -> Step 7–9)

* Create the super admin with write detail 

  * * Section 4 -> Step 8 (Initialise the System)

---

> Reason: Database and blockchain must always be synchronised.
> If not, medical record transactions may fail or be rejected and verfication will fail.

---

## Changed something and the project is broken?

Recommended:

* Section 4: Fresh Start (Complete Reset)

---

## Getting errors related to database or migrations?

Try:

* Section 4 (Fresh Start) -> Step 1: Clean and Re-run Setup ( Case 2 part a)
* Section 1 (Setup Environment) -> Steps 7 to 9

### Important Note

Doing this can cause a mismatch between the database and the blockchain state.

As a result:

* The smart contract may reject new medical records
* Existing references may become invalid

### Recommended Fix

If this happens, you should:

1. Redeploy the smart contract
   -> Section 4 -> Step 5
2. Update backend configuration
   -> Section 4 -> Step 6
3. Create a new superadmin with the right detail
   -> Section 4 -> Step 8 (Initialise the System)

> This ensures both Database state and Blockchain state are fully synchronised again.
> If not, medical record transactions may fail or be rejected and verfication will fail.
---
---

<br><br><br><br>
---
# Monitoring Blockchain Activity (Sepolia Etherscan)

You can monitor all blockchain transactions and contract interactions using:
[https://sepolia.etherscan.io/](https://sepolia.etherscan.io/)

---

If you are using the pre-configured setup (Section 1 and 2), the following addresses are relevant:

### Super Admin Wallet

* `0xc220F2e826d9F791B0a631ffeF8b199c35853e9B`

### Hospital Wallets (Sample Accounts)

* `0xfbE180B05ECEE0010B99674a91CCC3570559562c`
* `0x1459c5fFdefDB27A76bBA50E9686c9C2D4DCBbd3`
* `0xf9c66CA8112Faa43916Ee66b78D7f8a22e6E804D`

### Smart Contract Address

* `0x54a1844b9B73d2df7f546fE6dC5c7Ce1341c5ffd`

---

### Notes

* You can paste any of the above addresses into Sepolia Etherscan to:

  * View transactions
  * Track contract interactions
  * Verify blockchain activity

* You are not limited to these accounts:

  * If you create new wallets (e.g., via Section 4: Fresh Start), you can monitor them as well
  * Any wallet address used in the system can be tracked via Etherscan

* You can view any wallet address that exists on the Sepolia test network.

    - This includes:
        - The provided test accounts
        - Any new wallets you create and use within this project
        - Any wallet that has interacted with your deployed contract

    -  This applies only to the Sepolia network. Wallet activity from other networks (e.g., Ethereum Mainnet) will not appear unless viewed on the correct network explorer.
---
---

<br><br><br><br>
---

# How to Get Sepolia Test ETH 

If you are using your own MetaMask wallet (not the pre-configured account), you will need Sepolia test ETH to perform blockchain transactions.

You can obtain test ETH using the Sepolia faucet:

---

## Steps

1. Go to the Sepolia Faucet:
   [https://sepolia-faucet.pk910.de](https://sepolia-faucet.pk910.de)

2. Open your MetaMask wallet

   * At the top, click the account dropdown
   * Copy your wallet address that you want the test ETH to go to

3. Paste your wallet address into the faucet

4. Complete any human verification / captcha required

5. Click Start Mining

6. Wait for ETH to accumulate

   * The faucet distributes ETH over time (not instantly)
   * Wait for the amount that you want to have (Minimum Claim Reward: 0.05 SepETH and Maximum Claim Reward: 2.5 SepETH)

7. To stop mining:

   * Click Stop Mining
   * Wait for the mined ETH to be transferred to your wallet

---

## Notes

* This step is only required if: 
    - Your wallet has no test ETH
    - Your ETH runs out later on one account want to mine more.
    - Optionally if you have other wallet that has test ETH on it you can transfer the fund between them.

---

## Verify Balance

1. You can check your wallet balance using Sepolia Etherscan by pasting your wallet address.

or

2. Just on MetaMask on thet wallet account
---
---

<br><br><br><br>
---


# Test Account Credentials
The following account has been pre-configured to simplify testing of blockchain-related features in this system.

> IMPORTANT: This account is provided strictly for testing and evaluation purposes. It is publicly shared.
---

##  Infura / MetaMask Account

* Email: `superadminintergrityvault@gmail.com`
* Password: `Qwerty!2`

---

## MetaMask Wallet

* Wallet Password: `Qwerty!2`

* Secret Recovery Phrase:

```text
coyote unknown castle kid actress woman gossip system sketch smoke choice phone
```

---

## Security Warning

* This wallet is NOT SECURE and may already be compromised.
* Do NOT store real funds in this account.
* Do NOT reuse this wallet for personal or production use.
* Anyone with access to this document can control this wallet.

---
---

<br><br><br><br>

# Add Hospital

This section explains how to onboard a new hospital into the system.

---

## Step 1: Create a Wallet for the Hospital

1. Open MetaMask
2. Click the account selector (top center)
3. Click Create Account
4. Give it a name (e.g., *Hospital A*)

---

## Step 2: Copy Wallet Details

For the newly created hospital account:

* Copy the Wallet Address
* Export the Private Key

> This wallet represents the hospital on the blockchain.

---

## Step 3: Log in as Super Admin

### If if you don’t remember it

In SSMS, run:

```sql
SELECT * 
FROM [IntegritryVaultDb].[dbo].[Users] 
WHERE Role = 4;
```

* `Role = 4` = Super Admin
* From the result, note:

  * Email
  or
  * Username

> Use the Email or Username and password to login as a superadmin in the frontend



#### Important: Super Admin Uniqueness

There should only be ONE Super Admin account in the system.

If you find multiple Super Admin accounts, clean them up using the steps below:

#### Step 1: Validate Against Configured Wallet

i) Open backend configuration:

```
IntegrityVault.Api -> appsettings.json
```

ii) Locate:

```json
"Blockchain": {
  "SuperAdminWalletAddress": "0x..."
}
```

iii) Compare this wallet address with the `WalletAddress` field in the database

iv) Delete all Super Admin accounts where the wallet address does NOT match the configured `SuperAdminWalletAddress`

---

#### Step 2: Recreate and Verify

i) Log in with each Super Admin account

ii) Attempt to add hospital from the frontend

iii) Only one Super Admin account should successfully allow hospital creation

iv) Keep that working account and delete all other Super Admin accounts



<br>

### If you don’t remember the password

#### Option A:

If using the pre-configured account, try the default password:
```text
Qwerty!2
```

#### Option B

You should NOT directly edit the password in the database unless you fully understand the hashing mechanism (you’ll likely break login if you do).

Instead:

### Recommended: Use backend API

i) Run the backend

ii) Locate the endpoint:

```
POST /api/User/superadmin
```

iii) Use it to reset/patch the password

* Only update the password field
* Example (conceptually):

```json
{
  "password": "Qwerty!2"
}
```

> Use the Email or Username and the newly set password to login as a superadmin in the frontend

<br>

### If there is no superadmin account:
#### Option 1 (Recommended): Check existing configuration

i) Open backend configuration:

   ```
   IntegrityVault.Api -> appsettings.json
   ```

ii) Locate:

   ```json
   "Blockchain": {
     "SuperAdminWalletAddress": "0x..."
   }
   ```

iii) Copy this wallet address

iv) Open MetaMask:

   * Search accounts
   * Find the account matching this wallet address
   * Switch to it

v) Create a new superadmin account with the new information

    Follow:
    Section 4 -> Step 8


<br>

#### Option B: Retrieve Wallet from .env

i) Go to the folder:

   ```
   integrity-vault-chain
   ```

ii) Open the `.env` file

iii) You will see something like:

   ```env
   SEPOLIA_RPC_URL=https://sepolia.infura.io/v3/fbb3b60c206748f59acefab2460f2027
   PRIVATE_KEY=0x275cd15a0e1a47e773882951ac4213983e43130692b42be30fdfd06cf5903fcb
   ```

iv) Copy the `PRIVATE_KEY`

v) Import it into MetaMask to retrieve the wallet address

vi) Create a new superadmin account with the new information

    Follow:
    Section 4 -> Step 8

Warning:
This only works if the private key has not been changed after the last contract deployment. The superadmin is typically the account that deployed the contract, so if the key was rotated or replaced, this method will not recover the correct wallet.


### If wallet is missing or unknown

####  Solution A : Redeploy Contract

* Redeploy smart contract
  -> Section 4: Step 5

* Update Backend Configuration
  -> Section 4: Step 6

* Delete old Super Admin from database directly on SSMS (Locate the user account where role is set to 4)

* Recreate Super Admin
  → Section 4 → Step 8

<br>

## Solution B (Force Change Ownership – Sepolia)

The smart contract includes a recovery function:

```solidity
forceTransferOwnership()
```

This can be used to manually transfer ownership on the Sepolia Testnet.


### Steps to Execute

#### i) Go to Hardhat Project

Open Visual Studio Code terminal:

```bash
cd <PATH_TO_ROOT_FOLDER>\integrity-vault.chain
```


#### ii) Ensure `.env` is Configured

```env
SEPOLIA_RPC_URL=https://sepolia.infura.io/v3/<YOUR_INFURA_PROJECT_ID>
PRIVATE_KEY=0x<YOUR_WALLET_PRIVATE_KEY>
```
> This wallet = the one that will execute the ownership change


#### iii) Create Script

Create a file (e.g. `forceTransfer.js` inside `scripts/`):

```javascript
import pkg from "hardhat";
const { ethers } = pkg;

async function main() {
  const contractAddress = "0x<YOUR_CONTRACT_ADDRESS>";

  const Contract = await ethers.getContractFactory("HospitalManagement");
  const contract = Contract.attach(contractAddress);

  const tx = await contract.forceTransferOwnership();
  await tx.wait();

  console.log("Ownership transfer executed");
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
```



#### iv) Run Script on Sepolia

```bash
npx hardhat run scripts/forceTransfer.js --network sepolia
```


#### v) After executing force transfer:

* Update Backend Configuration
  -> Section 4: Step 6

* Delete old Super Admin from database directly on SSMS (Locate the user account where role is set to 4)

* Recreate Super Admin
  → Section 4 → Step 8


---

## Step 4: Create Hospital

1. Navigate to the Create Hospital feature 

2. Enter required details:

   * Hospital name
   * Wallet address (from MetaMask)

3. Submit the request

---

## Step 5: Create Hospital Admin

Immediately after creating the hospital:

1. Create an Admin user for that hospital
2. Use:

   * Same Hospital ID
   * Valid email/password

> This step is critical — without an admin, the hospital cannot operate.
---

## Notes

* Each hospital should have:

  * Its own MetaMask wallet
  * At least one admin account

* Do NOT reuse wallets between hospitals

* Ensure the wallet has Sepolia ETH for transactions

---

## Recommendation

For clean system structure:

* 1 Wallet = 1 Hospital
* 1+ Admins per Hospital

---
---

<br><br><br><br>



# Section 1 Setup Environment
## Step 1: System Specification

The project was developed and tested using the following environment:

a) Operating System

* Windows 11 Home (Version 25H2, OS Build 26200.8117)
* Windows PowerShell (Version 5.1.26100.8115)

b) Development Tools

* Visual Studio Code (v1.114.0)
* Visual Studio 2022 Community (v17.12.4)

c) Frontend

* Angular CLI (v20.3.16)
* Node.js (v22.16.0)
* npm (v10.9.2)

d) Backend

* ASP.NET Core Web API (.NET 9.0)

e) Database

* SQL Server 2022 (16.0.1000.6, Developer Edition)
* SQL Server Management Studio (SSMS v20.2)

f) Storage

* IPFS (Kubo v0.40.1)

---

## Step 2: Install Required Software

Before running the project, install the following software on your machine:
   - Visual Studio 2022 Community
   - Visual Studio Code
   - SQL Server 2022 (Developer Edition)
   - SQL Server Management Studio (SSMS)

The versions used during development are listed in Step 1. It is recommended to install the same versions if possible. However, if those versions are not available, you can install the latest versions, as they should still be compatible.

<br>

### Important (Visual Studio Setup)
When installing Visual Studio 2022, make sure to select the required workloads:
   - ASP.NET and web development
   - Ensure that .NET 9.0 SDK is installed
Without these components, the backend project will not run correctly.

<br>

### Important (SQL Server Password)

* During SQL Server 2022 installation, you will be asked to create a password for your SQL Server account.
* Remember this password carefully — if you forget it, there is no easy recovery. 

---

## Step 3: Node.js, npm, Angular CLI, and Hardhat


a)  Check Node.js

* Open Visual Studio Code.
* Open the terminal (`Ctrl + ` or Terminal -> New Terminal)
* Run:

  ```bash
  node -v
  ```
* If the version is 22.16.0, continue.
* If it’s different or Node.js is not installed, download Node.js v22.16.0 from [nodejs.org](https://nodejs.org/en/download/), run the installer, and ensure Add to PATH is selected. Check version again:

  ```bash
  node -v
  ```
<br>

b)  Check npm

* In the Visual Studio Code terminal, run:

  ```bash
  npm -v
  ```
* The npm version must be exactly 10.9.2.
* Case 1: Not installed – install it manually:

  ```bash
  npm install -g npm@10.9.2
  ```
* Case 2: Installed but version does not match 10.9.2 – reinstall the correct version:

  ```bash
  npm install -g npm@10.9.2
  ```
* Case 3: Installed and version matches 10.9.2 – do nothing.

<br>

c)  Install or Verify Angular CLI

* Check if Angular CLI is installed:

  ```bash
  ng version
  ```
* Case 1: Not installed – run:

  ```bash
  npm install -g @angular/cli@20.3.16
  ```
* Case 2: Installed but version does not match 20.3.16 – first uninstall, then install:

  ```bash
  npm uninstall -g @angular/cli
  npm install -g @angular/cli@20.3.16
  ```
* Case 3: Installed and version matches 20.3.16 – do nothing.
* Verify installation:

  ```bash
  ng version
  ```
<br>

d)  Install or Verify Hardhat

* Make sure you are in the project folder in Visual Studio Code terminal.
* Check Hardhat version:

  ```bash
  npx hardhat --version
  ```
* Case 1: Not installed – run:

  ```bash
  npm install --save-dev hardhat@2.28.6
  ```
* Case 2: Installed but version does not match 2.28.6 – first uninstall, then install:

  ```bash
  npm uninstall --save-dev hardhat
  npm install --save-dev hardhat@2.28.6
  ```
* Case 3: Installed and version matches 2.28.6 – do nothing.
* Verify installation:

  ```bash
  npx hardhat --version
  ```
<br>
<br>

> You can close this terminal after you are done with installation.

---

## Step 4: Open the Project

a) Open Project Root in Visual Studio Code

* Open Visual Studio Code.
* Go to File -> Open Folder…
* Select the root project folder, which is:

```
<PATH_TO_ROOT_FOLDER>\IntegrityVault
```

* Opening the root folder allows access to all sub-projects (frontend, backend, IPFS, etc.) from Visual Studio Code.
> Do not close Visual Studio Code after this step. Keeping it open will make it easier to run frontend commands and navigate the project later.

<br>

b) Open Backend Solution in Visual Studio 2022

* Open Visual Studio 2022 Community.
* Go to File -> Open -> Project/Solution…
* Navigate to the backend folder inside the root folder:

```
<PATH_TO_ROOT_FOLDER>\IntegrityVault\IntegrityVault.Backend
```

* Open the solution file:

```
IntegrityVault.Backend.sln
```

* This will load the backend project in Visual Studio with all projects and dependencies ready.
> Do not close Visual Studio after this step. Keeping it open ensures you can restore packages and build the backend without reopening the solution.

---


## Step 5: Restore Backend Dependencies

* Make sure the backend solution (`IntegrityVault.Backend.sln`) is open in Visual Studio 2022, not Visual Studio Code.
* Open the NuGet Package Manager Console in Visual Studio:

  * Go to Tools -> NuGet Package Manager -> Package Manager Console
* In the console, run:

```powershell
dotnet restore
```

* This will download and restore all NuGet packages for the solution, ensuring the backend is ready to build.

---

## Step 6: Connect to SQL Server in SSMS

a) Open SQL Server Management Studio (SSMS).

b) In the Connect to Server window:

   * Server type: Select Database Engine
   * Server name: Click the drop-down arrow and choose <Browse for more…>

     * In the new window, expand Database Engine.
     * SSMS will list all SQL Server instances it can find on your network or locally.
     * Pick the one you want to connect to.
   * Authentication: Choose SQL Server Authentication (if you set a SQL password) or Windows Authentication
   * Login: Enter your username (usually `sa` for SQL Authentication)
   * Password: Enter the password you created during installation

Remember both the server name and password. You will need them later, and forgetting them may require reinstalling SQL Server.

c) Click Connect.

d) Once connected, keep SSMS open for database setup and queries in the next steps.

---


## Step 7: Configure Database Connection

a) Make sure the backend solution (`IntegrityVault.Backend.sln`) is open in Visual Studio 2022, not Visual Studio Code.

b) In Visual Studio, open the IntegrityVault.Api project.

c) Open the `appsettings.json` file.

d)  Locate the ConnectionStrings section:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=SAAHIL;Database=IntegrityVaultDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

e) Update this connection string with your own SQL Server instance:

   * Server: Replace `SAAHIL` with your SQL Server instance name.
   * Database: Make sure it is exactly `IntegrityVaultDb` (no spaces, same spelling and capitalisation).
   * Authentication:

     * For SQL Authentication, use:

       ```json
       "DefaultConnection": "Server=<your_server>;Database=IntegrityVaultDb;User Id=<your_username>;Password=<your_password>;TrustServerCertificate=True;"
       ```
     * For Windows Authentication, keep `Trusted_Connection=True`.

f) Save the file.

> No extra spaces in the connection string. The database name must match `IntegrityVaultDb`.

---

## Step 8: Clean and Build the Solution

a) In Visual Studio, make sure IntegrityVault.Backend.sln is open.

b) Open the NuGet Package Manager Console (Tools -> NuGet Package Manager -> Package Manager Console).

c) Run the following commands one at a time:

```powershell
# Clean the solution.
dotnet clean

# Build the solution.
dotnet build
```

> This ensures all projects are compiled freshly and ready for migrations.

---

## Step 9: Run Database Migration

a) In the NuGet Package Manager Console, ensure:

   * Package source: set to All
   * Default project: set to IntegrityVault.Api

b) Run the migration update command first:

```powershell
dotnet ef database update --project IntegrityVault.Repository --startup-project IntegrityVault.Api
```

c) If it fails:

   * Navigate to the `IntegrityVault.Repository` folder and delete the Migrations folder.
   * Run the commands in this order:

```powershell
# Create migration
dotnet ef migrations add InitialCreate --project IntegrityVault.Repository --startup-project IntegrityVault.Api

# Apply migration
dotnet ef database update --project IntegrityVault.Repository --startup-project IntegrityVault.Api
```

> If it still fails, double-check your connection string in Step 7.

d) If migration succeeds:

   * Open SSMS.
   * Expand Databases -> you should see `IntegrityVaultDb`.
   * If it doesn’t appear, try Refresh, wait 1 minute, or close and restart SSMS.

>Once the database is visible, move on to the next step.

---


## Step 10: Install and Configure IPFS (Kubo)

a) Download Kubo

   * Go to the official IPFS Kubo releases: [https://github.com/ipfs/kubo/releases](https://github.com/ipfs/kubo/releases)

   * Download Kubo v0.40.1 for Windows (amd64):

     ```
     kubo_v0.40.1_windows-amd64.zip
     ```

   > Make sure it is v0.40.1, the same version used in development.

b) Extract Kubo

   * Extract the contents of the ZIP file.

   * Move the extracted files (`ipfs.exe`, `install.sh`, `README.md`, LICENSE files, etc.) directly into your project folder:

     ```
     <PATH_TO_ROOT_FOLDER>\integrity-vault.ipfs
     ```

   * After extraction, your folder should look EXACTLY like this:

```text
<PATH_TO_ROOT_FOLDER>\integrity-vault.ipfs
├─ node1
├─ node2
├─ node3
├─ build-log
├─ install.sh
├─ ipfs.exe
├─ LICENSE
├─ LICENSE-APACHE
├─ LICENSE-MIT
├─ README.md
├─ setup-ipfs-nodes.ps1
├─ start-ipfs-nodes.ps1
├─ stop-ipfs-nodes.ps1
└─ swarm.key
```

> It is important that `ipfs.exe` is at the root of `integrity-vault.ipfs` along with your existing PS1 scripts. Do not leave it inside a nested folder from the ZIP.

c) Verify IPFS Installation

   * In Visual Studio Code.
   * Open a new terminal.
   * Navigate to the `integrity-vault.ipfs` folder.
   * Run command:

```powershell
.\ipfs.exe --version
```

* It should display:

```
ipfs version 0.40.1
```
<br>
> You can close this terminal after you are done with verfication.

---


## Step 11: Set Up Angular Frontend & Hardhat Project

### a) Angular Frontend

i) Open the project in Visual Studio Code

* Open Visual Studio Code.
* Open the terminal (`Ctrl + ` or Terminal -> New Terminal).
* Navigate to your Angular frontend folder. For example:

```bash
cd <PATH_TO_ROOT_FOLDER>\integrity-vault.web\
```

ii) Install dependencies

* In the Visual Studio Code terminal, run:

```bash
npm install
```

> This will create a `node_modules` folder with all required packages.

### b) Hardhat Project

i) Navigate to Hardhat project folder

* Open a new terminal in Visual Studio Code (or use the same one).
* Go to your Hardhat project folder:

```bash
cd <PATH_TO_ROOT_FOLDER>\integrity-vault.chain\
```

ii) Install dependencies

* Run:

```bash
npm install
```

> This will install Hardhat, Ethers.js, and all project plugins in `node_modules`.

iii) Compile Solidity contracts

* Run:

```bash
npx hardhat compile
```
<br>
> You can now close the terminal if you don’t need to run scripts immediately.

---

## Step 12: Run SQL Script in SSMS

i) Open SQL Server Management Studio (SSMS)

* Open SSMS if it’s not already open (from Step 6).
* Connect to your SQL Server instance (the same one you configured in Step 7).


ii) Create a new query window

* In SSMS, click New Query on the toolbar.
* Make sure the correct database is selected from the drop-down (for example, `master` or your `IntegrityVaultDb` depending on the script).


iii) Copy and paste the SQL script

* Open the text file containing the SQL commands.
* Select all content (`Ctrl + A`) and copy it (`Ctrl + C`).
* Go to SSMS query window and paste the content (`Ctrl + V`).


iv) Run the query

* Click Execute or press `F5`.
* Wait until the messages panel shows success message.

> This will create any tables, stored procedures, or initial data required by the project.

--- 
---

<br><br><br><br>

# Section 2 Run the Project

## Step 1: Check Internet Connection

* Ensure your machine has a stable internet connection. Some services (Blockchain services)require access to online resources.


---

## Step 2: Free Required Ports

* The project requires the following ports to be free. Verify no other application is using them:

```
4002, 4003, 4004, 4200, 5002, 5003, 5088, 7018, 8082, 8083, 8084
```

* Tip: You can check which ports are in use with PowerShell:

```powershell
netstat -ano | findstr :<PORT_NUMBER>
```

* If a port is occupied, stop the conflicting process before proceeding.


---

## Step 3: Ensure SQL Server is Running

* Open SQL Server Management Studio (SSMS).
* Connect to your configured SQL Server instance (used in Section 1 Part 7).
> DO NOT STOP THE CONNECTION WHEN YOU ARE TRYING TO RUN THE PROJECT.

---

## Step 4: Start Backend in Visual Studio

* Ensure is `IntegrityVault.Backend.sln` open in Visual Studio 2022.
* Select the `IntegrityVault.Api` project as the startup project.
* Enable HTTPS (this is usually the default, but check the launch settings).
* Start the project (press `F5` or click Start Debugging).

> Note: Ensure the backend starts successfully before running frontend or IPFS.

>DO NOT CLOSE THAT TERMINAL WHEN YOU ARE TRYING TO RUN THE PROJECT.

---

## Step 5: Start IPFS Nodes

* Open Visual Studio Code with the project folder loaded.
* Open a new terminal and navigate to your IPFS folder:

```bash
cd <PATH_TO_ROOT_FOLDER>\integrity-vault.ipfs
```

* Run the script to start IPFS nodes:

```powershell
./start-ipfs-nodes.ps1
```

* Allow any pop up network specific pop up that you might be prompted to.
* Wait until all nodes are started. You should see node IDs and connect” status.

> DO NOT CLOSE THAT TERMINAL WHEN YOU ARE TRYING TO RUN THE PROJECT.
---

## Step 6: Start Angular Frontend

* Still in Visual Studio Code.
* Open another terminal in Visual Studio Code.
* Navigate to the Angular frontend folder:

```bash
cd <PATH_TO_ROOT_FOLDER>\integrity-vault.web
```

* Start the frontend server:

```bash
ng serve --port 4200 --open
```

* Wait until Angular successfully compiles. Open a browser and go to:
```
http://localhost:4200
```
> DO NOT CLOSE THAT TERMINAL WHEN YOU ARE TRYING TO RUN THE PROJECT.

## Step 7: IMPORTANT – IP Access Restriction (Login Issues)

If you attempt to log in as a Doctor, Hospital Admin, or External Provider and get an "Invalid Credentials" error—even if the username and password are correct—it is because of the IP Security Filter.

The Reason

To maintain medical security, these users can only log in if their current IP address is authorised for their specific hospital. Since most internet connections use Dynamic IPs, your IP may have changed since the project was last configured.

The Fix

Go to [https://whatismyipaddress.com/](https://whatismyipaddress.com/) and copy your IPv4 or IPv6.

Log in to the frontend as the Super Admin.

Navigate to the Hospital section.

Find the hospital associated with the account you are trying to use

Click edit.

Add your current IP address to that hospital’s Authorised IP list.

You will now be able to log in successfully as the Doctor, Hospital Admin, or External Provider

---
---

<br><br><br><br>

# Section 3: Stop the Project


## Step 1: Stop the Backend (C# / ASP.NET Core)

* Go to Visual Studio 2022 where `IntegrityVault.Api` is running.
* Make sure all ongoing transactions are complete.
* Stop the backend by either:

  * Pressing Shift + F5 or clicking Stop Debugging, or
  * Closing Visual Studio (if running in debug mode).

> Warning: Stopping the backend while transactions are in progress can cause incomplete data or errors.

---

## Step 2: Stop IPFS Nodes

There are two ways to safely stop IPFS:

### Option A: Use the Original Terminal

* In the terminal where you ran:

```powershell
./start-ipfs-nodes.ps1
```

* Type:

```text
YES
```

* This will gracefully stop all running IPFS nodes.

### Option B: Use a New Terminal

* Open a new terminal in Visual Studio Code.
* Navigate to your IPFS folder:

```bash
cd <PATH_TO_ROOT_FOLDER>\integrity-vault.ipfs
```

* Run the stop script:

```powershell
./stop-ipfs-nodes.ps1
```

* You will be asked to confirm. Type `YES` to stop all nodes.
* Wait until the terminal indicates that nodes are offline.

> Tip: Check Task Manager for any running `ipfs.exe` processes to confirm all nodes are stopped.

---

## Step 3: Stop Angular Frontend

* Simply close the terminal where `ng serve --port 4200 --open` is running.
* There is no special shutdown command; Angular stops when the terminal is closed.

> Make sure any unsaved frontend work is saved before closing the terminal.

---
---

<br><br><br><br>

# Section 4: Fresh Start (Complete Reset)

## Step 1: Clean and Re-run Setup

Before re-running the setup, determine your situation:

### Case 1: First-Time Setup

* If you are running the project for the first time:

  * Simply follow Section 1: Setup Environment (Step 1 -> Step 11).

> No cleanup is required.



### Case 2: Existing Setup (Already Used the System)

* If you have previously run the project, you must remove old data before starting fresh.

#### a) Delete Existing Database

* Open SQL Server Management Studio (SSMS).
* Connect to your SQL Server instance.
* Expand Databases.
* Locate:

```
IntegrityVaultDb
```

* Right-click -> Delete -> Confirm deletion.

> Warning: This will permanently delete all stored data.


#### b) Re-run Setup

* After deleting the database:

  * Go back and follow Section 1: Setup Environment (Step 4 -> Step 11).

This will:

* Recreate the database
* Reapply migrations
* Restore a clean system state

---

## Step 2: Install MetaMask

MetaMask is required to interact with the blockchain features of the system.


### a) Download MetaMask

* Open the official MetaMask download page:
  [https://metamask.io/en-GB/download](https://metamask.io/en-GB/download)

* Select your preferred browser (e.g., Chrome, Edge, Firefox).

* Click Install MetaMask for <Browser>.


### b) Install the Extension

* You will be redirected to your browser’s extension store.
* Click Add to Browser / Install.
* Confirm installation when prompted.


### c) Set Up MetaMask

* After installation, open MetaMask from your browser extensions.

* Choose one of the following:

  * Create a new wallet, or
  * Import an existing wallet (if you already have one)

* Follow the on-screen instructions:

  * Set a password
  * Securely save your recovery phrase

### d) Configure Sepolia Test Network

As of now, the easiest way to enable Sepolia is:

1. Open MetaMask
2. Click the three horizontal lines (☰) in the top corner
3. Go to Settings -> Networks
4. Enable “Show test networks”
5. Click the three vertical dots (⋮) next to Sepolia (NOT Linea Sepolia) the network
6. Click Edit
7. Click Save

> This will switch your wallet to the Sepolia test network.

---

## Step 3: Set Up Infura


### a) Go to Infura

* Open the Infura website: [https://www.infura.io/](https://www.infura.io/)


### b) Sign Up / Log In

1. Click Get Started.
2. Continue with your MetaMask wallet.
3. Link your MetaMask wallet to an Infura account.
   * Tip: Linking with a Google account often works best.
4. Complete the signup process.
5. When asked for a plan, select the Free Tier.


### c) Find Ethereum Sepolia RPC Endpoint

1. On the sidebar, click Endpoints -> Infura RPC.
2. Find the Ethereum Sepolia endpoint under Active Endpoints.
3. Copy the URL — this is your RPC URL.

> Keep this URL handy — you will use it in your Hardhat configuration and possibly your backend project to connect to Sepolia.

---

## Step 4: Get Sepolia Test ETH

### Option 1: Use the Sepolia Faucet

1. Go to the Sepolia Faucet: [https://sepolia-faucet.pk910.de](https://sepolia-faucet.pk910.de)
2. Open your MetaMask wallet -> On top there is drop down with all the account. Click on it and copy the ETH wallet addresss.
3. Paste your wallet address into the faucet.
4. Complete any human verification / captcha required.
5. Click Start Mining.
6. Wait for ETH to accumulate
    - The faucet works by mining test ETH over time, not instant transfer.
    - Wait for at least 2.5 test ETH.
7. To stop minning, click the stop mining button and wait for the test ETH to hit your waalet account

---

### Option 2: Use Provided Test Account

* The Sepolia test account will already provided for testing purposes. You can try to log into that account if to see if still has eth. Keep in mind there is a risk that acount might have been compromised as the account info is public.
1. Log in to the provided MetaMask account.
2. Ensure the network is set to Sepolia.
3. Copy your personal wallet address (your own MetaMask account) [Open your MetaMask wallet -> On top there is drop down with all the account. Click on it and copy the ETH wallet addresss.].
4. From the burner account, send a small amount of Sepolia ETH to your wallet.
5. Switch back to your wallet and confirm the ETH has been received.

---

## Step 5: Deploy Blockchain Contracts Using Hardhat (Sepolia)

### a) Get Your Wallet Private Key

1. Open MetaMask.
2. Click the account name -> shows a list of accounts.
3. Select the account to use for deployment.
4. Click the three vertical dots -> Account Details -> Export Private Key.
5. Enter your MetaMask password and copy the private key.

> Keep this key safe — anyone with it can access your funds. You will need it for deployment.

<br>

### b) Configure Hardhat Environment

i) Open Visual Studio Code.
ii) Navigate to the Hardhat project folder:

```bash
cd <PATH_TO_ROOT_FOLDER>\integrity-vault.chain
```

iii) Open the `.env` file and set:

```env
SEPOLIA_RPC_URL=https://sepolia.infura.io/v3/<YOUR_INFURA_PROJECT_ID>
PRIVATE_KEY=0x<YOUR_PRIVATE_KEY>
```

* Replace `SEPOLIA_RPC_URL` with your Infura Sepolia RPC URL
* Replace `PRIVATE_KEY` with your wallet private key


c) Verify Hardhat Configuration

Ensure `hardhat.config.js` includes Sepolia network:

```javascript
import "@nomicfoundation/hardhat-toolbox";
import * as dotenv from "dotenv";
dotenv.config();

export default {
  solidity: "0.8.28",
  networks: {
    hardhat: {
      accounts: {
        mnemonic: "test test test test test test test test test test test junk",
        count: 4,
        path: "m/44'/60'/0'/0/",
        initialIndex: 0,
        balance: "1000000000000000000000000000000000000000000"
      }
    },
    sepolia: {
      url: process.env.SEPOLIA_RPC_URL,
      accounts: [process.env.PRIVATE_KEY]
    }
  }
};
```


### d) Deployment Script

`deploy.js` should look like this:

```javascript
import pkg from "hardhat";
const { ethers } = pkg;

async function main() {
  const [owner] = await ethers.getSigners();

  console.log("Superadmin (owner):", owner.address);

  const Registry = await ethers.getContractFactory("HospitalManagement", owner);
  const registry = await Registry.deploy();

  await registry.waitForDeployment();

  const address = await registry.getAddress();
  console.log("HospitalManagement deployed to:", address);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
```


### e) Deploy to Sepolia

i) In the Visual Studio Code terminal, ensure you are in the Hardhat folder:

```bash
cd <PATH_TO_ROOT_FOLDER>\integrity-vault.chain
```

ii) Run the deployment command with environment variables:

```bash
 npx hardhat run scripts/deploy.js --network sepolia
```

iii) Wait for compilation and deployment.

iv) Check output:

```
Superadmin (owner): 0x...
HospitalManagement deployed to: 0x<CONTRACT_ADDRESS>
```

> Remember this contract address in your backend.

---

## Step 6: Update Backend Configuration

i) Ensure IntegrityVault.Backend.sln is open in Visual Studio 2022.

ii) Open the IntegrityVault.Api project.

iii) Locate the appsettings.json file.

iv) Update the Blockchain section with the new deployment info:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=<YOUR_SQL_SERVER>;Database=IntegrityVaultDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "Is_this_at_least_32_characters_long_?",
    "Issuer": "IntegrityVaultAPI"
  },
  "Crypto": {
    "MasterKey": "I_am_exactly_32_chars_long!HAHA!"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Blockchain": {
    "RPC_URL": "https://sepolia.infura.io/v3/<YOUR_INFURA_PROJECT_ID>",
    "ContractAddress": "<YOUR_DEPLOYED_CONTRACT_ADDRESS>",
    "SuperAdminWalletAddress": "<YOUR_WALLET_ADDRESS_USED_FOR_DEPLOYMENT>"
  },
  "AllowedHosts": "*"
}
```

> Replace placeholders with your actual values:
>
> * `<YOUR_INFURA_PROJECT_ID>` -> your Infura Sepolia RPC URL
> * `<YOUR_DEPLOYED_CONTRACT_ADDRESS>` -> the address printed when you deployed the contract
> * `<YOUR_WALLET_ADDRESS_USED_FOR_DEPLOYMENT>` -> the wallet address used to deploy the contract

v) Save the appsettings.json file.
vi) Open the NuGet Package Manager Console in Visual Studio:
* Go to Tools -> NuGet Package Manager -> Package Manager Console

```powershell
# Run each code one at a time.
dotnet clean
dotnet build
```

> This ensures the backend is updated with the new blockchain connection and is ready to run.

---

## Step 7: Reconfigure IPFS (Clean Reset of Old Data)


### a) Open Visual Studio Code Terminal

1. Open Visual Studio Code
2. Open a new terminal:

   * `Terminal -? New Terminal`

---

### b) Navigate to IPFS Folder

Run:

```bash
cd <PATH_TO_ROOT_FOLDER>\integrity-vault.ipfs
```

---

### c) Run IPFS Reset Setup Script

Execute the reset script:

```powershell
./setup-ipfs-folder.ps1
```

---

### d) Confirm Reset Operation

When prompted, type:

```text
YES
```

---

## Step 8: Initialise the System (Create Super Admin)

After successfully starting the backend, you must create the Super Admin account. This account has full system privileges and is required to access administrative features.

### a) Run the Backend

i) Open Visual Studio 2022.
ii) Ensure `IntegrityVault.Api` is set as the startup project.
iii) Make sure that this is running on https
iv) Click Start Debugging (`F5`) or press the Run button.
v) Wait for the application to start.

> Once the backend is running, Swagger UI will automatically open in your browser.



### b) Open Swagger

* If Swagger does not open automatically, navigate to:

```
https://localhost:<PORT>/swagger
```


### c) Locate the Super Admin Endpoint

i) In Swagger, find the endpoint:

```
POST /api/User/superadmin
```

ii) Expand the endpoint.
iii) Click Try it out.


### d) Create the Super Admin Account

i) Fill in the required fields in the request body.

ii) Wallet address (IMPORTANT):
   - You MUST use the SAME wallet address that deployed the smart contract.
   - This is required because the smart contract restricts hospital creation to the contract owner only.
   - If a different wallet is used, hospital onboarding will fail on-chain.

iii) Private key:
   - Must correspond to the deployment wallet (contract owner wallet).
   - This is required for signing blockchain transactions.

iv) Hospital ID:
   - Can be set to `1` or any placeholder value.
   - The backend will treat this account as system-level and may override it to `NULL`.


### e) Execute the Request

1. Click Execute.
2. Ensure the response indicates success (e.g., HTTP 200 or 201).


### f) Important Clarification on Multiple Super Admins

This Super Admin account is the first and most powerful account in the system.
Although the system may technically allow creating multiple Super Admin records in the backend database, only one wallet (the contract owner wallet) has actual blockchain authority.

- The smart contract restricts privileged operations (e.g. adding hospitals) to the contract owner wallet only, which is assigned to the Super Admin role in this system.
- Super Admin can also add admins of hospital from the frontend
- Each Super Admin must have a unique wallet address.
- Only the Super Admin linked to the contract owner wallet can perform blockchain-level administrative actions such as hospital onboarding.

> As a result, creating additional Super Admin accounts does not provide any additional blockchain privileges and is effectively redundant from a system authority perspective.

> Store the credentials securely. Losing this account may result in loss of administrative access.


Your “testing section” is clear in intent, but it’s a bit rough and inconsistent compared to the structured style of the rest of your document. Here’s a cleaner, professional version that matches your earlier sections and is easier for others to follow.

---
---

<br><br><br><br>

# Section 5 Running Tests

## Step 1: Run Frontend Tests (Angular)

i) Open Visual Studio Code.

ii) Open the frontend project folder:

```bash
cd <PATH_TO_ROOT_FOLDER>\integrity-vault.web\
```

iii) Run the Angular test command:

```bash
npm test
```

or

```bash
ng test
```

iv) The test runner will start automatically.

v) To stop the tests:

   * Press `Ctrl + C`, or
   * Close the terminal.

---

## Step 2: Run Blockchain Tests (Hardhat)

i) Open Visual Studio Code.

ii) Navigate to the blockchain project folder:

```bash
cd <PATH_TO_ROOT_FOLDER>\integrity-vault.chain\
```

iii) Run the Hardhat test command:

```bash
npx hardhat test
```

iv) Wait for all smart contract tests to execute.

---

## Step 3: Run Backend Tests (.NET)

i) Open Visual Studio 2022.

ii) Open the solution file in <PATH_TO_ROOT_FOLDER>\IntegrityVault.Backend:

```
IntegrityVault.Backend.sln
```

3. Open the NuGet Package Manager Console:

   * Go to: `Tools → NuGet Package Manager → Package Manager Console`

4. Run the test command:

```powershell
dotnet test
```

5. Review the test results in the console output.

---

## Notes

* Ensure all dependencies are installed before running tests:

  * `npm install` (frontend)
  * `npm install` (Hardhat project)
  * `.NET restore` (backend, if needed)

  > See Section 1 step 3 and Section 1 setp 5 for deatil
