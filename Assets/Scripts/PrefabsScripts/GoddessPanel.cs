using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class GoddessPanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private RectTransform rect;

    [Header("Animation Settings")]
    public float slideDistance = 500f;
    public float duration = 0.5f;

    [Header("UI References")]
    public Button closeButton;
    public Image elenaraHighlight;
    public Image velithraHighlight;
    public Image kaelythHighlight;
    public Color activeColor = Color.yellow;
    public Color inactiveColor = Color.white;

    private Dictionary<GoddessType, Image> highlights;

    public static GoddessPanel ActivePanel { get; private set; }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();

        highlights = new Dictionary<GoddessType, Image>
        {
            { GoddessType.Elenara, elenaraHighlight },
            { GoddessType.Velithra, velithraHighlight },
            { GoddessType.Kaelyth, kaelythHighlight }
        };

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
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

        RefreshHighlight();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        if (ActivePanel == this)
            ActivePanel = null;
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

        RefreshHighlight();

        StartCoroutine(SlideFade(new Vector2(0, -slideDistance), 0f, destroyAfter: true));
    }

    private void RefreshHighlight()
    {
        if (GameManager.Instance.currentDoobie == null) return;

        GoddessType current = GameManager.Instance.currentDoobie.CurrentGoddess;

        foreach (var kvp in highlights)
        {
            if (kvp.Value == null) continue;
            kvp.Value.color = (kvp.Key == current) ? activeColor : inactiveColor;
        }
    }

    public void ClosePanel()
    {
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
