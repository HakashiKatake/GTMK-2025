using System.Collections.Generic;
using UnityEngine;

public class Tootiredtotype : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform image;
        public float delay = 0.5f; // Lower = faster movement, 1 = same as camera
    }

    public List<ParallaxLayer> layers = new List<ParallaxLayer>();
    public Camera mainCamera;

    private Vector3 previousCameraPosition;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        previousCameraPosition = mainCamera.transform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = mainCamera.transform.position - previousCameraPosition;

        foreach (ParallaxLayer layer in layers)
        {
            if (layer.image != null)
            {
                float parallaxX = deltaMovement.x * (1 - layer.delay);
                float parallaxY = deltaMovement.y * (1 - layer.delay);
                layer.image.position += new Vector3(parallaxX, parallaxY, 0);
            }
        }

        previousCameraPosition = mainCamera.transform.position;
    }
}
