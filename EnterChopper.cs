using Cinemachine;
using UnityEngine;

public class EnterChopper : MonoBehaviour
{
    //collect the player animator to control enter/exit vehicle animations
    [SerializeField]
    public Animator playerAnim;

    //used to update vehicle camera target
    [SerializeField]
    public CinemachineVirtualCamera chopperCam;

    //we get the player transform to relocate the player object in world space
    [SerializeField]
    public Transform playerObject;

    private AudioSource audioSource;

    private void Awake()
    {
        

    }

    public void ChopperEmbark()
    {
        

        //set driving camera target and follow to new player vehicle
        chopperCam.LookAt = this.transform;
        chopperCam.Follow = this.transform;

        //hide character renderer via animator
        playerAnim.SetBool("isChopper", true);
        
        //activate vehicle scripts
        this.gameObject.GetComponent<HelicopterController>().enabled = true;
        this.gameObject.GetComponentInChildren<HeliRotorController>().enabled = true;
        this.transform.GetComponent<ControlPanel>().enabled = true;
        this.transform.GetComponent<AudioSource>().enabled = true;

        //deactivate player movement scripts
        //playerObject.GetComponent<PlayerMovement>().enabled = false;
        playerObject.GetComponent<CharacterController>().enabled = false;
        playerObject.GetComponent<PlayerInteraction>().busy = false;
    }

    public void ChopperDismbark()
    {
        //find a vector 3 next to the vehicle
        //***if speed > 'x' need to apply brake when exiting then eject player after timer
        Vector3 exitPos = this.transform.position + new Vector3(0, 0, -3);

        //move player back to vehicle location next to driver door
        playerObject.transform.position = exitPos;

        //switch camera back to locomotion cam handled via anim controller/cinemachine state driven cam
        playerAnim.SetBool("isChopper", false);

        //deactivate vehicle scripts
        this.gameObject.GetComponent<HelicopterController>().enabled = false;
        this.gameObject.GetComponentInChildren<HeliRotorController>().enabled = false;
        this.transform.GetComponent<ControlPanel>().enabled = false;
        this.transform.GetComponent<AudioSource>().enabled = false;

        //reactivate player scripts
        //playerObject.GetComponent<PlayerMovement>().enabled = true;
        playerObject.GetComponent<CharacterController>().enabled = true;
        playerObject.GetComponent<PlayerInteraction>().busy = false;
    }

}
