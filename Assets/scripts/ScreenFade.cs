using UnityEngine;
using System.Collections;

public enum ScreenState { Normal, Sleeping, Dead, Flashbanged }

public class ScreenFade : MonoBehaviour
{
    public static ScreenFade Instance;
    public static ScreenState State = ScreenState.Normal;

    private float alpha = 0f;
    private Material glMaterial;
    private Coroutine currentFadeCoroutine;
    private int sleepCallCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        State = ScreenState.Normal;
        DontDestroyOnLoad(gameObject);
        CreateGLMaterial();
        Debug.Log("[ScreenFade] 初期化完了");
    }

    void CreateGLMaterial()
    {
        glMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        glMaterial.hideFlags = HideFlags.HideAndDontSave;
        glMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glMaterial.SetInt("_Cull", 0);
        glMaterial.SetInt("_ZWrite", 0);
        glMaterial.SetInt("_ZTest", 0);
    }

    void OnEnable()
    {
        Camera.onPostRender += DrawOverlay;
    }

    void OnDisable()
    {
        Camera.onPostRender -= DrawOverlay;
    }

    void DrawOverlay(Camera cam)
    {
        if (cam != Camera.main) return;
        if (alpha <= 0f) return;
        if (glMaterial == null) CreateGLMaterial();

        GL.PushMatrix();
        GL.LoadOrtho();
        glMaterial.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(new Color(0f, 0f, 0f, alpha));
        GL.Vertex3(0f, 0f, 0f);
        GL.Vertex3(1f, 0f, 0f);
        GL.Vertex3(1f, 1f, 0f);
        GL.Vertex3(0f, 1f, 0f);
        GL.End();
        GL.PopMatrix();
    }

    public void FadeOutIn(float duration, System.Action onBlack = null, System.Action onComplete = null)
    {
        sleepCallCount++;
        Debug.Log($"[ScreenFade] FadeOutIn開始 duration={duration} 呼び出し回数={sleepCallCount} Time={Time.time:F2}");
        if (currentFadeCoroutine != null)
        {
            Debug.LogWarning($"[ScreenFade] コルーチン多重実行を検出！前のコルーチンを停止します");
            StopCoroutine(currentFadeCoroutine);
        }
        currentFadeCoroutine = StartCoroutine(FadeOutInRoutine(duration, onBlack, onComplete));
    }

    IEnumerator FadeOutInRoutine(float duration, System.Action onBlack, System.Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            alpha = elapsed / duration;
            Debug.Log($"[ScreenFade] フェードアウト中 alpha={alpha:F3} elapsed={elapsed:F3}");
            yield return null;
            elapsed += Time.deltaTime;
        }
        alpha = 1f;
        Debug.Log("[ScreenFade] 暗転完了 alpha=1.000");

        onBlack?.Invoke();

        elapsed = 0f;
        while (elapsed < duration)
        {
            alpha = 1f - elapsed / duration;
            Debug.Log($"[ScreenFade] フェードイン中 alpha={alpha:F3} elapsed={elapsed:F3}");
            yield return null;
            elapsed += Time.deltaTime;
        }
        alpha = 0f;
        Debug.Log("[ScreenFade] フェードイン完了 alpha=0.000");

        currentFadeCoroutine = null;
        onComplete?.Invoke();
    }
}
