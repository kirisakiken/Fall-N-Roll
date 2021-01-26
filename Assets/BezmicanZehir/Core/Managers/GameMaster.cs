using System.Collections;
using System.Linq;
using UnityEngine;

namespace BezmicanZehir.Core.Managers
{
    public class GameMaster : MonoBehaviour
    {
        [Header("Application Settings")]
        [SerializeField] private int frameLimit;

        [SerializeField] private Transform endGoal;
        private PlayerMove _player;
        private AIController[] _agents;

        private float[] _distances;
        public static int PlayerCount;

        public static bool RoundIsLive;
        private bool _playerWon;

        public delegate void RoundFinish(bool playerHasWon);
        public static RoundFinish roundFinish;

        private void Awake()
        {
            _playerWon = false;
            RoundIsLive = true;
            
            LockFrameLimit();
            FindPlayerAndAgents();

            PlayerCount = _agents.Length + 1;
        }

        private void Update()
        {
            if (!RoundIsLive) return;

            PlayerMove.Rank = GetPlayerRank();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!RoundIsLive) return;
            
            if (other.CompareTag("Player"))
            {
                RoundIsLive = false;
                _playerWon = true;
                roundFinish?.Invoke(_playerWon);
            }
            else if (other.CompareTag("AI"))
            {
                RoundIsLive = false;
                _playerWon = false;
                roundFinish?.Invoke(_playerWon);
            }
        }
        private void FindPlayerAndAgents()
        {
            _player = FindObjectOfType<PlayerMove>();
            _agents = FindObjectsOfType<AIController>();
        }

        private int GetPlayerRank()
        {
            var playerDist = Vector3.Distance(_player.transform.position, endGoal.position);
            var agentDistances = new float[_agents.Length + 1];

            for (var i = 0; i < _agents.Length; i++)
            {
                agentDistances[i] = Vector3.Distance(_agents[i].transform.position, endGoal.position);
            }

            agentDistances[agentDistances.Length - 1] = playerDist;

            var counter = 1;
            var orderedDistances = agentDistances.OrderBy(t => t);
            foreach (var dist in orderedDistances)
            {
                if (dist.Equals(playerDist)) return counter;
                counter++;
            }
            return counter;
        }
        
        private void LockFrameLimit()
        {
            Application.targetFrameRate = frameLimit;
        }
    }
}
