using System.Collections;
using UnityEngine;

public class GhostEffect : MonoBehaviour
{
    [Header("Configurações do Efeito")]
    [Tooltip("Ativa/desativa a geração contínua do rastro de fantasmas.")]
    public bool isGhostActive = true;

    [Tooltip("Intervalo de tempo em segundos entre a criação de cada fantasma.")]
    [SerializeField] private float ghostInterval = 0.05f;

    [Tooltip("Tempo total em segundos que o fantasma leva para sumir completamente.")]
    [SerializeField] private float ghostLifetime = 0.3f;

    [Header("Aparência do Fantasma")]
    [Tooltip("Cor e transparência inicial do fantasma.")]
    [SerializeField] private Color ghostColor = new Color(0.5f, 0.8f, 1f, 0.6f);

    [Tooltip("Material customizado para o fantasma (Opcional, deixe nulo para usar o padrão).")]
    [SerializeField] private Material ghostMaterial;

    [Header("Requisitos de Movimento")]
    [Tooltip("Se verdadeiro, só gera fantasmas se o objeto estiver se movendo.")]
    [SerializeField] private bool onlyWhenMoving = true;
    [SerializeField] private float minVelocityToSpawn = 0.1f;

    private SpriteRenderer sourceSR;
    private Rigidbody2D rb2d;
    private float ghostTimer;

    private void Awake()
    {
        sourceSR = GetComponentInChildren<SpriteRenderer>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!isGhostActive || sourceSR == null) return;

        // Verifica se o objeto está se movendo (caso a opção esteja ativa)
        if (onlyWhenMoving && rb2d != null)
        {
            if (rb2d.linearVelocity.magnitude < minVelocityToSpawn) return;
        }

        ghostTimer += Time.deltaTime;

        if (ghostTimer >= ghostInterval)
        {
            SpawnGhost();
            ghostTimer = 0f;
        }
    }

    public void SpawnGhost()
    {
        if (sourceSR == null || sourceSR.sprite == null) return;

        // 1. Cria o GameObject do Fantasma
        GameObject ghostObj = new GameObject("GhostTrail");
        ghostObj.transform.position = transform.position;
        ghostObj.transform.rotation = transform.rotation;
        ghostObj.transform.localScale = transform.localScale;

        // 2. Configura o SpriteRenderer do Fantasma
        SpriteRenderer ghostSR = ghostObj.AddComponent<SpriteRenderer>();
        ghostSR.sprite = sourceSR.sprite;
        ghostSR.flipX = sourceSR.flipX;
        ghostSR.flipY = sourceSR.flipY;
        ghostSR.sortingLayerID = sourceSR.sortingLayerID;
        ghostSR.sortingOrder = sourceSR.sortingOrder - 1; // Renderiza logo atrás do player

        if (ghostMaterial != null)
        {
            ghostSR.material = ghostMaterial;
        }

        // 3. Inicia o Fade Out e destruição
        StartCoroutine(FadeAndDestroyGhost(ghostObj, ghostSR));
    }

    private IEnumerator FadeAndDestroyGhost(GameObject ghostObj, SpriteRenderer ghostSR)
    {
        float elapsedTime = 0f;
        Color initialColor = ghostColor;

        while (elapsedTime < ghostLifetime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / ghostLifetime;

            // Suaviza o canal Alpha (transparência) até 0
            Color currentColor = initialColor;
            currentColor.a = Mathf.Lerp(initialColor.a, 0f, t);

            if (ghostSR != null)
            {
                ghostSR.color = currentColor;
            }

            yield return null;
        }

        Destroy(ghostObj);
    }

    // Métodos públicos para ativar por gatilhos externos (ex: habilidades/dash/tempo lento)
    public void EnableGhost() => isGhostActive = true;
    public void DisableGhost() => isGhostActive = false;
}