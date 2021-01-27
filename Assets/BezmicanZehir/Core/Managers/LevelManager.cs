using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace BezmicanZehir.Core.Managers
{
    public class LevelManager : MonoBehaviour
    {
        public static int CurrentSceneIndex; // Active scene index
        public UnityEvent onSceneTransition;

        private void Awake()
        {
            CurrentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        }
        
        /// <summary>
        /// Pauses game.
        /// </summary>
        public static void PauseGame()
        {
            Time.timeScale = 0.0f;
        }

        /// <summary>
        /// Resumes game.
        /// </summary>
        public static void ResumeGame()
        {
            Time.timeScale = 1.0f;
        }

        /// <summary>
        /// Resets current scene.
        /// </summary>
        public void ResetLevel()
        {
            SceneManager.LoadScene(CurrentSceneIndex);
        }

        /// <summary>
        /// Loads scene with given parameter.
        /// </summary>
        /// <param name="levelIndex"> Target scene.</param>
        public void LoadLevel(int levelIndex)
        {
            //StartCoroutine(LoadAsync(levelIndex));
            Time.timeScale = 1.0f;
            SceneManager.LoadScene(levelIndex);
        }

        /// <summary>
        /// Exits game.
        /// </summary>
        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
