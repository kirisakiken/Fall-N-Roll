using UnityEngine;

namespace BezmicanZehir.Core
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] [Min(0)] private float angularSpeed;
        [SerializeField] [Range(-1, 1)] private int direction;

        private void FixedUpdate()
        {
            AngularRotation();
        }

        private void AngularRotation()
        {
            transform.Rotate(Vector3.up * (angularSpeed * direction * Time.deltaTime));
        }
    }
}
