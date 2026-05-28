using UnityEngine;

public class AfterImage : MonoBehaviour
{
    private SpriteRenderer sr;

    private float fadeSpeed;
    private float currentAlpha;

    private Color color;

    private AfterImageTracer owner;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Initialize(
        Sprite sprite,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale,
        Color tint,
        float startAlpha,
        float activeTime,
        float fadeSpeed,
        AfterImageTracer owner
    )
    {
        this.owner = owner;
        this.fadeSpeed = fadeSpeed;

        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;

        sr.sprite = sprite;

        color = tint;
        currentAlpha = startAlpha;

        color.a = currentAlpha;

        sr.color = color;

        gameObject.SetActive(true);

        CancelInvoke();
        Invoke(nameof(DisableSelf), activeTime);
    }

    private void Update()
    {
        currentAlpha -= fadeSpeed * Time.deltaTime;

        color.a = currentAlpha;

        sr.color = color;
    }

    private void DisableSelf()
    {
        owner.ReturnToPool(this);
    }
}