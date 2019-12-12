using UnityEngine;
using Cinemachine;
public class EnterVehicle : MonoBehaviour
{
    public Transform driverDoor;
    public Transform driverSeat;
    public Transform chassis;

    [Header("Occupants")]
    public Transform driver;
    public Transform passengerFR;
    public Transform passengerRL;
    public Transform passengerRR;

    //player interaction script passes us the playerObject
    [HideInInspector] public Transform playerObject;
    
    //used to update vehicle camera target
    [HideInInspector] public CinemachineVirtualCamera driveCam;
    [HideInInspector] public CinemachineVirtualCamera inCarCam;
    [HideInInspector] public Animator playerAnim;

    //navmesh moves player to vehicle
    private FloatTimer animWaitTimer = new FloatTimer(3.5f);
    private FloatTimer exitWaitTimer = new FloatTimer(3f);
    private RCC_CarControllerV3 _carController;
    private PlayerInteraction playerInteraction;
    private Animator _driverDoorAnim;
    private bool exiting = false;
    private bool isDriving = false;
    private bool entering = false;
    private PoliceSiren _siren;

    private void Start()
    {
        _driverDoorAnim = driverDoor.GetComponent<Animator>();
        _carController = GetComponent<RCC_CarControllerV3>();
        _siren = GetComponent<PoliceSiren>();
    }

    private void FixedUpdate()
    {

    }

    private void Update()
    {
        if (playerObject != null && !exiting && !isDriving)
        {
            NavigateToVehicle();

            if (entering)
            {
                Debug.Log("enter timer start");
                animWaitTimer.Tick();
                
                if (animWaitTimer.ready)
                {
                    //move player object under vehicle
                    playerObject.transform.parent = chassis.transform;
                                        
                    StartCar();
                    isDriving = true;
                    playerInteraction.isDriving = true;
                    entering = false;
                    animWaitTimer.Reset();
                }
            }
        }
        else if (exiting)
        {
            exitWaitTimer.Tick();

            if (exitWaitTimer.ready)
            {
                exitWaitTimer.Reset();
                Debug.Log("exit timer ready");
                Disembark();
            }
        }

        if (isDriving && !entering && !exiting)
        {
            playerObject.transform.position = driverSeat.position;
            playerObject.transform.Rotate(transform.forward);
        }
    }

    public void MoveToVehicle()
    {
        playerInteraction = playerObject.GetComponent<PlayerInteraction>();
    }

    private void NavigateToVehicle()
    {
        float distanceToDoor = Vector3.Distance(playerObject.transform.position, driverDoor.position);
        
        if (distanceToDoor < 2f && !isDriving)
        {
            entering = true;
            playerAnim.SetFloat("MovementSpeed", 0f);
            playerInteraction.playerNav.enabled = false;
            //playerObject.GetComponent<CharacterController>().enabled = false;

            //disable player controls
            playerInteraction.DisableCharacterControls();

            var lookTarget = this.transform.position - new Vector3(0, 1.5f, 0);
            playerObject.transform.LookAt(lookTarget);
            playerInteraction.playerNav.enabled = false;

            //play enter vehicle animation
            playerAnim.SetBool("isDriving", true);
            _driverDoorAnim.SetTrigger("OpenDoor");
            //_driverDoorAnim.ResetTrigger("OpenDoor");
        }
        else
        {
            playerAnim.SetFloat("MovementSpeed", 0.5f);
        }
    }

    private void StartCar()
    {
        //activate vehicle scripts
        _carController.canControl = true;
        _carController.StartEngine();

        //update player movement scripts
        playerInteraction.busy = false;
        playerInteraction.playerNav.enabled = false;

        //tell driving cameras to follow new player vehicle
        playerInteraction.cameraBrain.enabled = true;
        driveCam = playerInteraction.driveCam;
        inCarCam = playerInteraction.inCarCam;
        driveCam.LookAt = this.transform;
        driveCam.Follow = this.transform;
        driveCam.Priority = 11;
        inCarCam.LookAt = this.transform;
        inCarCam.Follow = this.transform;

        if (_siren != null) { _siren.enabled = true; }
    }

    public void StopCar()
    {
        //***if speed > 'x' need to apply brake when exiting then eject player after timer

        //switch camera back to locomotion cam handled via anim controller/cinemachine state driven cam
        _driverDoorAnim.SetTrigger("OpenDoor");
        //_driverDoorAnim.ResetTrigger("OpenDoor");
        playerAnim.SetBool("isDriving", false);
        isDriving = false;
        exiting = true;
        _carController.canControl = false;
        _carController.KillEngine();
    }

    private void Disembark()
    {
        playerInteraction.ResetRotation();
        playerInteraction.busy = false;
        playerInteraction.isDriving = false;
        playerInteraction.ragdollRoot.gameObject.SetActive(true);
        playerInteraction.thirdPersonInput.enabled = true;

        playerObject.transform.parent = null;
        playerObject = null;

        driveCam.Priority = 10;
        exiting = false;

        playerInteraction.EnableCharacterControls();

        if (_siren != null) { _siren.enabled = false; }
    }

    public void StartCarNPC(Transform destination)
    {
        //NPC embark controlled via movement script
        _carController.StartEngine();
        var aiCarController = GetComponent<RCC_AICarController>();
        aiCarController.enabled = true;
        aiCarController.targetChase = destination;
    }
}