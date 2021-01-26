using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

namespace BezmicanZehir.Core
{
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
                //Debug.Log($"Rewarded : {aiController.name} by {rewardAmount}!");
            }
        }

        private void ExecuteReward(AIController agentController, float amount)
        {
            _rewardAbles[agentController] = false;
            SetNextPointForTarget(agentController);
            CustomReward(agentController, amount);
        }

        private void ResetReward(AIController agentController)
        {
            _rewardAbles[agentController] = true;
        }

        private void SetNextPointForTarget(AIController aiController)
        {
            aiController.NextRewardPointPosition = transform.position;
        }
        
        private void CustomReward(AIController aiController, float amount)
        {
            aiController.AddReward(amount);
        }
    }
}
