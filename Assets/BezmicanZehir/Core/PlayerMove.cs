using System;
using System.Collections;
using BezmicanZehir.Core.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BezmicanZehir.Core
{
    /// <summary>
    /// This class is mainly used to move Player in 3D world space.
    /// </summary>
    public class PlayerMove : MonoBehaviour
    {
        [Header("Input Fields")]
        [SerializeField] private string horizontalInputName;
        [SerializeField] private string verticalInputName;
        
        [Header("Movement Fields")]
        [SerializeField] [Min(0)] private float movementSpeed;
        [SerializeField] [Min(0)] private float rotationSmoothMultiplier;
        [SerializeField] private Transform humanoidTransform;
        private Rigidbody _playerRigidbody;
        private Transform _playerTransform;
        private float _smoothingVelocity;
        private Vector3 _spawnPoint;
        private Quaternion _spawnRotation;
        private bool _canMove;
        private bool _isGrounded;

        public static int Rank;

        [Header("Animator Fields")]
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

        [Header("Other")]
        [SerializeField] private int paintSceneIndex;

        public delegate void ExecutePaintRoutine();
        public ExecutePaintRoutine executePaintRoutine;

        private void Awake()
        {
            _canMove = false;
            _waitForDeath = new WaitForSeconds(deathClip.length);
            _waitForRespawn = new WaitForSeconds(respawnClip.length);
            
            LockCursor();
            
            _playerTransform = GetComponent<Transform>();
            _playerRigidbody = GetComponent<Rigidbody>();
            
            _spawnPoint = _playerTransform.position;
            _spawnRotation = humanoidTransform.rotation;
        }

        private IEnumerator Start()
        {
            yield return _waitForRespawn;
            _canMove = true;
        }

        private void Update()
        {
            if (!GameMaster.RoundIsLive) return;
            
            _isGrounded = GroundControl();
            humanoidAnimator.SetBool(Falling, !_isGrounded);
            
            if (!_canMove || !_isGrounded) return;
            
            MovementInput();
        }
        
        #region Events (Collision/Trigger)

        private void OnCollisionEnter(Collision other)
        {
            if (!_canMove) return;
            
            // Used to execute PlayerDeath event chain
            if (other.collider.CompareTag("Death") || other.collider.CompareTag("Obstacle"))
            {
                DeathEvent();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // If player falls into Water execute : FallRespawnEvent()
            if (other.CompareTag("Water"))
            {
                StartCoroutine(FallRespawnEvent());
            }
            // If player finishes the round execute : FinishEvent()
            else if (other.CompareTag("Finish"))
            {
                FinishEvent();
            }
        }
        
        #endregion

        #region Movement
        
        /// <summary>
        /// Main input function to move player. 
        /// </summary>
        private void MovementInput()
        {
            var horizontalInput = Input.GetAxis(horizontalInputName);
            var verticalInput = Input.GetAxis(verticalInputName);

            var rightMovement = _playerTransform.right * horizontalInput;
            var forwardMovement = _playerTransform.forward * verticalInput;

            var movementVector = Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * movementSpeed;
            movementVector.y = _playerRigidbody.velocity.y;
            
            AnimationController(horizontalInput, verticalInput);

            _playerRigidbody.velocity = movementVector;
            PlayerRotation(horizontalInput, verticalInput);
        }

        /// <summary>
        /// Rotates player's visual body (only mesh) with given parameters.
        /// </summary>
        /// <param name="horizontal"> Right input.</param>
        /// <param name="vertical"> Forward input.</param>
        private void PlayerRotation(float horizontal, float vertical)
        {
            var angle = RotatePlayerToTargetInput(horizontal, vertical);
            var targetAngle = Mathf.SmoothDampAngle(humanoidTransform.eulerAngles.y, angle, ref _smoothingVelocity, rotationSmoothMultiplier);

            humanoidTransform.rotation = Quaternion.Euler(0.0f, targetAngle, 0.0f);
        }

        /// <summary>
        /// This function used to calculate rotation angle on Y axis with given parameters.
        /// </summary>
        /// <param name="horizontal"> Right input.</param>
        /// <param name="vertical"> Forward input.</param>
        /// <returns> Target Y Angle as Degrees.</returns>
        private float RotatePlayerToTargetInput(float horizontal, float vertical)
        {
            return Mathf.Atan2(horizontal, vertical) * Mathf.Rad2Deg;
        }
        
        #endregion

        #region Animation
        
        /// <summary>
        /// This function is used to toggle between IDLE-RUNNING animations with given parameters.
        /// </summary>
        /// <param name="horizontalInput"> Right input.</param>
        /// <param name="verticalInput"> Forward input.</param>
        private void AnimationController(float horizontalInput, float verticalInput)
        {
            if (horizontalInput != 0.0f || verticalInput != 0.0f)
            {
                humanoidAnimator.SetBool(IsRunning, true);
            }
            else
            {
                humanoidAnimator.SetBool(IsRunning, false);
            }
        }
        
        #endregion
        
        #region Events

        /// <summary>
        /// This function is used to execute chain events for PlayerDeath
        /// </summary>
        private void DeathEvent()
        {
            _canMove = false;
            humanoidAnimator.SetTrigger(DeathTrigger);
            StartCoroutine(RespawnEvent());
        }

        /// <summary>
        /// This function used to respawn player after hitting obstacle.
        /// </summary>
        /// <returns> Waits for death and landing animations.</returns>
        private IEnumerator RespawnEvent()
        {
            yield return _waitForDeath; // Wait Until death animation finishes.
            
            _playerTransform.position = _spawnPoint;
            humanoidTransform.rotation = _spawnRotation;
            humanoidAnimator.SetTrigger(RespawnTrigger);
            
            yield return _waitForRespawn; // Wait Until landing animation finishes.
            _canMove = true;
        }

        /// <summary>
        /// This function is used to respawn player after falling into water.
        /// </summary>
        /// <returns> Waits for landing animation.</returns>
        private IEnumerator FallRespawnEvent()
        {
            _canMove = false;
            
            _playerTransform.position = _spawnPoint;
            humanoidTransform.rotation = _spawnRotation;
            humanoidAnimator.SetTrigger(RespawnTrigger);
            
            yield return _waitForRespawn; // Wait Until landing animation finishes.
            _canMove = true;
        }

        /// <summary>
        /// This function executes after player finishes the round.
        /// </summary>
        private void FinishEvent()
        {
            _canMove = false;
            humanoidAnimator.SetTrigger(Finish);
            UnlockCursor();
            
            if (LevelManager.CurrentSceneIndex == paintSceneIndex)
                executePaintRoutine?.Invoke();
        }
        
        #endregion

        #region General

        /// <summary>
        /// This function is used to determine if player is grounded.
        /// </summary>
        /// <returns> isGrounded.</returns>
        private bool GroundControl()
        {
            return Physics.SphereCast(transform.position, 1.0f, Vector3.down, out var theHit);
        }
        
        /// <summary>
        /// This function is used to lock the cursor.
        /// </summary>
        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// This function is used to unlock the cursor.
        /// </summary>
        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
        }
        
        #endregion
    }
}
