using UnityEngine;
using VolumetricFogAndMist2;

public class FogRaiser : MonoBehaviour
{
    public VolumetricFog sceneFog;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("Space"))
        {
            Debug.Log("button pressed");
            //sceneFog.
        }

    }
}
