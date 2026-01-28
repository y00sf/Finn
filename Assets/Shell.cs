using System.Collections;
using UnityEngine;

public class Shell : MonoBehaviour
{
    [SerializeField] private Transform SpawanTransform;
    [SerializeField] private FishingManager fishingManager;
    [SerializeField] private float spawnInterval = 1f;

    private GameObject currentBait;

    void Start()
    {
        StartCoroutine(SpawnerLoop());
    }

    private void SpawnBait()
    {
        if (fishingManager == null || fishingManager.baits == null || fishingManager.baits.Length == 0) return;

        int rand = Random.Range(0, fishingManager.baits.Length);
        var prefab = fishingManager.baits[rand].baitPrefab;
        if (prefab == null) return;

        currentBait = Instantiate(prefab, SpawanTransform.position, SpawanTransform.rotation);
    }

    IEnumerator SpawnerLoop()
    {
        yield return new WaitForSeconds(spawnInterval);

        while (true)
        {
            if (currentBait == null)
            {
                SpawnBait();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
