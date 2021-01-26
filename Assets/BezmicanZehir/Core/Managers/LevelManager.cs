using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BezmicanZehir.Core.Managers
{
    public class LevelManager : MonoBehaviour
    {
        public static int CurrentSceneIndex;

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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadLevel(int levelIndex)
        {
            //StartCoroutine(LoadAsync(levelIndex));
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
