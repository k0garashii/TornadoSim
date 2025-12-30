using System;
using UnityEngine;
using System.Collections.Generic;

public enum GridType
{
    Square,
    Cube
}

public class Tornado : MonoBehaviour
{
    public ParticleSystem tornadoParticles;
    private ParticleSystem.Particle[] particles;
    public List<Rigidbody> elements = new List<Rigidbody>();
    public GameObject flowFieldPrefab;
    [Header("Tornado Parameters")]
    public float tornadoStrength = 20f;
    public float turbulenceStrength = 2f; // Intensité de la turbulence
    public float a = 0.5f; // Aspiration
    public float nu = 0.1f; // Viscosité cinématique
    [Header("Tornado Shape Parameters")]
    public float gammaBase = 10f;   // près du sol
    public float gammaTop = 40f;    // en haut
    public float gammaExponent = 1.5f; // forme du cône
    public float tornadoHeight = 30f;
    public GameObject ceilingHeight;
    [Header("FlowField Parameters")]
    public GridType gridType = GridType.Cube;
    public int size = 5;
    public int resolution = 4;
    private List<GameObject> flowField = new List<GameObject>();
    private int lastResolution = 4;

    private void Awake()
    {
        particles = new ParticleSystem.Particle[tornadoParticles.main.maxParticles];
        for (int i = 0; i < resolution; i++)
        {
            GameObject arrow = Instantiate(flowFieldPrefab);
            arrow.SetActive(false);
            flowField.Add(arrow);
        }
    }

    private void Update()
    {
        if (lastResolution != resolution)
        {
            for (int i = 0; i < flowField.Count; i++)
            {
                flowField[i].SetActive(false);
            }
        }
        lastResolution = resolution;
        DrawFloawField();
        ceilingHeight.transform.position = new Vector3(0, tornadoHeight, 0);
    }

    void FixedUpdate()
    {
        foreach (Rigidbody element in elements)
        {
            Vector3 fluidVelocity = ApplyTornado(element.position);
            Vector3 relativeVelocity = fluidVelocity - element.linearVelocity;
            element.AddForce(relativeVelocity * tornadoStrength, ForceMode.Force);
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
        
        Vector3 radial = (elementPos - transformPos).normalized;
        Vector3 tangential = Vector3.Cross(Vector3.up, radial).normalized;
        
        float height = Mathf.Max(0f, z);
        float height01 = Mathf.Clamp01(height / tornadoHeight);
        
        float gammaAtHeight = Mathf.Lerp(
            gammaBase, 
            gammaTop, 
            Mathf.Pow(height01, gammaExponent)
        );
        
        float vr = -a * dist;
        float vtheta = gammaAtHeight / (2 * Mathf.PI * dist) * (1 - Mathf.Exp(- (a * dist * dist) / (2 * nu)));
        float vz = 2 * a * z;

        Vector3 baseVelocity = vr * radial + vtheta * tangential + vz * Vector3.up;
        
        float t = Time.time * 0.5f;
        Vector3 noise = new Vector3(
            Mathf.PerlinNoise(elementPos.x + t, elementPos.y) - 0.5f,
            0,
            Mathf.PerlinNoise(elementPos.z, elementPos.y + t) - 0.5f
        );
        noise *= turbulenceStrength;
        
        return baseVelocity + noise;
    }

    private void DrawFloawField()
    {
        DrawCubeField();
    }

    private void DrawCubeField()
    {
        int index = 0;
        float halfsize = size / 2f;
        float step = (resolution > 1) ? size / (float)(resolution - 1) : 0;
        
        for(int i = 0; i < resolution; i++)
        {
            for(int j = 0; j < resolution; j++)
            {
                for(int k = 0; k < resolution; k++)
                {
                    float posX = (i * step) - halfsize;
                    float posY = (j * step);
                    float posZ = (k * step) - halfsize;
                    
                    Vector3 relativePos = new Vector3(posX, posY, posZ);
                    Vector3 worldPos = transform.position + relativePos;
                    Vector3 flow = ApplyTornado(worldPos);
                    
                    GameObject arrow;
                    if (index == flowField.Count)
                    {
                        arrow = Instantiate(flowFieldPrefab);
                        flowField.Add(arrow);
                    }
                    else
                        arrow = flowField[index];
                    PlaceArrow(arrow, worldPos, flow);
                    index++;
                }
            }
        }
    }
    private void PlaceArrow(GameObject arrow, Vector3 position, Vector3 direction)
    {
        if(!arrow)
            arrow = Instantiate(flowFieldPrefab);
        arrow.transform.position = position;
        arrow.transform.rotation = Quaternion.LookRotation(direction);
        arrow.SetActive(true);
    }
}