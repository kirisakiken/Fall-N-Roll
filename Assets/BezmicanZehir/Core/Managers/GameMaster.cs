using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace BezmicanZehir.Core.Managers
{
    public class GameMaster : MonoBehaviour
    {
        [SerializeField] private int frameLimit;
        [SerializeField] private Transform endGoal;
        
        private PlayerMove _player;
        private AIController[] _agents;
        private float[] _distances;
        public static int PlayerCount;

        public static bool RoundIsLive;
        public static bool PlayerWon;

        public delegate void RoundFinish(bool playerHasWon);
        public static RoundFinish roundFinish;

        public UnityEvent onFinishSinglePlayer;
        public UnityEvent onFinishMultiPlayer;

        private void Awake()
        {
            PlayerWon = false;
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
                PlayerWon = true;
                //roundFinish?.Invoke(_playerWon);
                if (LevelManager.CurrentSceneIndex == 2)
                    onFinishMultiPlayer?.Invoke();
                else
                    onFinishSinglePlayer?.Invoke();
            }
            else if (other.CompareTag("AI"))
            {
                RoundIsLive = false;
                PlayerWon = false;
                //roundFinish?.Invoke(PlayerWon);
                onFinishMultiPlayer?.Invoke();
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
