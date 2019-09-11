This vehicle script is designed to be implemented alongside GameDevGuide's waypoint navigation system.

Configure the waypoint parent, # of vehicles to spawn and vehicle prefabs in the inspector, spawner will select random vehicles from the prefab array.

You will need something like this in your vehicles Start function to gather waypoints.

void Start()
{
        //get the parent of the assigned waypoint
        path = currentWaypoint.transform.parent;

        //gather the transforms underneath 'path' gameobject
        pathTransforms = path.GetComponentsInChildren<Transform>();

        nodes = new List<Transform>();
       
        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != path.transform)
            {
                nodes.Add(pathTransforms[i]);
            }
        }

        //assign current waypoint target to be node 0 in our list
        currentNode = currentWaypoint.GetSiblingIndex();
    }