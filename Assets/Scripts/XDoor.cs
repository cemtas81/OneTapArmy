
using System.Collections;
using UnityEngine;


public class XDoor : MonoBehaviour
{
    public GameObject doorPrefab;
    
    private float spawnTime;
    private BoxCollider boxCollider;
    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();  
        StartCoroutine(SpawnDoorCoroutine());
        spawnTime = Random.Range(20f, 60f);
    }

    private IEnumerator SpawnDoorCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnTime);

            Vector3 spawnPosition = GetRandomPositionInBoxCollider(boxCollider);
            Instantiate(doorPrefab, spawnPosition, Quaternion.identity);
        }
    }

   
    private Vector3 GetRandomPositionInBoxCollider(BoxCollider boxCollider)
    {
        Vector3 center = boxCollider.center + boxCollider.transform.position;
        Vector3 size = boxCollider.size;

        float x = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
        float y = -4.3f;
        float z = Random.Range(center.z - size.z / 2, center.z + size.z / 2);

        return new Vector3(x, y, z);
    }
}
