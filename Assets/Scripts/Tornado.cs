using System;
using UnityEngine;
using System.Collections.Generic;

public class Tornado : MonoBehaviour
{
    public ParticleSystem tornadoParticles;
    public List<Rigidbody> elements = new List<Rigidbody>();
    public float a = 0.5f; // Aspiration
    public float gamma = 20f; // Puissance de rotation
    public float nu = 0.1f; // Viscosité cinématique
    
    private ParticleSystem.Particle[] particles;

    private void Awake()
    {
        particles = new ParticleSystem.Particle[tornadoParticles.main.maxParticles];
    }

    void FixedUpdate()
    {
        foreach (Rigidbody element in elements)
        {
            element.linearVelocity = ApplyTornado(element.position);
        }
    }

    private void LateUpdate()
    {
        int numParticlesAlive = tornadoParticles.GetParticles(particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            particles[i].velocity = ApplyTornado(particles[i].position);
        }
        tornadoParticles.SetParticles(particles, numParticlesAlive);
    }

    private Vector3 ApplyTornado(Vector3 elementPos)
    {
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

        return Vr * Radial + Vtheta * Tangential + Vz * Vector3.up;
    }
}
