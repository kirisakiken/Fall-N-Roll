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

        public void SetMusicVolume(float value)
        {
            musicMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20.0f);
        }

        public void SetEffectVolume(float value)
        {
            effectMixer.SetFloat("EffectVolume", Mathf.Log10(value) * 20.0f);
        }
    }
}
