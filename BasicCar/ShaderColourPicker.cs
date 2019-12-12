using UnityEngine;

public class ShaderColourPicker : MonoBehaviour
{
    public int[] materialArrayIndex;
    public Transform[] vehicleBody;
    public Material paintMaterial;
    public Color[] colours;

    void Start()
    {
        Material randomPaint = new Material(paintMaterial);
        var randomColour = colours[Random.Range(0, colours.Length)];

        foreach (Transform t in vehicleBody)
        {
            for (int i = 0; i < materialArrayIndex.Length; i++)
            {
                t.GetComponent<Renderer>().materials[materialArrayIndex[i]].SetColor("_BaseColor", randomColour);
                t.GetComponent<Renderer>().materials[materialArrayIndex[i]] = randomPaint;
            }
        }
    }
}