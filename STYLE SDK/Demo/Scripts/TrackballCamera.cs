using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TrackballCamera : MonoBehaviour
{
    const float defaultDistance = 1f;
    const float minDistanceFactor = .1f;
    const float maxDistanceFactor = 5;

    const float initialDistance = 1f;
    const float initialYaw = 205f;
    const float initialPitch = -30f;
    
	public float yawSensitivity = 1;
	public float pitchSensitivity = 1;

    [Range(.01f,.5f)]
    public float scrollSensitivity = .1f;
    
    float distance = initialDistance;
    float yaw = initialYaw;
    float pitch = initialPitch;
    Bounds? target;
    float targetSize;
    Vector3? lastMousePosition;

    Camera myCamera;
    
    public void SetTarget(Bounds bounds) {
        target = bounds;
        targetSize = bounds.extents.magnitude*2;
        ResetPerspective();
        SetCameraParameters();
    }

    public void UnsetTarget() {
        target = null;
        targetSize = defaultDistance;
        ResetPerspective();
        SetCameraParameters();
    }
    
    void Start() {
        myCamera = GetComponent<Camera>();
    }
    
    void LateUpdate () {
        var mouseBtn = Input.GetMouseButton (0);

        var scroll = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scroll*scrollSensitivity,minDistanceFactor,maxDistanceFactor);
        if (mouseBtn) {
            var pos = Input.mousePosition;
            if(lastMousePosition.HasValue) {
                yaw = (yaw + (pos.x-lastMousePosition.Value.x)*yawSensitivity) % 360;
                pitch = Mathf.Clamp(pitch+(pos.y-lastMousePosition.Value.y)*pitchSensitivity,-80,80);
            }
            lastMousePosition = pos;
        } else {
            lastMousePosition = null;
        }
        transform.rotation = Quaternion.AngleAxis(yaw,Vector3.up) * Quaternion.AngleAxis(pitch,Vector3.left);
        var dir = transform.forward;

        var pivot = Vector3.zero;
        
        if (target.HasValue) {
            pivot = target.Value.center;
        }
        transform.position = pivot - dir*(distance*targetSize);
    }

    void ResetPerspective() {
        distance = initialDistance;
        yaw = initialYaw;
        pitch = initialPitch;
    }

    void SetCameraParameters() {
        myCamera.nearClipPlane = targetSize * .0001f;
        myCamera.farClipPlane = targetSize * (maxDistanceFactor+1);
    }
}