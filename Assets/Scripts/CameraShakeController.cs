using UnityEngine;

using Cinemachine;

public class CameraShakeController : MonoBehaviour
{
    public static CameraShakeController Instance { get; private set; }

    private CinemachineImpulseSource source;

    void Awake()
    {
        if (Instance == null) Instance = this;

        // Tenta pegar o componente de qualquer forma
        source = GetComponent<CinemachineImpulseSource>();

        if (source == null)
        {
            Debug.LogWarning("CinemachineImpulseSource não encontrado no objeto " + gameObject.name);
        }
    }

    public void Shake(float intensidade = 1f)
    {
        if (source != null)
        {
            source.GenerateImpulse(intensidade);
        }
    }
}