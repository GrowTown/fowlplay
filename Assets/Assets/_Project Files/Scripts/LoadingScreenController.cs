using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{

    public Slider loadingSlider;

    private string sceneToLoad;

    public void StartLoading(string sceneName)
    {
        sceneToLoad = sceneName;
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float progress = 0f;

        while (!operation.isDone)
        {
            float target = Mathf.Clamp01(operation.progress / 0.9f);
            progress = Mathf.MoveTowards(progress, target, Time.deltaTime);
            loadingSlider.value = progress;

            if (progress >= 1f && operation.progress >= 0.9f)
            {
                //yield return new WaitForSeconds(0.1f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}


