using UnityEngine;

public class WaypointNavigator : MonoBehaviour
{
    
    CharacterNavigationController controller;
    public Waypoint currentWaypoint;

    int direction;
    void Awake()
    {
        controller = GetComponent<CharacterNavigationController>();

    }

     
    void Start()
    {
        //generate random number for direction value
        direction = Mathf.RoundToInt(Random.Range(0f, 1f));
        controller.SetDestination(currentWaypoint.GetPosition());
    }


    void Update()
    {
   

        if (controller.reachedDestination)
        {
            bool shouldBranch = false;

            if(currentWaypoint.branches !=null && currentWaypoint.branches.Count > 0)
            {
                shouldBranch = Random.Range(0f, 1f) <= currentWaypoint.branchRatio ? true : false;
            }

            if (shouldBranch)
            {
                currentWaypoint = currentWaypoint.branches[Random.Range(0, currentWaypoint.branches.Count - 1)];
            }
            else
            {
                if (direction == 0)
                {
                    if (currentWaypoint.nextWaypoint != null)
                    {
                        //tells the CharacterNavigationController to update to the next waypoint if it has reached its destination
                        currentWaypoint = currentWaypoint.nextWaypoint;
                    }
                    else
                    {
                        currentWaypoint = currentWaypoint.previousWaypoint;
                        direction = 1;
                    }
                }
                else if (direction == 1)
                {
                    if (currentWaypoint.previousWaypoint != null)
                    {
                        currentWaypoint = currentWaypoint.previousWaypoint;
                    }
                    else
                    {
                        currentWaypoint = currentWaypoint.nextWaypoint;
                        direction = 0;
                    }
                }
            }
                controller.SetDestination(currentWaypoint.GetPosition());
        }
    }
}
