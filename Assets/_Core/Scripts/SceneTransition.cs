// Плавный переход (fade) между сценами через DOTween
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("Настройки перехода")]
    public Image fadeImage;

    [Tooltip("Длительность затемнения в секундах")]
    public float fadeDuration = 0.4f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Начинаем с прозрачного экрана
        if (fadeImage != null)
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
    }

    // Затемнить → загрузить сцену → осветлить
    public void FadeToScene(string sceneName)
    {
        // Блокируем клики во время перехода
        fadeImage.raycastTarget = true;

        fadeImage.DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                SceneManager.LoadScene(sceneName);
                SceneManager.sceneLoaded += OnSceneLoaded;
            });
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // отписываемся

        // Осветляем после загрузки
        fadeImage.DOFade(0f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() => fadeImage.raycastTarget = false);
    }
}