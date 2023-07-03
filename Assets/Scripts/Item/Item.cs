using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // Item types
    public enum ItemType
    {
        Ammo,
        Turbo,
        Shield
    }

    public int m_ItemNumber = 1; // Number (Also known as ID)
    public ItemType itemType;    // Type of item

    // Item effects
    public int ammoAmount = 5;            // Ammo amount to add
    public float turboDuration = 10f;     // Turbo duration in seconds

    public float turboSpeedMultiplier = 2f;
    public float turboTurnSpeedMultiplier = 2f;

    // Tank references
    private TankHealth tankHealth;     // for shield
    private TankMovement tankMovement; // for speed
    private TankShooting tankShooting; // for ammo

    public GameObject[] itemPrefab;   // Item prefab to allocate

    private MeshRenderer meshRenderer;
    private Light itemLight;
    private BoxCollider boxCollider;
    private MeshFilter meshFilter;
    private Transform itemTransform;

    private void Awake()
    {
        // Get initial components
        meshRenderer = GetComponent<MeshRenderer>();
        itemLight = GetComponent<Light>();
        boxCollider = GetComponent<BoxCollider>();
        meshFilter = GetComponent<MeshFilter>();
        itemTransform = GetComponent<Transform>();
    }

    // constant item rotation
    void Update()
    {
        this.transform.Rotate(0, 0, 50 * Time.deltaTime); //rotates 50 degrees per second around z axis
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Obtain player properties
            tankHealth   = other.GetComponent<TankHealth>();
            tankMovement = other.GetComponent<TankMovement>();
            tankShooting = other.GetComponent<TankShooting>();

            // Apply the item effect based on the item type
            switch (itemType)
            {
                case ItemType.Ammo: // Add ammo to the tank
                    tankShooting.AddAmmo(ammoAmount);
                    break;

                case ItemType.Turbo: // Activate turbo effect on the player
                    tankMovement.ActivateTurbo(turboDuration, turboSpeedMultiplier, turboTurnSpeedMultiplier);
                    break;

                case ItemType.Shield: // Activate shield effect on the player
                    tankHealth.ActivateShield();
                    break;
            }

            meshRenderer.enabled = false;
            itemLight.enabled = false;
            boxCollider.enabled = false;
            StartCoroutine(ActivateItem(5f));
        }
    }

    // Reset item after being picked up
    private void ResetItemPickup()
    {
        // Reactivate item by changing its type and reusing the prefab
        int randomItem = Random.Range(0, itemPrefab.Length);
        
        MeshRenderer newMeshRenderer = itemPrefab[randomItem].GetComponent<MeshRenderer>();
        Light newItemLight = itemPrefab[randomItem].GetComponent<Light>();
        BoxCollider newBoxCollider = itemPrefab[randomItem].GetComponent<BoxCollider>();
        Item newItemScript = itemPrefab[randomItem].GetComponent<Item>();
        Transform newTransform = itemPrefab[randomItem].GetComponent<Transform>(); // for item scale
        MeshFilter newMeshFilter = itemPrefab[randomItem].GetComponent<MeshFilter>();

        // Assign the new components to the current item
        itemTransform.localScale = newTransform.localScale;
        meshFilter.sharedMesh = newMeshFilter.sharedMesh;
        meshRenderer.sharedMaterials = newMeshRenderer.sharedMaterials;
        itemLight.color = newItemLight.color;
        itemLight.intensity = newItemLight.intensity;
        boxCollider.size = newBoxCollider.size;
        boxCollider.center = newBoxCollider.center;
        boxCollider.isTrigger = newBoxCollider.isTrigger;
        this.itemType = newItemScript.itemType;

        // Enable item components
        meshRenderer.enabled = true;
        itemLight.enabled = true;
        boxCollider.enabled = true;
    }

    public IEnumerator ActivateItem(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ResetItemPickup();
    }
}
