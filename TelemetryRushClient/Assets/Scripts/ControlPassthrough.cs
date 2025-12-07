using UnityEngine;

public class ControlPassthrough : MonoBehaviour {

    [SerializeField] private Vector3 defaultPosition;
    [SerializeField] private Vector3 defaultRotation;

    public void IsPassthroughActive(bool passthroughActive) {
        if (passthroughActive) {
            transform.SetPositionAndRotation(defaultPosition, Quaternion.Euler(defaultRotation.x, defaultRotation.y, defaultRotation.z));
        }
    }
}
