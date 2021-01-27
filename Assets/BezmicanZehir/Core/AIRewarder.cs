using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

namespace BezmicanZehir.Core
{
    /// <summary>
    /// This class is used to reward ML-agents while training ML-Agent brains.
    /// For every instance of this class. Agent can get rewarded with given amount.
    /// </summary>
    public class AIRewarder : MonoBehaviour
    {
        [SerializeField] private float rewardAmount;
        
        private AIController[] _agents;
        private bool[] _rewardAbleAgents;

        private Dictionary<AIController, bool> _rewardAbles;
        
        private void Start()
        {
            _agents = FindObjectsOfType<AIController>();
            foreach (var agent in _agents)
            {
                agent.resetReward += ResetReward;
            }

            _rewardAbles = new Dictionary<AIController, bool>(_agents.Length);
            foreach (var t in _agents)
            {
                _rewardAbles.Add(t, true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("AI"))
            {
                var aiController = other.GetComponent<AIController>();
                if (!_rewardAbles[aiController]) return;
                
                ExecuteReward(aiController, rewardAmount);
            }
        }

        /// <summary>
        /// This function used to execute rewarding routine for given ML-agent.
        /// </summary>
        /// <param name="agentController"> Target ML-agent.</param>
        /// <param name="amount"> Reward amount.</param>
        private void ExecuteReward(AIController agentController, float amount)
        {
            _rewardAbles[agentController] = false;
            SetNextPointForTarget(agentController);
            CustomReward(agentController, amount);
        }

        /// <summary>
        /// Resets reward of target agent after its death event so agent may get rewarded after
        /// respawn.
        /// </summary>
        /// <param name="agentController"> Target ML-agent.</param>
        private void ResetReward(AIController agentController)
        {
            _rewardAbles[agentController] = true;
        }

        /// <summary>
        /// Sets next reward point position for given ML-agent.
        /// </summary>
        /// <param name="aiController"> Target ML-agent.</param>
        private void SetNextPointForTarget(AIController aiController)
        {
            aiController.NextRewardPointPosition = transform.position;
        }
        
        /// <summary>
        /// This function used to reward given ML-agent with custom amount.
        /// </summary>
        /// <param name="aiController"> Target ML-agent.</param>
        /// <param name="amount"> Reward amount.</param>
        private void CustomReward(AIController aiController, float amount)
        {
            aiController.AddReward(amount);
        }
    }
}
