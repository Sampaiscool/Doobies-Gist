using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class GoddessPanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private RectTransform rect;

    [Header("Animation Settings")]
    public float slideDistance = 500f;
    public float duration = 0.5f;

    public static GoddessPanel ActivePanel { get; private set; }

    private void OnDestroy()
    {
        if (ActivePanel == this)
            ActivePanel = null;
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // Destroy any previous active panel
        if (ActivePanel != null && ActivePanel != this)
        {
            Destroy(ActivePanel.gameObject);
        }
        ActivePanel = this;

        rect.anchoredPosition = new Vector2(0, -slideDistance);
        canvasGroup.alpha = 0f;

        StartCoroutine(SlideFade(Vector2.zero, 1f));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetGoddess(string goddessName)
    {
        if (GameManager.Instance.currentDoobie == null) return;

        GoddessType goddessType = GoddessType.None;
        switch (goddessName)
        {
            case "Elenara": goddessType = GoddessType.Elenara; break;
            case "Velithra": goddessType = GoddessType.Velithra; break;
            case "Kaelyth": goddessType = GoddessType.Kaelyth; break;
        }

        GameManager.Instance.currentDoobie.SetGoddess(goddessType);

        // Slide out and destroy
        StartCoroutine(SlideFade(new Vector2(0, -slideDistance), 0f, destroyAfter: true));
    }


    private IEnumerator SlideFade(Vector2 targetPos, float targetAlpha, bool destroyAfter = false)
    {
        Vector2 startPos = rect.anchoredPosition;
        float startAlpha = canvasGroup.alpha;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        rect.anchoredPosition = targetPos;
        canvasGroup.alpha = targetAlpha;

        if (destroyAfter)
            Destroy(gameObject);
    }
}
