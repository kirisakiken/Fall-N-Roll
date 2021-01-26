using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BezmicanZehir.Core
{
    public class RotatingPlatform : MonoBehaviour
    {
        [SerializeField] [Min(0)] private float angularSpeed;
        [SerializeField] [Range(-1, 1)] private int direction;
        [SerializeField] [Min(0)] private float forceFieldPower;
        [SerializeField] private Transform rollerTransform;

        private void FixedUpdate()
        {
            AngularRotation();
        }

        private void AngularRotation()
        {
            rollerTransform.Rotate(Vector3.forward * (angularSpeed * direction * Time.deltaTime), Space.World);
        }

        private void ForceFieldPower(Rigidbody rb, Vector3 forceVector)
        {
            rb.AddForce(forceVector, ForceMode.Acceleration);
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("AI"))
            {
                var force = Vector3.right * (-direction * forceFieldPower * Time.deltaTime);
                ForceFieldPower(other.GetComponent<Rigidbody>(), force);
            }
        }
    }
}
