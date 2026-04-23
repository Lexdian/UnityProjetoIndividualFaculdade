using UnityEngine;

public class ParallaxLoop : MonoBehaviour
{
    public float speed = 0.5f;
    private Material mat;
    private float offset;

    void Start()
    {
        // Pega o material do MeshRenderer
        mat = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        // O Time.time aqui faz o movimento ser constante e infinito
        offset += speed * Time.deltaTime;

        // Aplica o deslocamento na textura. 
        // Como a imagem é "Repeat", ela volta para o começo sozinha.
        mat.mainTextureOffset = new Vector2(0, offset);
    }
}