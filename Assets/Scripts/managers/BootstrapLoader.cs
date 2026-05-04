using UnityEngine;
using UnityEngine.SceneManagement;

public static class BootstrapLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if (Object.FindFirstObjectByType<BootStrapper>() != null)
            return;

        SceneManager.LoadScene("bootstrap", LoadSceneMode.Additive);
    }
}
