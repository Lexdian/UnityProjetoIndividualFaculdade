using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public void GameobjectDisable()
    {
        transform.SetParent(null);
        gameObject.SetActive(false);
    }
}
