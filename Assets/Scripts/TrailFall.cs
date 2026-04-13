using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailFall : MonoBehaviour
{
    public float fallSpeed = 2f;

    private TrailRenderer trail;

    void Start()
    {
        trail = GetComponent<TrailRenderer>();
    }

    void LateUpdate()
    {
        int count = trail.positionCount;

        Vector3[] positions = new Vector3[count];
        trail.GetPositions(positions);

        for (int i = 0; i < count; i++)
        {
            positions[i] += Vector3.down * fallSpeed * Time.deltaTime;
        }

        trail.SetPositions(positions);
    }
}