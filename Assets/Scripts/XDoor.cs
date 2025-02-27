
using System.Collections;
using UnityEngine;

public class XDoor : MonoBehaviour
{
    public GameObject doorPrefab;

    private void Start()
    {
        StartCoroutine(SpawnDoorCoroutine());
    }

    private IEnumerator SpawnDoorCoroutine()
    {
        while (true)
        {
            float spawnTime = Random.Range(30f, 60f);
            yield return new WaitForSeconds(spawnTime);

            Vector3 spawnPositionMin = new Vector3(-3.35f, 0.9409993f, -6.21f);
            Vector3 spawnPositionMax = new Vector3(3.41f, 0.9409993f, -3.71f);
            Vector3 spawnPosition = new Vector3(
                Random.Range(spawnPositionMin.x, spawnPositionMax.x),
                Random.Range(spawnPositionMin.y, spawnPositionMax.y),
                Random.Range(spawnPositionMin.z, spawnPositionMax.z)
            );

            Instantiate(doorPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
