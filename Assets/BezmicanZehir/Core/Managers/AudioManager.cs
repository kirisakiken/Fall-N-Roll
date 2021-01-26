using System;
using UnityEngine;

namespace BezmicanZehir.Core.Managers
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip backSound;
        [SerializeField] private AudioSource effectSource;

        public void PlayClickSound()
        {
            effectSource.clip = clickSound;
            effectSource.Play();
        }

        public void PlayBackSound()
        {
            effectSource.clip = backSound;
            effectSource.Play();
        }
    }
}
