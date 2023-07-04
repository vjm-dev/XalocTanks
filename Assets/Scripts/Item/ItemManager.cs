using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class ItemManager : MonoBehaviour
{
    [HideInInspector] public Transform m_SpawnPoint;
    [HideInInspector] public GameObject m_Instance;
    [HideInInspector] public Item m_Item;

    // Get the transform from the gameObject itself at the start of the game
    private void Start()
    {
        m_SpawnPoint = GetComponent<Transform>();
    }

    public void Setup()
    {
        m_Item = m_Instance.GetComponent<Item>();
    }

    // used at the start of the round
    public void ResetItem()
    {
        m_Instance.transform.position = m_SpawnPoint.position;
        m_Instance.transform.rotation = m_SpawnPoint.rotation;
        
        m_Instance.SetActive(false);
        m_Instance.SetActive(true);

        // reset and wait to load in scene
        m_Instance.GetComponent<BoxCollider>().enabled = false;
        m_Instance.GetComponent<Light>().enabled = false;
        m_Instance.GetComponent<MeshRenderer>().enabled = false;
        StartCoroutine(m_Item.ActivateItem(5f));
    }
}
