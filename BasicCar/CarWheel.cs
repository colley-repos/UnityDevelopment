using UnityEngine;

public class CarWheel : MonoBehaviour
{ 
    public WheelCollider targetWheel;
    private Vector3 wheelPosition = new Vector3();
    private Quaternion wheelRotation = new Quaternion();

    private void Update()
    {
        GameObject parent = GetComponentInParent<Transform>().gameObject;

        GameObject grandParent = parent.GetComponentInParent<Transform>().gameObject;
        /*
         * debug script for catching incomplete configurations
        if (targetWheel == null)
        {
            Debug.Log("this car needs wheel assigned ", grandParent);
        }
        */
        targetWheel.GetWorldPose(out wheelPosition, out wheelRotation);
       
    transform.position = wheelPosition;
    transform.rotation = wheelRotation;
    }
}
