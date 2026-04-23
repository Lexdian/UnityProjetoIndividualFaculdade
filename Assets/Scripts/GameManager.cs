using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Configurações de Pause")]
    [SerializeField] private GameObject painelPause;
    private bool jogoPausado = false;

    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (jogoPausado)
                RetomarJogo();
            else
                PausarJogo();
        }
    }

    public void PausarJogo()
    {
        jogoPausado = true;
        painelPause.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RetomarJogo()
    {
        jogoPausado = false;
        painelPause.SetActive(false);
        Time.timeScale = 1f;
    }
}
