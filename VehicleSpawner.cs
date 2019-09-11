using System.Collections;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    //prefabs to instantiate
    public GameObject[] vehiclePrefabs;    //game object to spawn
    public Transform waypointRoute;       //the waypoint root on which to spawn our vehicles

    private GameObject randomVehiclePrefab;
    
    public int noOfVehicles;         //number of objects to spawn
    

    void Start()
    {
        StartCoroutine(Spawn());
                       
    }

    //coroutine iterated through waypoints for every spawn number
    IEnumerator Spawn()
    {


        int count = 0;

        while (count < noOfVehicles)
        {
            //select random waypoint as spawn target
            Transform child = waypointRoute.GetChild(Random.Range(0, waypointRoute.transform.childCount - 1));
            RaycastHit hitInfo;
            LayerMask npcVehicle = LayerMask.GetMask("NPCVehicle");

            //we cast a sphere over the waypoint and if it collides with an object under the NPCVehicle layermask we return
            if (Physics.SphereCast(child.position, 15f, -child.forward, out hitInfo, 1f, npcVehicle))
            {
                Debug.Log("spawn collision avoided");
                yield return new WaitForEndOfFrame();
            }

            randomVehiclePrefab = vehiclePrefabs[(Random.Range(0, vehiclePrefabs.Length))];

            //instantiate vehicle prefab and declare it 'obj'
            GameObject obj = Instantiate(randomVehiclePrefab);
            obj.transform.parent = gameObject.transform;


            //move object to selected waypoint and match waypoint orientation
            obj.transform.position = child.position;
            obj.transform.rotation = child.rotation;


            //assign selected waypoint to vehicle pathing
            obj.GetComponent<CarEngine>().currentWaypoint = child.transform;
            

            yield return new WaitForSecondsRealtime(1f);

            count++;
        }
    }
}
