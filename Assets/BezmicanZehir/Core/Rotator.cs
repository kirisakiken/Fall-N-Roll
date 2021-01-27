using UnityEngine;

namespace BezmicanZehir.Core
{
    /// <summary>
    /// This class is used to rotate the Rotator obstacle.
    /// </summary>
    public class Rotator : MonoBehaviour
    {
        [SerializeField] [Min(0)] private float angularSpeed;
        [SerializeField] [Range(-1, 1)] private int direction;

        private void FixedUpdate()
        {
            AngularRotation();
        }

        /// <summary>
        /// Rotates 'this' rotator relative to Time and given Serialized parameters.
        /// </summary>
        private void AngularRotation()
        {
            transform.Rotate(Vector3.up * (angularSpeed * direction * Time.deltaTime));
        }
    }
}
