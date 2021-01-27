using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace BezmicanZehir.Core.Managers
{
    public static class SaveManager
    {
        /// <summary>
        /// Saves music and effect sound levels with given parameters.
        /// </summary>
        /// <param name="effectsValue"> Effect sound value.</param>
        /// <param name="musicValue"> Music sound value.</param>
        public static void SaveAudioPreferences(float effectsValue, float musicValue)
        {
            var binFormatter = new BinaryFormatter();
            var path = Application.persistentDataPath + "/audio.kken";
            
            var fileStream = new FileStream(path, FileMode.Create);

            var data = effectsValue + "," + musicValue;
            
            binFormatter.Serialize(fileStream, data);
            
            Debug.Log($"Saved Data : {data}");
            
            fileStream.Close();
        }

        /// <summary>
        /// Loads audio preferences which is saved.
        /// </summary>
        /// <returns> Effects sound value and Music sound value as float array.</returns>
        public static float[] LoadAudioPreferences()
        {
            var path = Application.persistentDataPath + "/audio.kken";

            if (File.Exists(path))
            {
                var binFormatter = new BinaryFormatter();
                var fileStream = new FileStream(path, FileMode.Open);

                var data = binFormatter.Deserialize(fileStream) as string;
                fileStream.Close();

                var returnArr = new float[2];
                var returnValuesAsString = data.Split(',');
                for (var i = 0; i < returnValuesAsString.Length; i++)
                {
                    returnArr[i] = float.Parse(returnValuesAsString[i]);
                }

                return returnArr;
            }

            return null;
        }
    }
}
