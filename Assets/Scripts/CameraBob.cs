using UnityEngine;

public class CameraBob : MonoBehaviour
{
    [Header("Bob Settings")]
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobAmplitude = 0.5f;
    
    [Header("Advanced Settings")]
    [SerializeField] private float horizontalBobSpeed = 0.8f;
    [SerializeField] private float horizontalBobAmplitude = 0.3f;
    [SerializeField] private float rotationBobSpeed = 1.2f;
    [SerializeField] private float rotationBobAmplitude = 1f;
    
    [Header("Randomization")]
    [SerializeField] private bool useRandomOffset = true;
    [SerializeField] private float randomOffsetRange = 10f;
    
    private Vector3 originalPosition;
    private float originalRotation;
    private float verticalOffset;
    private float horizontalOffset;
    private float rotationOffset;
    
    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.eulerAngles.z;
        
        if (useRandomOffset)
        {
            verticalOffset = Random.Range(0f, randomOffsetRange);
            horizontalOffset = Random.Range(0f, randomOffsetRange);
            rotationOffset = Random.Range(0f, randomOffsetRange);
        }
    }
    
    void Update()
    {
        BobCamera();
    }
    
    void BobCamera()
    {
        float time = Time.time;
        
        float verticalBob = Mathf.Sin((time * bobSpeed) + verticalOffset) * bobAmplitude;
        float horizontalBob = Mathf.Sin((time * horizontalBobSpeed) + horizontalOffset) * horizontalBobAmplitude;
        float rotationBob = Mathf.Sin((time * rotationBobSpeed) + rotationOffset) * rotationBobAmplitude;
        
        Vector3 newPosition = originalPosition + new Vector3(horizontalBob, verticalBob, 0f);
        float newRotation = originalRotation + rotationBob;
        
        transform.position = newPosition;
        transform.rotation = Quaternion.Euler(0f, 0f, newRotation);
    }
    
    [ContextMenu("Reset Camera Position")]
    public void ResetCameraPosition()
    {
        transform.position = originalPosition;
        transform.rotation = Quaternion.Euler(0f, 0f, originalRotation);
    }
    
    public void SetBobIntensity(float intensity)
    {
        bobAmplitude = intensity;
        horizontalBobAmplitude = intensity * 0.6f;
        rotationBobAmplitude = intensity * 2f;
    }
}