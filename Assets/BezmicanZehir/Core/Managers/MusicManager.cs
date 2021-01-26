using System;
using UnityEngine;

namespace BezmicanZehir.Core.Managers
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager manager = null;
        
        private void Awake()
        {
            if (manager is null)
                manager = this;
            else if (manager != this)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
        }
    }
}
