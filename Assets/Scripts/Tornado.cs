using UnityEngine;
using System.Collections.Generic;

public class Tornado : MonoBehaviour
{
    public float a = 0.5f; // Aspiration
    public float gamma = 20f; // Puissance de rotation
    public float nu = 0.1f; // Viscosité cinématique
    public List<Rigidbody> elements = new List<Rigidbody>();
    public GameObject tornadoVisual;

    void FixedUpdate()
    {
        foreach (Rigidbody element in elements)
        {
            Vector3 elementPos = element.position;
            Vector3 transformPos = transform.position;
            float z = elementPos.y - transformPos.y;
            elementPos.y = 0;
            transformPos.y = 0;
            float dist = Vector3.Distance(elementPos, transformPos);
            Vector3 Radial = (elementPos - transformPos).normalized;
            Vector3 Tangential = Vector3.Cross(Vector3.up, Radial).normalized;
            
            float Vr = -a * dist;
            float Vtheta = gamma / (2 * Mathf.PI * dist) * (1 - Mathf.Exp(- (a * dist * dist) / (2 * nu)));
            float Vz = 2 * a * z;

            Vector3 velocity = Vr * Radial + Vtheta * Tangential + Vz * Vector3.up;
            element.linearVelocity = velocity;
        }
    }
}
