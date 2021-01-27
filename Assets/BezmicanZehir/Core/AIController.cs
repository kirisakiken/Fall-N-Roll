using System;
using System.Collections;
using BezmicanZehir.Core.Managers;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

namespace BezmicanZehir.Core
{
    /// <summary>
    /// This class is used to control ML-agents.
    /// </summary>
    public class AIController : Agent
    {
        private AIRewarder[] _rewarders;
        public Vector3 NextRewardPointPosition { get; set; }

        [Header("Movement Fields")]
        [SerializeField] private Transform agentBodyTransform;
        [SerializeField] private Transform humanoidTransform;
        [SerializeField] private Rigidbody agentRigidbody;
        [SerializeField] private float movementSpeed;
        [SerializeField] private float rotationSensitivity;
        private CapsuleCollider _agentCollider;
        private float _smoothingVelocity;
        private Vector3 _spawnPoint;
        private Quaternion _spawnRotation;
        private bool _canMove;
        
        [Header("Animation Fields")]
        [SerializeField] private Animator humanoidAnimator;
        [SerializeField] private AnimationClip deathClip;
        [SerializeField] private AnimationClip respawnClip;
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int DeathTrigger = Animator.StringToHash("DeathTrigger");
        private static readonly int RespawnTrigger = Animator.StringToHash("RespawnTrigger");
        private static readonly int Finish = Animator.StringToHash("Finish");
        private WaitForSeconds _waitForDeath;
        private WaitForSeconds _waitForRespawn;

        public delegate void ResetReward(AIController agentController);
        public ResetReward resetReward;

        private void Awake()
        {
            _agentCollider = GetComponent<CapsuleCollider>();
            
            _canMove = false;
            _waitForDeath = new WaitForSeconds(deathClip.length);
            _waitForRespawn = new WaitForSeconds(respawnClip.length);
            
            _spawnPoint = agentBodyTransform.position;
            _spawnRotation = humanoidTransform.rotation;
        }

        private IEnumerator Start()
        {
            yield return _waitForRespawn;
            _canMove = true;
            GameMaster.roundFinish += EndLevel;
        }

        /*
         * Following region uses Unity's ML-Agents methods. For more information,
         * please check out : https://github.com/Unity-Technologies/ml-agents
         */
        #region Machine Learning

        /// <summary>
        /// This method executes when episode of 'this' agent begins.
        /// </summary>
        public override void OnEpisodeBegin()
        {
            agentBodyTransform.position = _spawnPoint;
            agentBodyTransform.rotation = _spawnRotation;
        }
        
        /// <summary>
        /// Every time before making decision, 'this' agent collects observations,
        /// this method executes before every decision.
        /// </summary>
        /// <param name="sensor"></param>
        public override void CollectObservations(VectorSensor sensor)
        {
            var target = NextRewardPointPosition - transform.position;
            sensor.AddObservation(target);
            
            AddReward(-0.001f);
        }

        /// <summary>
        /// This function used to get input from neural networks.
        /// </summary>
        /// <param name="vectorAction"> ActionInput(s) as vector coming from neural networks.</param>
        public override void OnActionReceived(float[] vectorAction)
        {
            if (!GameMaster.RoundIsLive) return;
            if (!_canMove) return;
            
            var x = vectorAction[0];
            var y = vectorAction[1];
            
            var yProcessed = y == 1.0f ? 1.0f : -1.0f;
            
            AIRigidbodyMovement(x, yProcessed);
        }

        /// <summary>
        /// This function is used to control agent with player input during run-time.
        /// Since input parameter conversion didn't implemented. You'll need to convert actionsOut
        /// before using Heuristic learning.
        /// </summary>
        /// <param name="actionsOut"> Neural network inputs given by player input.</param>
        public override void Heuristic(float[] actionsOut)
        {
            // Warning ! Conversion needed for Mouse X Input !
            actionsOut[0] = Input.GetAxisRaw("Vertical");
            actionsOut[1] = Input.GetAxisRaw("Mouse X");
            
            AIRigidbodyMovement(actionsOut[0], actionsOut[1]);
        }

        #endregion
        
        #region Events (Collision-Trigger)

