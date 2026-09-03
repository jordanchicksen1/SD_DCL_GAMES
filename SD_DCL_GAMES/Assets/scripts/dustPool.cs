using System.Collections.Generic;
using UnityEngine;

public class DustPool : MonoBehaviour
{
    public static DustPool Instance { get; private set; }

    [Header("Dust")]
    [SerializeField] private GameObject dustPrefab;

    [SerializeField] private int poolSize = 20;

    private readonly List<DustCloud> pool = new List<DustCloud>();

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(
                dustPrefab,
                transform
            );

            obj.SetActive(false);

            DustCloud cloud = obj.GetComponent<DustCloud>();

            if (cloud != null)
            {
                pool.Add(cloud);
            }
            else
            {
                Debug.LogError(
                    "DustPool: Dust prefab is missing DustCloud component."
                );
            }
        }
    }

    public void Spawn(Vector3 position)
    {
        foreach (DustCloud cloud in pool)
        {
            if (!cloud.gameObject.activeSelf)
            {
                cloud.Spawn(position);
                return;
            }
        }
    }
}