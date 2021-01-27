using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BezmicanZehir.Core
{
    /// <summary>
    /// This class is used to control RotatingPlatform obstacles.
    /// </summary>
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

        /// <summary>
        /// This function is used to AddForce on Player/Agents while they staying in trigger
        /// of 'this' rotating platform instance.
        /// </summary>
        /// <param name="other"> Target collider which is in trigger.</param>
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("AI"))
            {
                var force = Vector3.right * (-direction * forceFieldPower * Time.deltaTime);
                ForceFieldPower(other.GetComponent<Rigidbody>(), force);
            }
        }
        
        /// <summary>
        /// This function is used to rotate 'this' platform relative to Time and other Serialized parameters.
        /// </summary>
        private void AngularRotation()
        {
            rollerTransform.Rotate(Vector3.forward * (angularSpeed * direction * Time.deltaTime), Space.World);
        }

        /// <summary>
        /// This function is used to AddForce on any Player/ML-Agent with given parameters.
        /// </summary>
        /// <param name="rb"> Target rigidbody (Player/Agents).</param>
        /// <param name="forceVector"> Force amount and direction.</param>
        private void ForceFieldPower(Rigidbody rb, Vector3 forceVector)
        {
            rb.AddForce(forceVector, ForceMode.Acceleration);
        }
    }
}
