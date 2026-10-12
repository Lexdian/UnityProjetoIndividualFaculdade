using UnityEngine;

public class KeepRotation : MonoBehaviour
{
    private Quaternion rotacaoInicial;

    void Start()
    {
        rotacaoInicial = transform.rotation;
    }

    void LateUpdate()
    {
        transform.rotation = rotacaoInicial;
    }
}
