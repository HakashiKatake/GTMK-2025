using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WindManager : MonoBehaviour
{
    public GameObject windPrefab;
    public float particleLifetime = 3f;
    public float spawnInterval = 0.2f;

    private Camera mainCamera;
    private List<GameObject> currentParticles = new List<GameObject>();

    void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(WindLoop());
    }

    IEnumerator WindLoop()
    {
        while (true)
        {
            int amountToSpawn = Random.Range(1, 5);
            yield return StartCoroutine(SpawnBatch(amountToSpawn));

            while (currentParticles.Count > 0)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator SpawnBatch(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = GetRandomPointInCameraView();
            GameObject wind = Instantiate(windPrefab, spawnPos, Quaternion.identity, transform);
            currentParticles.Add(wind);
            StartCoroutine(DestroyAfterTime(wind, particleLifetime));
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    Vector2 GetRandomPointInCameraView()
    {
        float zDistance = 10f;
        Vector3 randomViewportPos = new Vector3(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            zDistance
        );
        Vector3 worldPos = mainCamera.ViewportToWorldPoint(randomViewportPos);
        return new Vector2(worldPos.x, worldPos.y);
    }

    IEnumerator DestroyAfterTime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        if (currentParticles.Contains(obj))
            currentParticles.Remove(obj);
        Destroy(obj);
    }
}
