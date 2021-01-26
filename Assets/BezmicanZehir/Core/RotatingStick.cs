using UnityEngine;

namespace BezmicanZehir.Core
{
    public class RotatingStick : MonoBehaviour
    {
        [SerializeField] private float explosionForce;
        [SerializeField] private float explosionRadius;
        [SerializeField] private float explosionVerticalOffset;

        private void OnCollisionEnter(Collision other)
        {
            if (other.transform.CompareTag("Player") || other.transform.CompareTag("AI"))
            {
                other.transform.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, other.GetContact(0).point + 
                                                                                                            new Vector3(0.0f, explosionVerticalOffset, 0.0f), explosionRadius);
            }
        }
    }
}
