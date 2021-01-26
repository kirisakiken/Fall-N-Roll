using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace BezmicanZehir.Core.Managers
{
    public class LevelManager : MonoBehaviour
    {
        public static int CurrentSceneIndex;

        public UnityEvent onSceneTransition;

        private void Awake()
        {
            CurrentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        }
        
        public static void PauseGame()
        {
            Time.timeScale = 0.0f;
        }

        public static void ResumeGame()
        {
            Time.timeScale = 1.0f;
        }

        public void ResetLevel()
        {
            SceneManager.LoadScene(CurrentSceneIndex);
        }

        public void LoadLevel(int levelIndex)
        {
            //StartCoroutine(LoadAsync(levelIndex));
            Time.timeScale = 1.0f;
            SceneManager.LoadScene(levelIndex);
        }

        private IEnumerator LoadAsync(int levelIndex)
        {
            yield return null;
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
