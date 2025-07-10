using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // Item types
    public enum ItemType
    {
        AMMO,
        TURBO,
        SHIELD
    }

    public ItemType itemType;    // Type of item

    // Item effects
    public int ammoAmount = 5;            // Ammo amount to add
    public float turboDuration = 10f;     // Turbo duration in seconds

    public float turboSpeedMultiplier = 2f;
    public float turboTurnSpeedMultiplier = 2f;

    // Tank references
    private TankStatus tankStatus;     // for shield
    private TankMovement tankMovement; // for speed
    private TankShooting tankShooting; // for ammo

    public GameObject[] itemPrefab;   // Item prefab to allocate

    private MeshRenderer meshRenderer;
    private Light itemLight;
    private BoxCollider boxCollider;
    private MeshFilter meshFilter;
    private Transform itemTransform;

    private struct ItemData
    {
        public Vector3 scale;
        public Mesh mesh;
        public Material[] materials;
        public Color lightColor;
        public float lightIntensity;
        public Vector3 colliderSize;
        public Vector3 colliderCenter;
        public bool isTrigger;
        public ItemType itemType;
    }

    // Item data cache
    private ItemData[] itemDataCache;

    private void Awake()
    {
        // Get initial components
        meshRenderer    = GetComponent<MeshRenderer>();
        itemLight       = GetComponent<Light>();
        boxCollider     = GetComponent<BoxCollider>();
        meshFilter      = GetComponent<MeshFilter>();
        itemTransform   = transform;

        CacheItemData();
    }

    private void CacheItemData()
    {
        itemDataCache = new ItemData[itemPrefab.Length];
        for (int i = 0; i < itemPrefab.Length; i++)
        {
            GameObject prefab = itemPrefab[i];
            ItemData data = new ItemData
            {
                scale           = prefab.transform.localScale,
                mesh            = prefab.GetComponent<MeshFilter>().sharedMesh,
                materials       = prefab.GetComponent<MeshRenderer>().sharedMaterials,
                lightColor      = prefab.GetComponent<Light>().color,
                lightIntensity  = prefab.GetComponent<Light>().intensity,
                colliderSize    = prefab.GetComponent<BoxCollider>().size,
                colliderCenter  = prefab.GetComponent<BoxCollider>().center,
                isTrigger       = prefab.GetComponent<BoxCollider>().isTrigger,
                itemType        = prefab.GetComponent<Item>().itemType
            };
            itemDataCache[i] = data;
        }
    }

    // constant item rotation
    void Update()
    {
        transform.Rotate(0, 0, 50 * Time.deltaTime); // rotates 50 degrees per second around z axis
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Obtain player properties
            tankStatus   = other.GetComponent<TankStatus>();
            tankMovement = other.GetComponent<TankMovement>();
            tankShooting = other.GetComponent<TankShooting>();

            // Apply the item effect based on the item type
            switch (itemType)
            {
                case ItemType.AMMO: // Add ammo to the tank
                    tankShooting.AddAmmo(ammoAmount);
                    break;

                case ItemType.TURBO: // Activate turbo effect on the player
                    tankMovement.ActivateTurbo(turboDuration, turboSpeedMultiplier, turboTurnSpeedMultiplier);
                    break;

                case ItemType.SHIELD: // Activate shield effect on the player
                    tankStatus.ActivateShield();
                    break;
            }

            // Disable item components
            meshRenderer.enabled = false;
            itemLight.enabled = false;
            boxCollider.enabled = false;
            StartCoroutine(ActivateItem(5f));
        }
    }

    private void SpawnRandomItem(List<ItemData> validItems)
    {
        // Reactivate item by changing its type and reusing the prefab
        int randomItem = Random.Range(0, validItems.Count);
        ItemData data = validItems[randomItem];

        // Assign the new components to the current item
        itemTransform.localScale = data.scale; // for item scale
        meshFilter.sharedMesh = data.mesh;
        meshRenderer.sharedMaterials = data.materials;
        itemLight.color = data.lightColor;
        itemLight.intensity = data.lightIntensity;
        boxCollider.size = data.colliderSize;
        boxCollider.center = data.colliderCenter;
        boxCollider.isTrigger = data.isTrigger;
        itemType = data.itemType;

        // Enable item components
        meshRenderer.enabled = true;
        itemLight.enabled = true;
        boxCollider.enabled = true;
    }

    // Reset item after being picked up
    private void ResetItemPickup()
    {
        // Create a list of available item types based on game mode
        List<ItemType> availableTypes = new List<ItemType>();
        
        if (GamemodeController.IsSinglePlayer)
        {
            // Only allow Ammo and Turbo
            availableTypes.Add(ItemType.AMMO);
            availableTypes.Add(ItemType.TURBO);
        }
        else
        {
            // Allow all items
            availableTypes.Add(ItemType.AMMO);
            availableTypes.Add(ItemType.TURBO);
            availableTypes.Add(ItemType.SHIELD);
        }

        // Filter items that match the available types
        List<ItemData> validItems = new List<ItemData>();
        foreach (ItemData itemData in itemDataCache)
        {
            if (availableTypes.Contains(itemData.itemType))
            {
                validItems.Add(itemData);
            }
        }

        if (validItems.Count == 0)
        {
            Debug.LogError("No valid items available for spawning!");
            return;
        }

        // Spawn random item according to validItems list
        SpawnRandomItem(validItems);
    }

    public IEnumerator ActivateItem(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ResetItemPickup();
    }
}
