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