using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StarManager : MonoBehaviour
{
    public GameObject starPrefab;
    public float particleLifetime = 4f;
    public float spawnInterval = 0.05f;
    public float spawnScale = 1f;
    public float minimumDistance = 0.5f;

    private Camera mainCamera;
    private List<GameObject> currentParticles = new List<GameObject>();
    private List<Vector2> spawnPositions = new List<Vector2>();

    void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(StarLoop());
    }

    void OnDestroy()
    {
        // Stop all coroutines when the object is being destroyed
        StopAllCoroutines();
    }

    IEnumerator StarLoop()
    {
        while (this != null && mainCamera != null)
        {
            int amountToSpawn = Random.Range(40, 61);
            spawnPositions.Clear();
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
        int attempts = 0;

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = Vector2.zero;
            bool valid = false;

            while (!valid && attempts < 100)
            {
                attempts++;
                spawnPos = GetRandomPointInCameraView();
                
                // If camera is destroyed, break out of the loop
                if (mainCamera == null)
                {
                    yield break;
                }
                
                valid = true;

                foreach (Vector2 existing in spawnPositions)
                {
                    if (Vector2.Distance(existing, spawnPos) < minimumDistance)
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (!valid)
                continue;

            spawnPositions.Add(spawnPos);
            GameObject star = Instantiate(starPrefab, spawnPos, Quaternion.identity, transform);
            star.transform.localScale = Vector3.one * spawnScale;
            currentParticles.Add(star);
            StartCoroutine(DestroyAfterTime(star, particleLifetime));
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    Vector2 GetRandomPointInCameraView()
    {
        // Check if camera still exists
        if (mainCamera == null)
        {
            return Vector2.zero;
        }
        
        float zDistance = 10f;
        Vector3 viewportPos = new Vector3(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            zDistance
        );
        Vector3 worldPos = mainCamera.ViewportToWorldPoint(viewportPos);
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
