using UnityEngine;
using System;
using System.Collections.Generic;

namespace OmniWorld.Economy
{
    /// <summary>
    /// Governance system for DAO voting and proposals
    /// Manages community decision-making and treasury allocation
    /// </summary>
    public class Governance : MonoBehaviour
    {
        private static Governance _instance;
        public static Governance Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Governance>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("Governance");
                        _instance = go.AddComponent<Governance>();
                    }
                }
                return _instance;
            }
        }

        [Header("Voting Parameters")]
        public int proposalDurationDays = 7;
        public float quorumPercentage = 20f; // Minimum 20% participation
        public float approvalThreshold = 66.7f; // 2/3 majority

        [Header("Treasury")]
        public float treasuryBalance = 600000000f; // 30% of 2B total supply
        
        private List<Proposal> activeProposals = new List<Proposal>();
        private int nextProposalId = 1;

        public event Action<Proposal> OnProposalCreated;
        public event Action<Proposal> OnProposalExecuted;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("Governance System Initialized");
            Debug.Log($"Treasury Balance: {treasuryBalance:N0} $OMNI");
        }

        /// <summary>
        /// Create a new governance proposal
        /// </summary>
        public Proposal CreateProposal(string title, string description, ProposalType type, float requestedAmount = 0)
        {
            Proposal proposal = new Proposal
            {
                id = nextProposalId++,
                title = title,
                description = description,
                proposalType = type,
                requestedAmount = requestedAmount,
                createdTime = Time.time,
                endTime = Time.time + (proposalDurationDays * 86400f),
                status = ProposalStatus.Active
            };

            activeProposals.Add(proposal);
            OnProposalCreated?.Invoke(proposal);
            
            Debug.Log($"New Proposal Created: {proposal.title}");
            
            return proposal;
        }

        /// <summary>
        /// Cast a vote on a proposal
        /// </summary>
        public bool CastVote(int proposalId, string walletAddress, bool voteYes, float votingPower)
        {
            Proposal proposal = activeProposals.Find(p => p.id == proposalId);
            
            if (proposal == null)
            {
                Debug.LogWarning($"Proposal {proposalId} not found");
                return false;
            }

            if (proposal.status != ProposalStatus.Active)
            {
                Debug.LogWarning($"Proposal {proposalId} is not active");
                return false;
            }

            if (Time.time > proposal.endTime)
            {
                proposal.status = ProposalStatus.Ended;
                Debug.LogWarning($"Proposal {proposalId} voting period has ended");
                return false;
            }

            // Record vote
            if (voteYes)
            {
                proposal.votesFor += votingPower;
            }
            else
            {
                proposal.votesAgainst += votingPower;
            }

            proposal.totalVotes += votingPower;
            proposal.voters.Add(walletAddress);

            Debug.Log($"Vote cast on proposal {proposalId}: {(voteYes ? "YES" : "NO")} ({votingPower} voting power)");
            
            return true;
        }

        /// <summary>
        /// Finalize a proposal and execute if passed
        /// </summary>
        public void FinalizeProposal(int proposalId)
        {
            Proposal proposal = activeProposals.Find(p => p.id == proposalId);
            
            if (proposal == null || proposal.status != ProposalStatus.Active)
                return;

            if (Time.time < proposal.endTime)
            {
                Debug.LogWarning("Voting period has not ended yet");
                return;
            }

            // Check quorum
            float participationRate = (proposal.totalVotes / 1000000f) * 100f; // Simplified calculation
            
            if (participationRate < quorumPercentage)
            {
                proposal.status = ProposalStatus.Failed;
                Debug.Log($"Proposal {proposalId} failed: Did not reach quorum ({participationRate:F1}%)");
                return;
            }

            // Check approval threshold
            float approvalRate = (proposal.votesFor / proposal.totalVotes) * 100f;
            
            if (approvalRate >= approvalThreshold)
            {
                proposal.status = ProposalStatus.Passed;
                ExecuteProposal(proposal);
            }
            else
            {
                proposal.status = ProposalStatus.Failed;
                Debug.Log($"Proposal {proposalId} failed: Did not reach approval threshold ({approvalRate:F1}%)");
            }
        }

        /// <summary>
        /// Execute a passed proposal
        /// </summary>
        private void ExecuteProposal(Proposal proposal)
        {
            Debug.Log($"Executing proposal {proposal.id}: {proposal.title}");

            switch (proposal.proposalType)
            {
                case ProposalType.TreasuryAllocation:
                    if (proposal.requestedAmount <= treasuryBalance)
                    {
                        treasuryBalance -= proposal.requestedAmount;
                        Debug.Log($"Treasury allocation approved: {proposal.requestedAmount:N0} $OMNI");
                    }
                    break;

                case ProposalType.ParameterChange:
                    Debug.Log("Economic parameter change approved");
                    // TODO: Implement parameter changes
                    break;

                case ProposalType.FeatureRequest:
                    Debug.Log("Feature request approved - added to development queue");
                    break;

                case ProposalType.Emergency:
                    Debug.Log("Emergency action approved - executing immediately");
                    break;
            }

            OnProposalExecuted?.Invoke(proposal);
        }

        /// <summary>
        /// Get all active proposals
        /// </summary>
        public List<Proposal> GetActiveProposals()
        {
            return activeProposals.FindAll(p => p.status == ProposalStatus.Active);
        }

        /// <summary>
        /// Calculate voting power based on wallet holdings and reputation
        /// </summary>
        public float CalculateVotingPower(string walletAddress, float tokenBalance)
        {
            // Base voting power from token holdings
            float basePower = tokenBalance;

            // Bonus from reputation (up to 50% increase)
            float reputation = TransactionValidator.Instance.GetWalletReputation(walletAddress);
            float reputationBonus = reputation * 0.5f;

            return basePower * (1f + reputationBonus);
        }
    }

    [System.Serializable]
    public class Proposal
    {
        public int id;
        public string title;
        public string description;
        public ProposalType proposalType;
        public float requestedAmount;
        public float createdTime;
        public float endTime;
        public ProposalStatus status;
        public float votesFor;
        public float votesAgainst;
        public float totalVotes;
        public List<string> voters = new List<string>();
    }

    public enum ProposalType
    {
        TreasuryAllocation,
        ParameterChange,
        FeatureRequest,
        Emergency
    }

    public enum ProposalStatus
    {
        Active,
        Passed,
        Failed,
        Ended,
        Executed
    }
}
