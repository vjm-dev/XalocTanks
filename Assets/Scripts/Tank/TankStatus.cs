using UnityEngine;
using UnityEngine.UI;

public class TankStatus : MonoBehaviour
{
    private bool isAlive;                               // Variable to check if the tank is alive
    public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the tank dies.

    private AudioSource m_ExplosionAudio;               // The audio source to play when the tank explodes.
    private ParticleSystem m_ExplosionParticles;        // The particle system the will play when the tank is destroyed.
    private bool m_Dead;                                // Has the tank been destroyed yet?

    // Shield
    [HideInInspector] public bool hasShield;

    // Light to recognize if the tank has shield
    [HideInInspector] public GameObject lightGameObject;
    [HideInInspector] public Light lightComp;

    // Activate shield and set the light
    public void ActivateShield()
    {
        if (!hasShield) // if it was already activated, don't set the light again
        {
            lightGameObject = new GameObject("ShieldLight");
            lightComp = lightGameObject.AddComponent<Light>();
            lightComp.color = Color.green;
            lightComp.intensity = 4.2f;
            lightGameObject.transform.position = // set the position near to the tank
                            new Vector3(this.transform.position.x, this.transform.position.y + 3, this.transform.position.z);
            lightGameObject.transform.SetParent(this.transform); // set the light parent to the tank
        }
        hasShield = true;
    }

    public void DisableShield()
    {
        // tank shield disabled by default
        hasShield = false;

        // remove the shield light when starting
        Destroy(lightGameObject);
        Destroy(lightComp);
    }

    private void Awake()
    {
        // Instantiate the explosion prefab and get a reference to the particle system on it.
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();

        // Get a reference to the audio source on the instantiated prefab.
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

        // Disable the prefab so it can be activated when it's required.
        m_ExplosionParticles.gameObject.SetActive(false);
    }

    private void Start()
    {
        DisableShield();
    }

    private void OnEnable()
    {
        // When the tank is enabled, reset the tank's status and whether or not it's dead.
        isAlive = true;
        m_Dead = false;
    }


    public void Kill()
    {
        // Don't destroy if shield is activated
        if (hasShield)
        {
            // remove light resources
            Destroy(lightGameObject);
            Destroy(lightComp);
            hasShield = false;
            return;
        }

        // Set as dead
        isAlive = false;

        // If the current status isn't alive and it has not yet been registered, call OnDeath.
        if (!isAlive && !m_Dead)
            OnDeath();
    }

    private void OnDeath()
    {
        // Set the flag so that this function is only called once.
        m_Dead = true;

        // Move the instantiated explosion prefab to the tank's position and turn it on.
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive(true);

        // Play the particle system of the tank exploding.
        m_ExplosionParticles.Play();

        // Play the tank explosion sound effect.
        m_ExplosionAudio.Play();

        // Turn the tank off.
        gameObject.SetActive(false);
    }
}