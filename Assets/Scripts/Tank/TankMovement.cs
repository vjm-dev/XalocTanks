using System.Collections;
using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public int m_PlayerNumber = 1;

    public float m_Speed = 12f;          // speed when moving forward and back
    public float m_TurnSpeed = 180f;     // speed of turns in degrees per second

    public float m_PitchRange = 0.2f;    // pitch of the engine noises
    private float m_OriginalPitch;       // pitch of the audio source, only at the start of the scene

    private Rigidbody rb;

    // audio
    public AudioSource m_MovementAudio;  // used to play engine sounds
    public AudioClip m_EngineIdling;     // when the tank isn't moving
    public AudioClip m_EngineDriving;    // when the tank is moving

    private string m_MovementAxisName; // Input axis for moving forward and back.
    private string m_TurnAxisName;     // Input axis for turning.
    private float m_MovementInputValue;         // The current value of the movement input.
    private float m_TurnInputValue;             // The current value of the turn input.

    // for turbo
    private bool hasTurbo;
    private float originalSpeed;
    private float originalTurnSpeed;
    private Coroutine coroutine; // to control the turbo duration

    private void Awake()
    {
        // Initialize the speed
        originalSpeed = m_Speed;
        originalTurnSpeed = m_TurnSpeed;

        // set the coroutine in order to restart the turbo duration
        coroutine = StartCoroutine(TurboEnd(10f));
        rb = GetComponent<Rigidbody>();
    }

    // turbo management
    public void ActivateTurbo(float seconds, float turboSpeedMultiplier, float turboTurnSpeedMultiplier)
    {
        // From here, restart coroutine if the player picked up turbo item again before ending
        StopCoroutine(coroutine);

        // Don't multiplicate the speed if the player has turbo and picks up turbo item again
        if (!hasTurbo)
        {
            m_Speed *= turboSpeedMultiplier;
            m_TurnSpeed *= turboTurnSpeedMultiplier;
        }
        hasTurbo = true;

        coroutine = StartCoroutine(TurboEnd(seconds));
    }

    public void StopTurbo()
    {
        hasTurbo = false;
        m_Speed = originalSpeed;
        m_TurnSpeed = originalTurnSpeed;
        StopCoroutine(coroutine);
    }

    private IEnumerator TurboEnd(float seconds)
    {
        // reset the original speed
        yield return new WaitForSeconds(seconds);
        m_Speed = originalSpeed;
        m_TurnSpeed = originalTurnSpeed;
        hasTurbo = false;
    }
    
    private void OnEnable()
    {
        // When the tank is turned on, make sure it's not kinematic.
        rb.isKinematic = false;

        // Also reset the input values.
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }


    private void OnDisable()
    {
        // When the tank is turned off, set it to kinematic so it stops moving
        rb.isKinematic = true;
    }


    private void Start()
    {
        // Keep original speed values
        originalSpeed = m_Speed; 
        originalTurnSpeed = m_TurnSpeed;
        
        // The axes names are based on player number.
        m_MovementAxisName = "Vertical" + m_PlayerNumber;
        m_TurnAxisName = "Horizontal" + m_PlayerNumber;

        // Store the original pitch of the audio source.
        m_OriginalPitch = m_MovementAudio.pitch;
    }


    private void Update()
    {
        // Store the value of both input axes.
        m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
        m_TurnInputValue = Input.GetAxis(m_TurnAxisName);

        EngineAudio();
    }


    private void EngineAudio()
    {
        // If there is no input (the tank is stationary)...
        if (Mathf.Abs(m_MovementInputValue) < 0.1f && Mathf.Abs(m_TurnInputValue) < 0.1f)
        {
            // ... and if the audio source is currently playing the driving clip...
            if (m_MovementAudio.clip == m_EngineDriving)
            {
                // ... change the clip to idling and play it.
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
        else
        {
            // Otherwise if the tank is moving and if the idling clip is currently playing...
            if (m_MovementAudio.clip == m_EngineIdling)
            {
                // ... change the clip to driving and play.
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
    }


    private void FixedUpdate()
    {
        // Adjust the rigidbodies position and orientation in FixedUpdate.
        Move();
        Turn();
    }


    private void Move()
    {
        // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

        // Apply this movement to the rigidbody's position.
        rb.MovePosition(rb.position + movement);
    }


    private void Turn()
    {
        // Determine the number of degrees to be turned based on the input, speed and time between frames.
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

        // Make this into a rotation in the y axis.
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        // Apply this rotation to the rigidbody's rotation.
        rb.MoveRotation(rb.rotation * turnRotation);
    }
}