using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class ItemManager : MonoBehaviour
{
    public Transform m_SpawnPoint;
    [HideInInspector] public GameObject m_Instance;
    [HideInInspector] public Item m_Item;

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
