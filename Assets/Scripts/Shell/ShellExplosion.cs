using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    public LayerMask m_TankMask;                        // Used to filter what the explosion affects, this should be set to "Players".
    public LayerMask m_CactusMask;                      // Used to filter what the explosion affects, this should be set to "Cactuses".
    public ParticleSystem m_ExplosionParticles;         // Reference to the particles that will play on explosion.
    public AudioSource m_ExplosionAudio;                // Reference to the audio that will play on explosion.
    public float m_ExplosionForce = 1000f;              // The amount of force added to a tank at the centre of the explosion.
    public float m_MaxLifeTime = 2f;                    // The time in seconds before the shell is removed.
    public float m_ExplosionRadius = 0.5f;              // The maximum distance away from the explosion tanks can be and are still affected.

    private void Start()
    {
        // If it isn't destroyed by then, destroy the shell after it's lifetime.
        Destroy(gameObject, m_MaxLifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        LayerMask currentMask = (GamemodeController.IsSinglePlayer) ? m_CactusMask : m_TankMask;
        // Collect all the colliders in a sphere from the shell's current position to a radius of the explosion radius.
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, currentMask);

        // Go through all the colliders...
        for (int i = 0; i < colliders.Length; i++)
        {
            // ... and find their rigidbody.
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

            // If they don't have a rigidbody, go on to the next collider.
            if (!targetRigidbody)
                continue;

            // Add an explosion force.
            targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

            // In single player mode, only it will detect cactuses in the scene. In two player mode, only the tanks.
            if (GamemodeController.IsSinglePlayer)
            {
                // Find the Cactus script associated with the rigidbody.
                Cactus cactus = targetRigidbody.GetComponent<Cactus>();

                // If there is Cactus script attached to the gameobject, destroy the cactus.
                if (cactus)
                    cactus.DestroyCactus();
            }
            else
            {
                // Find the TankStatus script associated with the rigidbody.
                TankStatus targetTank = targetRigidbody.GetComponent<TankStatus>();

                // If there is TankStatus script attached to the gameobject, destroy the tank.
                if (targetTank)
                    targetTank.Kill();
            }
        }

        // Unparent the particles from the shell.
        m_ExplosionParticles.transform.parent = null;

        // Play the particle system.
        m_ExplosionParticles.Play();

        // Play the explosion sound effect.
        m_ExplosionAudio.Play();

        // Once the particles have finished, destroy the gameobject they are on.
        Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);

        // Destroy the shell.
        Destroy(gameObject);
    }
}