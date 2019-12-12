using System.Collections;
using UnityEngine;

public class VehicleAVFX : MonoBehaviour
{
    public GameObject smokePrefab;
    public GameObject explosionPrefab;

    private float vehicleDamage;
    private bool trackingVehicleDamage;
    private bool smokePlaying;
    RCC_CarControllerV3 carController;

    void Start()
    {
        carController = GetComponent<RCC_CarControllerV3>();
        if (carController != null)
        {
            trackingVehicleDamage = true;
        }
    }

    void Update()
    {
        TrackDamage();
    }

    void TrackDamage()
    {
        if (trackingVehicleDamage)
        {
            vehicleDamage = carController.damageMultiplier;

            if(vehicleDamage >= 15f)
            {
                SmokeEffect();
            }

            if(vehicleDamage >= 20f)
            {
                StartCoroutine(ExplosionEffect());
            }
        }
    }

    void SmokeEffect()
    {
        if(!smokePlaying)
        {
            GameObject smoke = Instantiate(smokePrefab);
            smoke.transform.position = transform.position;
            smokePlaying = true;
        }
    }

    IEnumerator ExplosionEffect()
    {
        Debug.Log("Vehicle Destroyed " + this.gameObject.name);
        yield return new WaitForEndOfFrame();
    }
}