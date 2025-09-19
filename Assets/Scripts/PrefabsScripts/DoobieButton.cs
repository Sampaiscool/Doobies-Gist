using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class DoobieButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public DoobieSO doobieData;
    public Button button;
    public TMP_Text nameText;
    public Image image;

    [Header("Hover UI")]
    public GameObject hoverInfoPanel;
    public TMP_Text hoverDescriptionText;
    public TMP_Text hoverStatsText;

    [Header("Animation")]
    public float animDuration = 0.25f; // one duration for both slide + fade
    public Vector2 hiddenOffset = new Vector2(-150f, 0f);

    private RectTransform hoverRect;
    private Vector2 originalPosition;
    private Coroutine animRoutine;
    private CanvasGroup hoverCanvasGroup;

    private bool isTeamSlot;
    private int teamSlotIndex;

    public void SetupButton(DoobieSO doobie, bool isTeamSlot = false, int teamSlotIndex = -1)
    {
        doobieData = doobie;
        this.isTeamSlot = isTeamSlot;
        this.teamSlotIndex = teamSlotIndex;

        nameText.text = doobie.doobieName;
        image.sprite = doobie.portrait;

        button.onClick.RemoveAllListeners();
        if (isTeamSlot)
        {
            button.onClick.AddListener(() =>
            {
                TeamSelectUI.Instance.OpenDoobieSelection(teamSlotIndex);
            });
        }
        else
        {
            button.onClick.AddListener(() =>
            {
                TeamSelectUI.Instance.OnDoobieSelected(doobieData, TeamSelectUI.Instance.selectedSlot);
            });
        }

        if (hoverInfoPanel != null)
        {
            hoverRect = hoverInfoPanel.GetComponent<RectTransform>();
            originalPosition = hoverRect.anchoredPosition;

            hoverCanvasGroup = hoverInfoPanel.GetComponent<CanvasGroup>();
            if (hoverCanvasGroup == null)
                hoverCanvasGroup = hoverInfoPanel.AddComponent<CanvasGroup>();

            hoverCanvasGroup.alpha = 0f;
            hoverInfoPanel.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverInfoPanel != null && doobieData != null)
        {
            hoverDescriptionText.text = doobieData.description;

            hoverStatsText.text =
                $"HP: {doobieData.baseHealth}\n" +
                $"Resource: {doobieData.doobieMainResource} ({doobieData.baseResourceMax})\n" +
                $"Skill Damage: {doobieData.skillDmg}\n" +
                $"Heal Power: {doobieData.healPower}\n" +
                $"Defense: {doobieData.baseDefence}";

            hoverInfoPanel.SetActive(true);

            if (animRoutine != null) StopCoroutine(animRoutine);
            animRoutine = StartCoroutine(AnimatePanel(
                originalPosition + hiddenOffset, 0f,
                originalPosition, 1f,
                disableOnEnd: false
            ));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverInfoPanel != null)
        {
            if (animRoutine != null) StopCoroutine(animRoutine);
            animRoutine = StartCoroutine(AnimatePanel(
                hoverRect.anchoredPosition, hoverCanvasGroup.alpha,
                originalPosition + hiddenOffset, 0f,
                disableOnEnd: true
            ));
        }
    }

    private IEnumerator AnimatePanel(Vector2 fromPos, float fromAlpha, Vector2 toPos, float toAlpha, bool disableOnEnd)
    {
        float elapsed = 0f;
        hoverRect.anchoredPosition = fromPos;
        hoverCanvasGroup.alpha = fromAlpha;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);

            hoverRect.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
            hoverCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);

            yield return null;
        }

        hoverRect.anchoredPosition = toPos;
        hoverCanvasGroup.alpha = toAlpha;

        if (disableOnEnd && toAlpha == 0f)
            hoverInfoPanel.SetActive(false);
    }
}
