using System;
using System.Collections;
using BezmicanZehir.Core.Managers;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

namespace BezmicanZehir.Core
{
    public class AIController : Agent
    {
        [Header("AI Fields")]
        [SerializeField] private Transform finishGoal;
        [SerializeField] private Transform firstCheckPoint;
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
        private static readonly int Falling = Animator.StringToHash("Falling");
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

        #region Machine Learning

        public override void OnEpisodeBegin()
        {
            agentBodyTransform.position = _spawnPoint;
            agentBodyTransform.rotation = _spawnRotation;
        }
        
        public override void CollectObservations(VectorSensor sensor)
        {
            var target = NextRewardPointPosition - transform.position;
            sensor.AddObservation(target);
            
            AddReward(-0.001f);
        }

        public override void OnActionReceived(float[] vectorAction)
        {
            if (!GameMaster.RoundIsLive) return;
            if (!_canMove) return;
            
            var x = vectorAction[0];
            //var z = vectorAction[1];
            var y = vectorAction[1];
            
            var yProcessed = y == 1.0f ? 1.0f : -1.0f;
            
            AIRigidbodyMovement(x, yProcessed, 0);
        }

        public override void Heuristic(float[] actionsOut)
        {
            actionsOut[0] = Input.GetAxisRaw("Vertical");
            actionsOut[1] = Input.GetAxisRaw("Mouse X");
            
            AIRigidbodyMovement(actionsOut[0], actionsOut[1], 0);
        }

        #endregion
        
        #region Events (Collision-Trigger)

        private void OnCollisionEnter(Collision other)
        {
            if (!_canMove) return;
            
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
            /*if (other.CompareTag("Wall"))
            {
                AddReward(-3.0f);
                //Debug.Log($"Punished : {name} by -15");
                EndEpisode();
            }*/
            if (other.CompareTag("Water"))
            {
                StartCoroutine(FallEvent());
            }
            /*if (other.CompareTag("Finish"))
            {
                FinishEvent();
            }*/
        }
        
        #endregion
        
        #region Events

        private void DeathEvent()
        {
            _canMove = false;
            humanoidAnimator.SetTrigger(DeathTrigger);
            StartCoroutine(HitEvent());
        }

        private IEnumerator HitEvent()
        {
            _agentCollider.enabled = false;
            agentRigidbody.isKinematic = true;

            yield return _waitForDeath; // Wait Until death animation finishes.
            
            // Penalty & Restart
            //Debug.Log($"Punished : {name} by -15");
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

        private IEnumerator FallEvent()
        {
            _canMove = false;

            // Penalty & Restart
            //Debug.Log($"Punished : {name} by -15");
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

        private void FinishEvent() // Used mainly for ML-Training
        {
            //_canMove = false;
            //humanoidAnimator.SetTrigger(Finish);
            
            // Reward & Restart
            //Debug.Log($"Rewarded : {name} by 500!");
            AddReward(15.0f);
            EndEpisode();
        }

        private void EndLevel(bool agentHasWon)
        {
            _canMove = false;
            agentRigidbody.velocity = Vector3.zero;
            if (agentHasWon)
                humanoidAnimator.SetTrigger(Finish);
        }
        
        #endregion
        
        #region Movement
        
        private void AIRigidbodyMovement(float zInput, float yInput, float xInput)
        {
            var forwardMovement = agentBodyTransform.forward * zInput;

            var movementVector = forwardMovement * movementSpeed;
            movementVector.y = agentRigidbody.velocity.y;
            
            AnimationController(zInput);
            AgentRotation(yInput);
            agentRigidbody.velocity = movementVector;
        }

        private void AgentRotation(float rotationInput)
        {
            var rotY = rotationInput * rotationSensitivity * Time.deltaTime;
            transform.eulerAngles += new Vector3(0.0f, rotY, 0.0f);
        }

        #endregion
        
        #region Animation

        private void AnimationController(float verticalInput)
        {
            humanoidAnimator.SetBool(IsRunning, verticalInput != 0.0f);
        }
        
        #endregion
    }
}
