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
            
            fadeImageAnimator.gameObject.SetActive(true);
            
            _isPaused = false;
            StartCoroutine(FadeIn());
            
            /*if (currentLevelIndex == 1)
            {
                Painter.endSinglePlayerLevel += OpenGameOverWindow;
                return;
            }
            GameMaster.roundFinish += OpenGameOverWindow;*/
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

        private void PauseGame()
        {
            _isPaused = true;
            OpenPauseMenu();
            UnlockCursor();
            LevelManager.PauseGame();
        }

        public void ResumeGame()
        {
            _isPaused = false;
            ClosePauseMenu();
            LockCursor();
            LevelManager.ResumeGame();
        }
        
        public void OpenPauseMenu()
        {
            pauseMenu.gameObject.SetActive(true);
            pauseMenu.interactable = true;
        }

        public void ClosePauseMenu()
        {
            pauseMenu.gameObject.SetActive(false);
            pauseMenu.interactable = false;
        }
        
        #endregion
        
        #region Settings

        public void OpenSettingsMenu()
        {
            settingsMenu.gameObject.SetActive(true);
            settingsMenu.interactable = true;
        }

        public void CloseSettingsMenu()
        {
            settingsMenu.gameObject.SetActive(false);
            settingsMenu.interactable = false;
        }

        public void SaveSettings()
        {
            
        }
        
        #endregion
        
        #region Audio

        public void SetEffectVolume()
        {
            _audioManager.SetEffectVolume(effectSlider.value);
        }

        public void SetMusicVolume()
        {
            _audioManager.SetMusicVolume(musicSlider.value);
        }
        
        #endregion
        
        #region Events

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
        
        private void SetPlayerRankOnUI()
        {
            _playerRank = PlayerMove.Rank;
            playerRankText.text = _playerRank.ToString();
        }
        
        #endregion
        
        #region General

        public void OpenWindow(CanvasGroup targetGroup)
        {
            targetGroup.gameObject.SetActive(true);
            targetGroup.interactable = true;
        }

        public void CloseWindow(CanvasGroup targetGroup)
        {
            targetGroup.gameObject.SetActive(false);
            targetGroup.interactable = false;
        }

        public void ReturnToMainMenu()
        {
            StartCoroutine(LoadMainScene());
        }

        private IEnumerator LoadMainScene()
        {
            fadeImageAnimator.gameObject.SetActive(true);
            fadeImageAnimator.SetBool(Out, true);
            yield return new WaitForSeconds(1.0f);
            levelManager.LoadLevel(0);
        }

        public void LoadTargetScene(int sceneIndex)
        {
            StartCoroutine(FadeOut(sceneIndex));
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        #region Fade

        private IEnumerator FadeIn()
        {
            fadeImageAnimator.SetBool(Out, false);
            yield return new WaitForSeconds(1.0f);
            fadeImageAnimator.gameObject.SetActive(false);
        }
        
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
