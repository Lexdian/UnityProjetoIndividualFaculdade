using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    private Vector3 initialScale;

    private void Awake()
    {
        // Salva a escala exata que você definiu no Inspector do Prefab
        initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        // Força a escala original toda vez que o impacto sai da Pool
        transform.localScale = initialScale;
    }

    private void LateUpdate()
    {
        // Trava final no ciclo de renderização: impede que qualquer outro script ou animação altere a escala
        if (transform.localScale != initialScale)
        {
            transform.localScale = initialScale;
        }
    }
}