using UnityEngine;

public class Follow : MonoBehaviour
{
    public Vector3 offset;
    public Transform target;
    private Vector3 targetPosition;

    // Update is called once per frame
    void LateUpdate()
    {
        targetPosition = target? target.position : Vector3.zero;
        this.transform.position = targetPosition + offset;
    }
}
