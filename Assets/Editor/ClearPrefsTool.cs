using UnityEditor;
using UnityEngine;
using Unity.Services.Authentication;

public static class ClearPrefsTool
{
    [MenuItem("Tools/Clear All PlayerPrefs & UGS Auth")]
    public static void ClearPrefsAndUGS()
    {
        // 1. Apaga todas as chaves do PlayerPrefs (onde o token anônimo do UGS fica salvo)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2. Se estiver em Play Mode e conectado ao UGS, encerra a sessão e remove o token da memória
        if (Application.isPlaying && AuthenticationService.Instance != null)
        {
            try
            {
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    AuthenticationService.Instance.SignOut(clearCredentials: true);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Aviso ao deslogar do UGS: {e.Message}");
            }
        }

        Debug.Log("PlayerPrefs e credenciais do UGS limpos com sucesso!");
    }
}