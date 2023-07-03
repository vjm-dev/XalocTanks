using UnityEngine;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour
{
    public float m_StartingHealth = 100f;               // The amount of health each tank starts with.
    public Slider m_Slider;                             // The slider to represent how much health the tank currently has.
    public Image m_FillImage;                           // The image component of the slider.
    public Color m_FullHealthColor = Color.green;       // The color the health bar will be when on full health.
    public Color m_ZeroHealthColor = Color.red;         // The color the health bar will be when on no health.
    public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the tank dies.

    private AudioSource m_ExplosionAudio;               // The audio source to play when the tank explodes.
    private ParticleSystem m_ExplosionParticles;        // The particle system the will play when the tank is destroyed.
    private float m_CurrentHealth;                      // How much health the tank currently has.
    private bool m_Dead;                                // Has the tank been reduced beyond zero health yet?

    // shield
    [HideInInspector] public bool hasShield;

    // light to recognize if the tank has shield
    [HideInInspector] public GameObject lightGameObject;
    [HideInInspector] public Light lightComp;

    // Activate shield and set the light
    public void ActivateShield()
    {
        if (!hasShield) // if it was already activated, don't set the light again
        {
            lightGameObject = new GameObject("The Light");
            lightComp = lightGameObject.AddComponent<Light>();
            lightComp.color = Color.green;
            lightComp.intensity = 4.2f;
            lightGameObject.transform.position = // set the position near to the tank
                            new Vector3(this.transform.position.x, this.transform.position.y + 3, this.transform.position.z);
            lightGameObject.transform.SetParent(this.transform); // set the parent position to the tank
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
        // When the tank is enabled, reset the tank's health and whether or not it's dead.
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;

        // Update the health slider's value and color.
        SetHealthUI();
    }


    public void TakeDamage(float amount)
    {
        // Don't destroy/receive damage if shield is activated
        if (hasShield)
        {
            // remove the light
            Destroy(lightGameObject);
            Destroy(lightComp);
            hasShield = false;
            return;
        }

        // Reduce current health by the amount of damage done.
        m_CurrentHealth -= amount;

        // Change the UI elements appropriately.
        SetHealthUI();

        // If the current health is at or below zero and it has not yet been registered, call OnDeath.
        if (m_CurrentHealth <= 0f && !m_Dead)
        {
            OnDeath();
        }
    }


    private void SetHealthUI()
    {
        // Set the slider's value appropriately.
        m_Slider.value = m_CurrentHealth;

        // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
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