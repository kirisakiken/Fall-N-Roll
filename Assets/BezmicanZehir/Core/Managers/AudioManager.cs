using UnityEngine;
using UnityEngine.Audio;

namespace BezmicanZehir.Core.Managers
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Mixers")]
        [SerializeField] private AudioMixer effectMixer;
        [SerializeField] private AudioMixer musicMixer;
        
        [Header("Sounds")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip backSound;
        [SerializeField] private AudioSource effectSource;

        /// <summary>
        /// Plays click sound.
        /// </summary>
        public void PlayClickSound()
        {
            effectSource.clip = clickSound;
            effectSource.Play();
        }

        /// <summary>
        /// Plays return click sound.
        /// </summary>
        public void PlayBackSound()
        {
            effectSource.clip = backSound;
            effectSource.Play();
        }

        /// <summary>
        /// Sets music volume with given parameter.
        /// </summary>
        /// <param name="value"> Target value.</param>
        public void SetMusicVolume(float value)
        {
            musicMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20.0f);
        }

        /// <summary>
        /// Sets sound effects volume with given parameter.
        /// </summary>
        /// <param name="value"> Target value.</param>
        public void SetEffectVolume(float value)
        {
            effectMixer.SetFloat("EffectVolume", Mathf.Log10(value) * 20.0f);
        }
    }
}
