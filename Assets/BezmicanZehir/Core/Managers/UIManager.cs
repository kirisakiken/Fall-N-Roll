using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BezmicanZehir.Core.Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private int currentLevelIndex;
        
        [Header("UI Groups")]
        [SerializeField] private CanvasGroup pauseMenu;
        [SerializeField] private CanvasGroup settingsMenu;
        [SerializeField] private Animator fadeImageAnimator;
        [SerializeField] private TMP_Text playerRankText;
        [SerializeField] private TMP_Text maxPlayerText;
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
            fadeImageAnimator.gameObject.SetActive(true);
            
            _isPaused = false;
            StartCoroutine(FadeIn());

            maxPlayerText.text = GameMaster.PlayerCount.ToString();
            GameMaster.roundFinish += OpenGameOverWindow;
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
        
        #region Events

        private void OpenGameOverWindow(bool playerHasWon)
        {
            gameOverWindow.SetActive(true);
            
            if (playerHasWon)
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
