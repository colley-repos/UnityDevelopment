using System.Collections.Generic;
using UnityEngine;
public class CarEngine : MonoBehaviour
{
    #region Declarations
    [SerializeField] private bool headlightsOn;

    [Header("Status")]
    [SerializeField] private float targetSpeed;             //speed of vehicle detected via front sensor
    [SerializeField] private float currentSpeed;            //calculated from RL wheel RPM
    [SerializeField] private bool driveEngaged;             //apply max speed
    [SerializeField] private bool isBraking = false;        //apply full brake
    [SerializeField] private bool avoiding = false;         //turn to avoid obstacle
    [SerializeField] private bool recoveryCheck = false;    //flagged when vehicle is idle for too long
    [SerializeField] private bool isFollowing;
    public bool inCombat = false;                           //disable navigation when in combat
    private bool isCornering = false;                     //avoid full throttle in corners

    //sensors used to detect and brake for traffic and detect upcoming turns
    [Header("Sensors")]
    [Range(0.5f, 10f)]
    public float sensorLength = 3f;
    [SerializeField] private CarEngine leadingVehicle;
    [SerializeField] private float obstacleMultiplier = 0f;
    private float distanceToCurrentNode;  //check distance between this object and it's current waypoint
    private float speedAdjustedSensorLength;               //used for speed matching leading vehicle
    private float speedAdjustedBrakeTorque;
    LayerMask detectLayers;

    private FloatTimer stallReverseTimer = new FloatTimer(6f);
    private FloatTimer stallTimer = new FloatTimer(6f);
    private FloatTimer recoveryExpiredTimer = new FloatTimer(1f);
    private float randomSteerAngle;
    private int skipWaypointCounter = 0;

    [Header("Navigation")]
    public Transform currentWaypoint;
    public Transform path;
    private List<Transform> nodes;
    private int currentNode = 0;
    private int nextNode = 1;
    private Transform[] pathTransforms;
    private float targetSteerAngle = 0;

    [Header("Engine Stats")]
    public bool is4WD;
    public float maxCorneringSpeed = 30f;
    public float maxSteerAngle = 45f;
    public float turnSpeed = 2.5f;
    public float throttle = 100f;
    public float speed = 70f;
    public float maxSpeed = 100f;
    public float maxBrakeTorque = 150f;
    public Vector3 centerOfMass;
    private float maxMotorTorque;

    //to be configured in inspector
    [Header("Connections")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    [Header("Lights")]
    public Transform headLights;
    public Transform brakeLights;
    #endregion

    #region Init
    private void Start()
    {
        //assign our random steer angle value, applied during recovery operation
        randomSteerAngle = Random.Range(-10f, 10f);
        maxMotorTorque = maxSpeed;
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;

        if(headlightsOn) { headLights.gameObject.SetActive(true); }

        if (currentWaypoint == null) { return; }

        //get the parent of the assigned waypoint
        path = currentWaypoint.transform.parent;
        
        //gather array of of the transforms underneath path gameobject
        pathTransforms = path.GetComponentsInChildren<Transform>();

        nodes = new List<Transform>();

        //create integer, and update integer count until it matches the length of the nodes list
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
    #endregion

    #region Update
    private void FixedUpdate()
    {
        if (inCombat)  { return; }
        if (nodes == null)  { return; }

        SensorRangeCheck();
        SensorRay();
        ApplySteer();
        LerpToSteerAngle();
        Avoiding();
        Braking();
        CheckWaypointDistance();
        //CheckCornerDistanceAndAngle();
        StallCheck();
        ApplyMaxSpeed();
    }
    #endregion

    #region Sensors
    private void SensorRangeCheck()
    {
        speedAdjustedSensorLength = sensorLength + (currentSpeed / 3f);
    }
    
    private void SensorRay()
    {
        Vector3 sensorStartPos = transform.position + new Vector3(0f, 0.15f, 0f);
        RaycastHit leadingVehicleSensorRayHit;
        RaycastHit brakeSensorRayHit;
        detectLayers = LayerMask.GetMask("NPCVehicle", "Pedestrian", "Player", "Enemy", "RCC");

        if (Physics.Raycast(sensorStartPos, transform.forward, out brakeSensorRayHit, speedAdjustedSensorLength / 2, detectLayers, QueryTriggerInteraction.Collide))
        {
            obstacleMultiplier += 0.01f * Time.deltaTime;

            if (brakeSensorRayHit.distance < (speedAdjustedSensorLength / 2))
            {
                speedAdjustedBrakeTorque = maxBrakeTorque * obstacleMultiplier;
                Debug.DrawLine(sensorStartPos, brakeSensorRayHit.point, Color.red, 1f);
                isBraking = true;
            }
            else
            {
                speedAdjustedBrakeTorque = maxBrakeTorque;
                isBraking = false;
            }
        }
        else
        {
            isBraking = false;
            obstacleMultiplier = 0;

            if (Physics.Raycast(sensorStartPos, transform.forward, out leadingVehicleSensorRayHit, speedAdjustedSensorLength, detectLayers, QueryTriggerInteraction.Collide))
            {

                Debug.DrawLine(sensorStartPos, leadingVehicleSensorRayHit.point, Color.green, 0.2f);

                if (leadingVehicleSensorRayHit.transform.GetComponentInParent<CarEngine>() != null && !isCornering)
                {
                    leadingVehicle = leadingVehicleSensorRayHit.transform.GetComponentInParent<CarEngine>();
                    Debug.DrawRay(sensorStartPos, leadingVehicleSensorRayHit.point, Color.blue, 1f);

                    //match leading vehicles speed
                    targetSpeed = leadingVehicle.GetComponent<CarEngine>().currentSpeed + Random.Range(-5f, 5f);
                    maxMotorTorque = targetSpeed;
                    isFollowing = true;
                }
            }
            else if (!Physics.Raycast(sensorStartPos, transform.forward, speedAdjustedSensorLength, detectLayers, QueryTriggerInteraction.Collide))
            {
                //reset target speed when no leading vehicle detected
                leadingVehicle = null;
                isFollowing = false;
            }
        }
    }

    void FrontAngleSensor()
    {
        
    }
    #endregion

    #region Drive System
    private void ApplySteer()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position);
        float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;
        targetSteerAngle = newSteer;
    }

