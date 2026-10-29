using UnityEngine;

public class SidewaysShooter : MonoBehaviour
{
    [Header("Configurações dos Projéteis")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 6f;

    [Tooltip("Ajuste do ângulo dos disparos em graus (ex: 90f faz as balas saírem a 90° e -90°, como Cima e Baixo).")]
    [SerializeField] private float anguloAjuste = 90f;

    [Header("Tempo de Ativação")]
    [Tooltip("Tempo em segundos até disparar e se destruir (0 = dispara imediatamente no spawn)")]
    [SerializeField] private float delayParaDisparo = 1.0f;

    [Tooltip("Se verdadeiro, as direções seguem a rotação do próprio objeto. Se falso, usa direções globais.")]
    [SerializeField] private bool usarDirecaoLocal = true;

    [Header("Pooling / Destruição")]
    [SerializeField] private bool destroyObject = true;

    private float timer = 0f;
    private bool disparou = false;
    private bool isGiant = false;

    public void SetGiant(bool giant)
    {
        isGiant = giant;
    }

    private void OnEnable()
    {
        timer = 0f;
        disparou = false;
        isGiant = false;
    }

    private void Update()
    {
        if (disparou) return;

        timer += Time.deltaTime;
        if (timer >= delayParaDisparo)
        {
            DispararELimpar();
        }
    }

    private void DispararELimpar()
    {
        disparou = true;

        if (bulletPrefab != null)
        {
            float baseAngle = usarDirecaoLocal ? transform.eulerAngles.z : 0f;

            // Calcula as duas direções opostas com base no ângulo ajustado
            Vector2 direcao1 = CriarVetorPorAngulo(baseAngle + anguloAjuste);
            Vector2 direcao2 = CriarVetorPorAngulo(baseAngle - anguloAjuste);

            CriarProjetil(direcao1);
            CriarProjetil(direcao2);
        }

        if (destroyObject)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private Vector2 CriarVetorPorAngulo(float anguloEmGraus)
    {
        float rad = anguloEmGraus * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private void CriarProjetil(Vector2 direcao)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // Verifica se este objeto pai é gigante (diretamente ou via BulletMovement)
        bool serGigante = isGiant;
        if (!serGigante && TryGetComponent<BulletMovement>(out BulletMovement myBm))
        {
            serGigante = myBm.IsGiant;
        }

        BulletMovement bm = bullet.GetComponent<BulletMovement>();
        if (bm != null)
        {
            bm.SetDirection(direcao, bulletSpeed);
            bm.SetGiant(serGigante); // Aplica a escala de gigante se necessário
        }

        // Se o novo projétil também tiver um SidewaysShooter, repassa a flag
        if (bullet.TryGetComponent<SidewaysShooter>(out SidewaysShooter subShooter))
        {
            subShooter.SetGiant(serGigante);
        }
    }
}