// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/security/Pausable.sol";

/**
 * @title OmniTransactionVerifier
 * @dev Security contract for validating transactions in the OMNI ecosystem
 * 
 * Features:
 * - Transaction validation and authentication
 * - Whitelist/blacklist functionality
 * - Transaction limits and rate limiting
 * - Fraud prevention mechanisms
 * 
 * This is an optional security layer that can be integrated with other contracts
 * to provide additional validation and fraud prevention.
 * 
 * Configuration:
 * - Developer: 0xCbBf46e4BFbcd099601D63482866EEC68Ebd8992
 * - Recovery: 0x81f5cfdD2851362E5986b26614517638Af89E514
 */
contract OmniTransactionVerifier is AccessControl, Pausable {
    // Roles
    bytes32 public constant ADMIN_ROLE = keccak256("ADMIN_ROLE");
    bytes32 public constant VERIFIER_ROLE = keccak256("VERIFIER_ROLE");

    // Transaction limits
    uint256 public maxTransactionAmount = 1000000 * 10**18; // 1M OMNICOIN default
    uint256 public dailyLimitPerAddress = 5000000 * 10**18; // 5M OMNICOIN daily
    uint256 public transactionCooldown = 60; // 60 seconds between txs

    // Address status tracking
    mapping(address => bool) public whitelist;
    mapping(address => bool) public blacklist;
    mapping(address => bool) public trustedContracts;
    
    // Transaction tracking
    mapping(address => uint256) public lastTransactionTime;
    mapping(address => uint256) public dailyTransactionVolume;
    mapping(address => uint256) public lastDailyReset;

    // Verification stats
    mapping(address => uint256) public transactionCount;
    uint256 public totalVerifications;

    // Events
    event AddressWhitelisted(address indexed account, uint256 timestamp);
    event AddressBlacklisted(address indexed account, uint256 timestamp);
    event AddressRemovedFromBlacklist(address indexed account, uint256 timestamp);
    event ContractTrusted(address indexed contractAddress, uint256 timestamp);
    event TransactionVerified(
        address indexed from,
        address indexed to,
        uint256 amount,
        bool approved,
        string reason
    );
    event LimitUpdated(string limitType, uint256 oldValue, uint256 newValue);

    /**
     * @dev Constructor
     */
    constructor(
        address _developerAddress,
        address _recoveryAddress
    ) {
        require(_developerAddress != address(0), "Invalid developer address");
        require(_recoveryAddress != address(0), "Invalid recovery address");

        // Grant roles
        _grantRole(DEFAULT_ADMIN_ROLE, _developerAddress);
        _grantRole(ADMIN_ROLE, _developerAddress);
        _grantRole(ADMIN_ROLE, _recoveryAddress);
        _grantRole(VERIFIER_ROLE, _developerAddress);

        // Automatically whitelist admin addresses
        whitelist[_developerAddress] = true;
        whitelist[_recoveryAddress] = true;
    }

    /**
     * @dev Verify a transaction
     * @param from Sender address
     * @param to Recipient address
     * @param amount Transaction amount
     * @return approved Whether the transaction is approved
     * @return reason Reason for approval/rejection
     */
    function verifyTransaction(
        address from,
        address to,
        uint256 amount
    ) external whenNotPaused returns (bool approved, string memory reason) {
        totalVerifications++;

        // Check if paused
        if (paused()) {
            emit TransactionVerified(from, to, amount, false, "Contract paused");
            return (false, "Contract paused");
        }

        // Check blacklist
        if (blacklist[from]) {
            emit TransactionVerified(from, to, amount, false, "Sender blacklisted");
            return (false, "Sender blacklisted");
        }

        if (blacklist[to]) {
            emit TransactionVerified(from, to, amount, false, "Recipient blacklisted");
            return (false, "Recipient blacklisted");
        }

        // Whitelisted addresses bypass all checks
        if (whitelist[from] || whitelist[to]) {
            transactionCount[from]++;
            emit TransactionVerified(from, to, amount, true, "Whitelisted");
            return (true, "Whitelisted");
        }

        // Trusted contracts bypass most checks
        if (trustedContracts[msg.sender]) {
            transactionCount[from]++;
            emit TransactionVerified(from, to, amount, true, "Trusted contract");
            return (true, "Trusted contract");
        }

        // Check transaction amount limit
        if (amount > maxTransactionAmount) {
            emit TransactionVerified(from, to, amount, false, "Amount exceeds max");
            return (false, "Amount exceeds maximum allowed");
        }

        // Check cooldown
        if (block.timestamp - lastTransactionTime[from] < transactionCooldown) {
            emit TransactionVerified(from, to, amount, false, "Cooldown active");
            return (false, "Transaction cooldown active");
        }

        // Check daily limit
        if (block.timestamp - lastDailyReset[from] >= 1 days) {
            // Reset daily counter
            dailyTransactionVolume[from] = 0;
            lastDailyReset[from] = block.timestamp;
        }

        if (dailyTransactionVolume[from] + amount > dailyLimitPerAddress) {
            emit TransactionVerified(from, to, amount, false, "Daily limit exceeded");
            return (false, "Daily transaction limit exceeded");
        }

        // Update tracking
        lastTransactionTime[from] = block.timestamp;
        dailyTransactionVolume[from] += amount;
        transactionCount[from]++;

        emit TransactionVerified(from, to, amount, true, "Verified");
        return (true, "Transaction verified");
    }

    /**
     * @dev Quick check if transaction would be approved (view function)
     */
    function canTransact(
        address from,
        address to,
        uint256 amount
    ) external view returns (bool) {
        if (paused()) return false;
        if (blacklist[from] || blacklist[to]) return false;
        if (whitelist[from] || whitelist[to]) return true;
        if (trustedContracts[msg.sender]) return true;
        if (amount > maxTransactionAmount) return false;
        if (block.timestamp - lastTransactionTime[from] < transactionCooldown) return false;
        
        uint256 currentDailyVolume = dailyTransactionVolume[from];
        if (block.timestamp - lastDailyReset[from] >= 1 days) {
            currentDailyVolume = 0;
        }
        
        if (currentDailyVolume + amount > dailyLimitPerAddress) return false;
        
        return true;
    }

    /**
     * @dev Add address to whitelist (admin only)
     */
    function addToWhitelist(address account) external onlyRole(ADMIN_ROLE) {
        whitelist[account] = true;
        emit AddressWhitelisted(account, block.timestamp);
    }

    /**
     * @dev Remove address from whitelist (admin only)
     */
    function removeFromWhitelist(address account) external onlyRole(ADMIN_ROLE) {
        whitelist[account] = false;
    }

    /**
     * @dev Add address to blacklist (admin only)
     */
    function addToBlacklist(address account) external onlyRole(ADMIN_ROLE) {
        blacklist[account] = true;
        emit AddressBlacklisted(account, block.timestamp);
    }

    /**
     * @dev Remove address from blacklist (admin only)
     */
    function removeFromBlacklist(address account) external onlyRole(ADMIN_ROLE) {
        blacklist[account] = false;
        emit AddressRemovedFromBlacklist(account, block.timestamp);
    }

    /**
     * @dev Mark a contract as trusted (admin only)
     */
    function setTrustedContract(address contractAddress, bool trusted) external onlyRole(ADMIN_ROLE) {
        trustedContracts[contractAddress] = trusted;
        if (trusted) {
            emit ContractTrusted(contractAddress, block.timestamp);
        }
    }

    /**
     * @dev Update maximum transaction amount (admin only)
     */
    function setMaxTransactionAmount(uint256 newMax) external onlyRole(ADMIN_ROLE) {
        uint256 oldMax = maxTransactionAmount;
        maxTransactionAmount = newMax;
        emit LimitUpdated("maxTransactionAmount", oldMax, newMax);
    }

    /**
     * @dev Update daily limit per address (admin only)
     */
    function setDailyLimitPerAddress(uint256 newLimit) external onlyRole(ADMIN_ROLE) {
        uint256 oldLimit = dailyLimitPerAddress;
        dailyLimitPerAddress = newLimit;
        emit LimitUpdated("dailyLimitPerAddress", oldLimit, newLimit);
    }

    /**
     * @dev Update transaction cooldown (admin only)
     */
    function setTransactionCooldown(uint256 newCooldown) external onlyRole(ADMIN_ROLE) {
        uint256 oldCooldown = transactionCooldown;
        transactionCooldown = newCooldown;
        emit LimitUpdated("transactionCooldown", oldCooldown, newCooldown);
    }

    /**
     * @dev Get address verification status
     */
    function getAddressStatus(address account) external view returns (
        bool isWhitelisted,
        bool isBlacklisted,
        uint256 txCount,
        uint256 currentDailyVolume,
        uint256 lastTxTime
    ) {
        isWhitelisted = whitelist[account];
        isBlacklisted = blacklist[account];
        txCount = transactionCount[account];
        
        // Calculate current daily volume
        if (block.timestamp - lastDailyReset[account] >= 1 days) {
            currentDailyVolume = 0;
        } else {
            currentDailyVolume = dailyTransactionVolume[account];
        }
        
        lastTxTime = lastTransactionTime[account];
    }

    /**
     * @dev Pause contract (emergency)
     */
    function pause() external onlyRole(ADMIN_ROLE) {
        _pause();
    }

    /**
     * @dev Unpause contract
     */
    function unpause() external onlyRole(ADMIN_ROLE) {
        _unpause();
    }

    /**
     * @dev Batch whitelist multiple addresses
     */
    function batchWhitelist(address[] calldata accounts) external onlyRole(ADMIN_ROLE) {
        for (uint256 i = 0; i < accounts.length; i++) {
            whitelist[accounts[i]] = true;
            emit AddressWhitelisted(accounts[i], block.timestamp);
        }
    }

    /**
     * @dev Batch blacklist multiple addresses
     */
    function batchBlacklist(address[] calldata accounts) external onlyRole(ADMIN_ROLE) {
        for (uint256 i = 0; i < accounts.length; i++) {
            blacklist[accounts[i]] = true;
            emit AddressBlacklisted(accounts[i], block.timestamp);
        }
    }
}
