using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BezmicanZehir.Core.Managers
{
    public class UIManager : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private int currentLevelIndex;
        private AudioManager _audioManager;
        
        [Header("UI Groups")]
        [SerializeField] private CanvasGroup pauseMenu;
        [SerializeField] private CanvasGroup settingsMenu;
        [SerializeField] private Animator fadeImageAnimator;
        [SerializeField] private TMP_Text playerRankText;
        [SerializeField] private TMP_Text maxPlayerText;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider effectSlider;
        private int _playerRank;
        private static readonly int Out = Animator.StringToHash("Out");

        [Header("Game Over Event")] 
        [SerializeField] private GameObject gameOverWindow;
        [SerializeField] private GameObject playerWon;
        [SerializeField] private GameObject agentWon;

        [Header("Input Fields")]
        [SerializeField] private KeyCode pauseKey;
        private bool _isPaused;
        


        private void Awake()
        {
            _audioManager = FindObjectOfType<AudioManager>();

            var data = SaveManager.LoadAudioPreferences();
            if (data is null)
            {
                SaveSettings();
            }
            else
            {
                LoadSettings();
            }
            
            fadeImageAnimator.gameObject.SetActive(true);
            
            _isPaused = false;
            StartCoroutine(FadeIn());
            
            if (LevelManager.CurrentSceneIndex == 2)
                maxPlayerText.text = GameMaster.PlayerCount.ToString();
        }

        private void Start()
        {
            SetEffectVolume();
            SetMusicVolume();
        }

        private void Update()
        {
            if (currentLevelIndex == 0) return;
            
            if (Input.GetKeyDown(pauseKey))
            {
                if (!_isPaused)
                {
                    PauseGame();
                }
            }

            if (currentLevelIndex == 1) return;
            
            SetPlayerRankOnUI();
        }

        #region Pause Menu

        /// <summary>
        /// Pauses game.
        /// </summary>
        private void PauseGame()
        {
            _isPaused = true;
            OpenPauseMenu();
            UnlockCursor();
            LevelManager.PauseGame();
        }

        /// <summary>
        /// Resumes game.
        /// </summary>
        public void ResumeGame()
        {
            _isPaused = false;
            ClosePauseMenu();
            LockCursor();
            LevelManager.ResumeGame();
        }
        
        /// <summary>
        /// This function is used to open pause menu.
        /// </summary>
        public void OpenPauseMenu()
        {
            pauseMenu.gameObject.SetActive(true);
            pauseMenu.interactable = true;
        }

        /// <summary>
        /// This function is used to close pause menu.
        /// </summary>
        public void ClosePauseMenu()
        {
            pauseMenu.gameObject.SetActive(false);
            pauseMenu.interactable = false;
        }
        
        #endregion
        
        #region Settings

        /// <summary>
        /// This function is used to open settings menu.
        /// </summary>
        public void OpenSettingsMenu()
        {
            settingsMenu.gameObject.SetActive(true);
            settingsMenu.interactable = true;
        }

        /// <summary>
        /// This function is used to close settings menu.
        /// </summary>
        public void CloseSettingsMenu()
        {
            settingsMenu.gameObject.SetActive(false);
            settingsMenu.interactable = false;
        }

        /// <summary>
        /// Saves Audio Settings.
        /// </summary>
        public void SaveSettings()
        {
            SaveManager.SaveAudioPreferences(effectSlider.value, musicSlider.value);
        }

        /// <summary>
        /// Loads Audio Settings.
        /// </summary>
        public void LoadSettings()
        {
            var data = SaveManager.LoadAudioPreferences();
            if (!(data is null))
            {
                effectSlider.value = data[0];
                musicSlider.value = data[1];
            }
        }
        
        #endregion
        
        #region Audio

        /// <summary>
        /// This function is used to set Effects volume.
        /// </summary>
        public void SetEffectVolume()
        {
            _audioManager.SetEffectVolume(effectSlider.value);
        }

        /// <summary>
        /// This function is used to set Music volume.
        /// </summary>
        public void SetMusicVolume()
        {
            _audioManager.SetMusicVolume(musicSlider.value);
        }
        
        #endregion
        
        #region Events

        /// <summary>
        /// This function is used to open Game Over window after player/agent finishes the round.
        /// </summary>
        public void OpenGameOverWindow()
        {
            UnlockCursor();
            gameOverWindow.SetActive(true);
            
            if (GameMaster.PlayerWon)
            {
                playerWon.SetActive(true);
                agentWon.SetActive(false);
            }
            else
            {
                playerWon.SetActive(false);
                agentWon.SetActive(true);
            }
        }
        
        /// <summary>
        /// This function is used to open Game Over window after player/agent finishes the round.
        /// Only used for SinglePlayer scene.
        /// </summary>
        public void OpenGameOverWindowForPaintScene()
        {
            UnlockCursor();
            gameOverWindow.SetActive(true);
            playerWon.SetActive(true);
            agentWon.SetActive(false);
        }
        
        /// <summary>
        /// This function is used to update Player's rank on UI.
        /// </summary>
        private void SetPlayerRankOnUI()
        {
            _playerRank = PlayerMove.Rank;
            playerRankText.text = _playerRank.ToString();
        }
        
        #endregion
        
        #region General

        /// <summary>
        /// This function enables canvas group with given parameter.
        /// </summary>
        /// <param name="targetGroup"> Target CanvasGroup.</param>
        public void OpenWindow(CanvasGroup targetGroup)
        {
            targetGroup.gameObject.SetActive(true);
            targetGroup.interactable = true;
        }

        /// <summary>
        /// This function disables canvas group with given parameter.
        /// </summary>
        /// <param name="targetGroup"> Target CanvasGroup.</param>
        public void CloseWindow(CanvasGroup targetGroup)
        {
            targetGroup.gameObject.SetActive(false);
            targetGroup.interactable = false;
        }

        /// <summary>
        /// This function is used to Load Main Menu with UI buttons.
        /// </summary>
        public void ReturnToMainMenu()
        {
            StartCoroutine(LoadMainScene());
        }

        /// <summary>
        /// This function is used to Load Main Menu with animation.
        /// </summary>
        /// <returns> Waits for fade out animation.</returns>
        private IEnumerator LoadMainScene()
        {
            fadeImageAnimator.gameObject.SetActive(true);
            fadeImageAnimator.SetBool(Out, true);
            yield return new WaitForSeconds(1.0f);
            levelManager.LoadLevel(0);
        }

        /// <summary>
        /// Loads scene with given parameter.
        /// </summary>
        /// <param name="sceneIndex"> Target scene index.</param>
        public void LoadTargetScene(int sceneIndex)
        {
            StartCoroutine(FadeOut(sceneIndex));
        }

        /// <summary>
        /// Locks cursor.
        /// </summary>
        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// Unlocks cursor.
        /// </summary>
        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        #region Fade

        /// <summary>
        /// Executes fade in animation.
        /// </summary>
        /// <returns> Waits for animation to finish.</returns>
        private IEnumerator FadeIn()
        {
            fadeImageAnimator.SetBool(Out, false);
            yield return new WaitForSeconds(1.0f);
            fadeImageAnimator.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Executes fade out animation and loads scene with given parameter.
        /// </summary>
        /// <param name="targetSceneIndex"> Target scene to load.</param>
        /// <returns> Waits for animation to finish.</returns>
        private IEnumerator FadeOut(int targetSceneIndex)
        {
            fadeImageAnimator.gameObject.SetActive(true);
            fadeImageAnimator.SetBool(Out, true);
            yield return new WaitForSeconds(1.0f);
            levelManager.LoadLevel(targetSceneIndex);
        }
        
        #endregion
        
        #endregion
    }
}
