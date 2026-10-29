using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPiecesController : MonoBehaviour
{
    public List<GameObject> partesEscudo = new List<GameObject>();

    [Header("Áudio")]
    [SerializeField] private AudioClip estouro;
    [SerializeField] private AudioSource audioSource;

    [Header("Configurações de Impacto Explosivo")]
    [SerializeField] private float forcaMinima = 16f;
    [SerializeField] private float forcaMaxima = 30f;
    [SerializeField] private float torqueMaximo = 350f;
    [SerializeField] private float dragLinear = 3.5f; // Faz o caco desacelerar rápido após o impulso inicial

    private List<Vector3> posicoesLocaisIniciais = new List<Vector3>();
    private List<Quaternion> rotacoesIniciais = new List<Quaternion>();

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Salva a formação original das partes do escudo no espaço local
        foreach (var parte in partesEscudo)
        {
            if (parte != null)
            {
                posicoesLocaisIniciais.Add(parte.transform.localPosition);
                rotacoesIniciais.Add(parte.transform.localRotation);
            }
        }
    }

    private void OnEnable()
    {
        ResetarPecas();

        // Aplica a força explosiva amplificada
        ExplosiveForceManager.AplicarExplosao2D(partesEscudo, transform.position, forcaMinima, forcaMaxima, torqueMaximo);
        foreach (var parte in partesEscudo)
        {
            if (parte != null)
            {
                StartCoroutine(parte.GetComponent<AutoFadeOut>().IniciarContagemFade());
            }

            if (audioSource != null && estouro != null)
            {
                audioSource.PlayOneShot(estouro);
            }

            StartCoroutine(Timer());
        }
    }

    private void ResetarPecas()
    {
        for (int i = 0; i < partesEscudo.Count; i++)
        {
            if (partesEscudo[i] != null && i < posicoesLocaisIniciais.Count)
            {
                // Reseta para o formato do escudo montado
                partesEscudo[i].transform.localPosition = posicoesLocaisIniciais[i];
                partesEscudo[i].transform.localRotation = rotacoesIniciais[i];

                // Zera forças do reuso do pool e ajusta o drag para o estouro ter o "pop" ideal
                Rigidbody2D rb = partesEscudo[i].GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.linearDamping = dragLinear; // No Unity 2022/anteriores use: rb.drag = dragLinear;
                }
            }
        }
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(0.7f);
        gameObject.SetActive(false);
    }
}