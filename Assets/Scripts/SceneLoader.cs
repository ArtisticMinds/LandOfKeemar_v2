using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    public GameObject loadingScreen;
    public static SceneLoader instance;
    public CanvasGroup canvasGroup;
    public void Awake()
    {
        instance = this;
       // DontDestroyOnLoad(gameObject);
    }
    public void LoadScene(string sceneToLoad)
    {
        StartCoroutine(StartLoad(sceneToLoad));
    }
    public IEnumerator StartLoad(string sceneToLoad)
    {
        loadingScreen.SetActive(true);
        yield return StartCoroutine(FadeLoadingScreen(1, 1));
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad,LoadSceneMode.Additive);
        while (!operation.isDone)
        {
            yield return null;
        }
        yield return StartCoroutine(FadeLoadingScreen(0, 1));
        loadingScreen.SetActive(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
    }
    IEnumerator FadeLoadingScreen(float targetValue, float duration)
    {
        float startValue = canvasGroup.alpha;
        float time = 0;
        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = targetValue;

    }
}