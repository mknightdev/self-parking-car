using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public List<MeshRenderer> carParts;

    // Start is called before the first frame update
    void Start()
    {
        // Set material as random colour
        Color randomColour = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1.0f);
        ApplyColour(randomColour);
    }

    void ApplyColour(Color _colour)
    {
        Material generatedMaterial;
        generatedMaterial = new Material(Shader.Find("Standard"));
        generatedMaterial.SetColor("_Color", _colour);

        // Apply the new colour to the body of the car
        for (int i = 0; i < carParts.Count; i++)
        {
            carParts[i].material = generatedMaterial;
        }
    }
}
