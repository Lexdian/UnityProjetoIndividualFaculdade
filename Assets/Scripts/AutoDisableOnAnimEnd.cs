using UnityEngine;

public class AutoDisableOnAnimEnd : MonoBehaviour
{
    // Este método será chamado pelo evento da animação
    public void DesativarObjeto()
    {
        gameObject.SetActive(false);
    }
}