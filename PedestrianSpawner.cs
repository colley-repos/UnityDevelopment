using System.Collections;
using UnityEngine;

public class PedestrianSpawner : MonoBehaviour
{
    public GameObject[] pedestrianPrefabs;
    public int pedestriansToSpawn;

    private GameObject _randomPedestrianPrefab;

    public void Start()
    {
        StartCoroutine(Spawn());
    }

    //coroutine iterated through waypoints for every spawn number
    IEnumerator Spawn()
    {
        int count = 0;
        while (count < pedestriansToSpawn)
        {
            _randomPedestrianPrefab = pedestrianPrefabs[(Random.Range(0, pedestrianPrefabs.Length))];
            GameObject obj = Instantiate(_randomPedestrianPrefab);

            //select random waypoint as spawn target
            Transform child = transform.GetChild(Random.Range(0, transform.childCount - 1));

            obj.GetComponent<WaypointNavigator>().currentWaypoint = child.GetComponent<Waypoint>();
            obj.transform.position = child.position;

            yield return new WaitForEndOfFrame();

            count++;
        }
    }
}