    private void LerpToSteerAngle()
    {
        wheelFL.steerAngle = Mathf.Lerp(wheelFL.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
        wheelFR.steerAngle = Mathf.Lerp(wheelFR.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
    }

    private void CheckCornerDistanceAndAngle()
    {
        if (currentNode == nodes.Count - 1)
        {
            nextNode = 0;
        }
        else 
        {
            nextNode = currentNode + 1;
        }

        if (Vector3.Distance(transform.position, nodes[nextNode].position) < 12f && Vector3.Angle(transform.forward, nodes[nextNode].forward) > 15f && !isFollowing)
        {
            maxMotorTorque = maxCorneringSpeed;
            isCornering = true;
        }
        else
        {
            isCornering = false;
        }
    }

    private void ApplyMaxSpeed()
    {
        currentSpeed = ((2 * Mathf.PI * wheelRL.radius * wheelRL.rpm * 60 / 1000) + (2 * Mathf.PI * wheelRR.radius * wheelRR.rpm * 60 / 1000) + (2 * Mathf.PI * wheelFL.radius * wheelFL.rpm * 60 / 1000) + (2 * Mathf.PI * wheelFR.radius * wheelFR.rpm * 60 / 1000)) / 4;

        if (currentSpeed < maxSpeed && !isFollowing && !isBraking && !isCornering && !recoveryCheck)
        {
            driveEngaged = true;
            maxMotorTorque = maxSpeed;
            wheelFL.motorTorque = maxMotorTorque + Random.Range(-5, 5);
            wheelFR.motorTorque = maxMotorTorque + Random.Range(-5, 5);

            if (is4WD)
            {
                wheelRL.motorTorque = maxMotorTorque + Random.Range(-5, 5);
                wheelRR.motorTorque = maxMotorTorque + Random.Range(-5, 5);
            }
        }
        else
        {
            driveEngaged = false;
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;

            if (is4WD)
            {
                wheelRL.motorTorque = 0;
                wheelRR.motorTorque = 0;
            }
        }
    }

    private void Avoiding()
    {

    }

    private void Braking()
    {
        if (isBraking)
        {
            brakeLights.gameObject.SetActive(true);
            maxMotorTorque = 0;
            wheelFR.brakeTorque = speedAdjustedBrakeTorque;
            wheelFL.brakeTorque = speedAdjustedBrakeTorque;
        }
        else
        {
            brakeLights.gameObject.SetActive(false);
            maxMotorTorque = maxSpeed;
            wheelFR.brakeTorque = 0f;
            wheelFL.brakeTorque = 0f;
        }
    }

    private void CheckWaypointDistance()
    {
        distanceToCurrentNode = Vector3.Distance(transform.position, nodes[currentNode].position);

        if (distanceToCurrentNode < 3f)
        {
            if (currentNode == nodes.Count - 1)
            {
                currentNode = 0;
            }
            else
            {
                currentNode++;
            }
        }
    }
    #endregion

    private void StallCheck()
    {
        var pos = transform.position;
        stallTimer.Tick();
        
        if (stallTimer.ready && currentSpeed < 5f && leadingVehicle == null)
        {
            stallReverseTimer.Tick();
            recoveryExpiredTimer.Tick();

            if (!recoveryCheck)
            {
                recoveryCheck = true;
                pos = transform.position;
            }

            isBraking = false;
            //skipWaypointCounter++;
            wheelFL.motorTorque = -80;
            wheelFR.motorTorque = -80;
            targetSteerAngle = randomSteerAngle;
            
            if (Vector3.Distance(pos, transform.position) > 3)
            {
                if (targetSteerAngle >= 1)
                {
                    targetSteerAngle -= 20f;
                }
                else if (targetSteerAngle <= -1)
                {
                    targetSteerAngle += 20f;
                }

                recoveryCheck = false;
                stallTimer.Reset();
                stallReverseTimer.Reset();
                recoveryExpiredTimer.Reset();
            }
            else if (recoveryExpiredTimer.ready)
            {
                recoveryCheck = false;
                stallTimer.Reset();
                stallReverseTimer.Reset();
                recoveryExpiredTimer.Reset();
            }
            
            if (stallReverseTimer.ready)
            {
                //reset all
                stallTimer.Reset();
                stallReverseTimer.Reset();
                recoveryExpiredTimer.Reset();
                randomSteerAngle = Random.Range(-45f, 45f);


            }
            /*
            if (skipWaypointCounter == 2)
            {
                if (currentNode == nodes.Count - 1)
                {
                    skipWaypointCounter = 0;            
                    currentNode = 0;
                }
                else
                {
                    skipWaypointCounter = 0;
                    currentNode++;
                }
            }
            */
        }
        else if (currentSpeed >= 30)
        {
            stallTimer.Reset();
            stallReverseTimer.Reset();
            recoveryExpiredTimer.Reset();
            recoveryCheck = false;
        }
    }
}