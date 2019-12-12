using UnityEngine;

[RequireComponent(typeof(CarEngine))]
[RequireComponent(typeof(AudioSource))]
public class CarAudio : MonoBehaviour
{
    private AudioSource audio_out;
    private CarEngine engine;
    [SerializeField] float modifier;

    void Start()
    {
        audio_out = GetComponent<AudioSource>();
        engine = GetComponent<CarEngine>();
    }

    void Update()
    {
        var currentThrottle = engine.speed;
        float soundPitchDifference = 1f;
        if (currentThrottle > 0.04f)  { soundPitchDifference = 1.5f; }

        if (currentThrottle > 0.055f) { soundPitchDifference = 1.8f; }

        if (currentThrottle > 0.075f) { soundPitchDifference = 2f; }

        if (currentThrottle > 0.08f) { soundPitchDifference = 2.5f; }

        audio_out.pitch = (currentThrottle * 35 / soundPitchDifference) * modifier + .6f;
    }
}
