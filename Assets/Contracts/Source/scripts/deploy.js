const hre = require("hardhat");

async function main() {
  console.log("Deploying OmniWorld Smart Contracts...");
  console.log("Network:", hre.network.name);
  console.log("");

  // Get deployer account
  const [deployer] = await hre.ethers.getSigners();
  console.log("Deploying with account:", deployer.address);
  
  const balance = await deployer.provider.getBalance(deployer.address);
  console.log("Account balance:", hre.ethers.formatEther(balance), "MATIC");
  console.log("");

  // Deploy OmniLandNFT (ERC-721)
  console.log("Deploying OmniLandNFT...");
  const OmniLandNFT = await hre.ethers.getContractFactory("OmniLandNFT");
  
  // Use deployer address as initial royalty receiver (can be changed later to treasury)
  const landNFT = await OmniLandNFT.deploy(deployer.address);
  await landNFT.waitForDeployment();
  const landNFTAddress = await landNFT.getAddress();
  
  console.log("✓ OmniLandNFT deployed to:", landNFTAddress);
  console.log("");

  // Deploy OmniItemsNFT (ERC-1155)
  console.log("Deploying OmniItemsNFT...");
  const OmniItemsNFT = await hre.ethers.getContractFactory("OmniItemsNFT");
  const itemsNFT = await OmniItemsNFT.deploy();
  await itemsNFT.waitForDeployment();
  const itemsNFTAddress = await itemsNFT.getAddress();
  
  console.log("✓ OmniItemsNFT deployed to:", itemsNFTAddress);
  console.log("");

  // Save deployment addresses
  const deploymentInfo = {
    network: hre.network.name,
    chainId: hre.network.config.chainId,
    deployer: deployer.address,
    contracts: {
      OmniLandNFT: landNFTAddress,
      OmniItemsNFT: itemsNFTAddress
    },
    timestamp: new Date().toISOString()
  };

  console.log("Deployment Summary:");
  console.log(JSON.stringify(deploymentInfo, null, 2));
  console.log("");

  // Save to file
  const fs = require("fs");
  const deploymentPath = \`./deployments/\${hre.network.name}.json\`;
  
  // Create deployments directory if it doesn't exist
  if (!fs.existsSync("./deployments")) {
    fs.mkdirSync("./deployments");
  }
  
  fs.writeFileSync(deploymentPath, JSON.stringify(deploymentInfo, null, 2));
  console.log("✓ Deployment info saved to:", deploymentPath);
  console.log("");

  // Verification instructions
  if (hre.network.name !== "hardhat" && hre.network.name !== "localhost") {
    console.log("To verify contracts on PolygonScan, run:");
    console.log(\`npx hardhat verify --network \${hre.network.name} \${landNFTAddress} "\${deployer.address}"\`);
    console.log(\`npx hardhat verify --network \${hre.network.name} \${itemsNFTAddress}\`);
  }
}

main()
  .then(() => process.exit(0))
  .catch((error) => {
    console.error(error);
    process.exit(1);
  });
