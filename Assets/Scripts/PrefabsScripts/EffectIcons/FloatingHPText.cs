using UnityEngine;
using TMPro;

public class FloatingHPText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public float moveSpeed = 30f;
    public float duration = 1f;
    public Vector3 moveDirection = Vector3.up;

    private float timer;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(string message, Color color, Vector3 direction)
    {
        textMesh.text = message;
        textMesh.color = color;
        moveDirection = direction.normalized;
        timer = 0f;
        canvasGroup.alpha = 1f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Move
        rectTransform.anchoredPosition += (Vector2)(moveDirection * moveSpeed * Time.deltaTime);

        // Fade out
        canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}