        private void OnCollisionEnter(Collision other)
        {
            if (!_canMove) return;
            
            // If agent hits any obstacle : Execute DeathEvent()
            if (other.collider.CompareTag("Obstacle"))
            {
                DeathEvent();
            }
            
            if (other.collider.CompareTag("Rotator"))
            {
                AddReward(-1.0f);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // If agent falls into water : Execute FallEvent()
            if (other.CompareTag("Water"))
            {
                StartCoroutine(FallEvent());
            }
        }
        
        #endregion
        
        #region Events

        /// <summary>
        /// Executes AI Agent's death event.
        /// </summary>
        private void DeathEvent()
        {
            _canMove = false; // Preventing agent movement during death animation.
            humanoidAnimator.SetTrigger(DeathTrigger);
            StartCoroutine(HitEvent());
        }

        /// <summary>
        /// HitEvent coroutine executes all necessary events for respawn.
        /// </summary>
        /// <returns> Returns delays for every animation during process.</returns>
        private IEnumerator HitEvent()
        {
            _agentCollider.enabled = false;
            agentRigidbody.isKinematic = true;

            yield return _waitForDeath; // Wait Until death animation finishes.
            
            // Penalty & Restart
            AddReward(-1.2f);
            EndEpisode();
            
            agentBodyTransform.position = _spawnPoint;
            humanoidTransform.rotation = _spawnRotation;
            humanoidAnimator.SetTrigger(RespawnTrigger);
            
            agentRigidbody.isKinematic = false;
            _agentCollider.enabled = true;
            
            yield return _waitForRespawn; // Wait Until landing animation finishes.
            
            //Reset self rewards
            resetReward?.Invoke(this);
            
            _canMove = true;
        }

        /// <summary>
        /// FallEvent coroutine executes all necessary events for respawn. Similar to HitEvent,
        /// but this one doesn't return death animation delay.
        /// </summary>
        /// <returns> Returns delays for every animation during process.</returns>
        private IEnumerator FallEvent()
        {
            _canMove = false;

            // Penalty & Restart
            AddReward(-3.0f);
            EndEpisode();

            agentBodyTransform.position = _spawnPoint;
            humanoidTransform.rotation = _spawnRotation;
            humanoidAnimator.SetTrigger(RespawnTrigger);
            
            yield return _waitForRespawn; // Wait Until landing animation finishes.
            
            //Reset self rewards
            resetReward?.Invoke(this);
            
            _canMove = true;
        }

        /// <summary>
        /// This function used to train ML-agent brains and it executes when agent crosses finish line.
        /// After that, agent respawns and trains from start to finish again. Do not use this for Live version.
        /// </summary>
        private void FinishEvent() 
        {
            //_canMove = false;
            //humanoidAnimator.SetTrigger(Finish);
            
            // Reward & Restart
            //Debug.Log($"Rewarded : {name} by 500!");
            AddReward(15.0f);
            EndEpisode();
        }

        /// <summary>
        /// This function executes when agent crosses finish line before player on Live version.
        /// Do not use this function for training ML-agent brains. Instead, use FinishEvent().
        /// </summary>
        /// <param name="agentHasWon"> The boolean which represent if 'this' agent won current round.</param>
        private void EndLevel(bool agentHasWon)
        {
            _canMove = false;
            agentRigidbody.velocity = Vector3.zero;
            if (agentHasWon)
                humanoidAnimator.SetTrigger(Finish);
        }
        
        #endregion
        
        #region Movement
        
        /// <summary>
        /// This function controls and moves agent in 3D world space with Rigidbody physics.
        /// </summary>
        /// <param name="zInput"> Forward (Top-down vertical) input which comes from neural networks.</param>
        /// <param name="yInput"> Up (Angular) input which comes from neural networks.</param>
        private void AIRigidbodyMovement(float zInput, float yInput)
        {
            var forwardMovement = agentBodyTransform.forward * zInput;

            var movementVector = forwardMovement * movementSpeed;
            movementVector.y = agentRigidbody.velocity.y;
            
            AnimationController(zInput);
            AgentRotation(yInput);
            agentRigidbody.velocity = movementVector;
        }

        /// <summary>
        /// This functions rotates agent on Y axis with given input.
        /// </summary>
        /// <param name="rotationInput"> Up (Angular) input which comes from neural networks.</param>
        private void AgentRotation(float rotationInput)
        {
            var rotY = rotationInput * rotationSensitivity * Time.deltaTime;
            transform.eulerAngles += new Vector3(0.0f, rotY, 0.0f);
        }

        #endregion
        
        #region Animation

        /// <summary>
        /// This function used to control animation states of this agent with given input.
        /// </summary>
        /// <param name="verticalInput"> Forward (Top-down vertical) input which comes from neural networks.</param>
        private void AnimationController(float verticalInput)
        {
            humanoidAnimator.SetBool(IsRunning, verticalInput != 0.0f);
        }
        
        #endregion
    }
}
