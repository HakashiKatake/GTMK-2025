using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    [Header("Timing Settings")]
    public float intermissionAmount = 60f;
    public float spiritFightDuration = 300f;
    public float playerFightDuration = 300f;
    
    [Header("Game Objects")]
    public GameObject spiritPrefab;
    public GameObject playerObject;
    public GameObject spiritObject;
    public GameObject playerBot;
    
    [Header("Spawn Settings")]
    public Camera mainCamera;
    public float spawnDistance = 20f;
    
    private List<GameObject> spawnedSpirits = new List<GameObject>();
    private Coroutine currentSequence;
    private bool isPlayerFight = false;
    
    void Start()
    {
        StartCoroutine(GameLoop());
    }
    
    IEnumerator GameLoop()
    {
        while (true)
        {
            yield return StartCoroutine(IntermissionPhase());
            yield return StartCoroutine(SpiritFightPhase());
        }
    }
    
    IEnumerator IntermissionPhase()
    {
        playerBot.SetActive(false);
        yield return new WaitForSeconds(intermissionAmount);
    }
    
    IEnumerator SpiritFightPhase()
    {
        isPlayerFight = false;
        playerBot.SetActive(false);
        StartCoroutine(SpawnSpirits());
        
        float timer = 0f;
        while (timer < spiritFightDuration)
        {
            if (!playerObject.activeInHierarchy)
            {
                SwapToSpirit();
                yield return StartCoroutine(PlayerFightPhase());
                yield break;
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (playerObject.activeInHierarchy)
        {
            playerObject.SetActive(false);
            SwapToSpirit();
            yield return StartCoroutine(PlayerFightPhase());
        }
    }
    
    IEnumerator PlayerFightPhase()
    {
        isPlayerFight = true;
        playerBot.SetActive(true);
        DestroyAllSpirits();
        
        float timer = 0f;
        while (timer < playerFightDuration)
        {
            if (!spiritObject.activeInHierarchy)
            {
                SwapToPlayer();
                yield break;
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (spiritObject.activeInHierarchy)
        {
            spiritObject.SetActive(false);
            SwapToPlayer();
        }
    }
    
    IEnumerator SpawnSpirits()
    {
        while (!isPlayerFight)
        {
            SpawnSpiritRandomly();
            yield return new WaitForSeconds(Random.Range(5f, 15f));
        }
    }
    
    void SpawnSpiritRandomly()
    {
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject spirit = Instantiate(spiritPrefab, spawnPos, Quaternion.identity);
        spawnedSpirits.Add(spirit);
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        Vector3 cameraPos = mainCamera.transform.position;
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * spawnDistance;
        return cameraPos + offset;
    }
    
    void SwapToSpirit()
    {
        Vector3 playerPos = playerObject.transform.position;
        spiritObject.transform.position = playerPos;
        spiritObject.SetActive(true);
    }
    
    void SwapToPlayer()
    {
        Vector3 spiritPos = spiritObject.transform.position;
        playerObject.transform.position = spiritPos;
        playerObject.SetActive(true);
    }
    
    void DestroyAllSpirits()
    {
        foreach (GameObject spirit in spawnedSpirits)
        {
            if (spirit != null)
                Destroy(spirit);
        }
        spawnedSpirits.Clear();
    }
}