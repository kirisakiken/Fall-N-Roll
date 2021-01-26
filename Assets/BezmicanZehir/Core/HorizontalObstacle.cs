using UnityEngine;

namespace BezmicanZehir.Core
{
    public class HorizontalObstacle : MonoBehaviour
    {
        [SerializeField] private Transform childObstacle;
        [SerializeField] [Min(0)] private float horizontalLength;
        [SerializeField] [Min(0)] private float speed;

        [SerializeField] private Vector3 horizontalOffset;
        
        private Vector3 _translationVector;

        private void FixedUpdate()
        {
            childObstacle.localPosition = MovementVector(horizontalOffset);
        }

        private Vector3 MovementVector(Vector3 offsetVector)
        {
            return new Vector3(Mathf.PingPong(Time.time * speed, horizontalLength), 0.0f, 0.0f) + offsetVector;
        }
    }
}
