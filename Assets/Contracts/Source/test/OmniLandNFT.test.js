const { expect } = require("chai");
const { ethers } = require("hardhat");

describe("OmniLandNFT", function () {
  let landNFT;
  let owner;
  let addr1;
  let addr2;

  beforeEach(async function () {
    [owner, addr1, addr2] = await ethers.getSigners();
    
    const OmniLandNFT = await ethers.getContractFactory("OmniLandNFT");
    landNFT = await OmniLandNFT.deploy(owner.address);
    await landNFT.waitForDeployment();
  });

  describe("Deployment", function () {
    it("Should set the right owner", async function () {
      expect(await landNFT.owner()).to.equal(owner.address);
    });

    it("Should have correct name and symbol", async function () {
      expect(await landNFT.name()).to.equal("OmniWorld Land");
      expect(await landNFT.symbol()).to.equal("OMNILAND");
    });
  });

  describe("Minting", function () {
    it("Should mint a new property NFT", async function () {
      const tx = await landNFT.mintProperty(
        addr1.address,
        "ipfs://test-metadata",
        "OmniLanta",
        "Residential",
        ethers.parseEther("1000")
      );

      expect(await landNFT.ownerOf(0)).to.equal(addr1.address);
    });
  });
});
