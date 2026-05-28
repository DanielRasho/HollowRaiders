using System.Collections.Generic;
using UnityEngine;

public class AfterImageTracer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AfterImage afterImagePrefab;

    [Header("Pool")]
    [SerializeField] private int poolSize = 10;

    [Header("Spawn")]
    [SerializeField] private float spawnRate = 0.02f;

    [Header("Visuals")]
    [SerializeField] private Color tint = Color.white;
    [SerializeField] private float startAlpha = 0.8f;

    [Header("Fade")]
    [SerializeField] private float activeTime = 0.15f;
    [SerializeField] private float fadeSpeed = 10f;

    private SpriteRenderer targetRenderer;
    private readonly Queue<AfterImage> pool = new();
    private float spawnTimer;

    private void Start()
    {
        targetRenderer = GetComponent<SpriteRenderer>();
        GeneratePool();
    }

    private void GeneratePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            AfterImage obj = Instantiate(afterImagePrefab);

            obj.gameObject.SetActive(false);

            pool.Enqueue(obj);
        }
    }

    public void Emit()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnRate) return;

        spawnTimer = 0f;

        AfterImage obj = GetFromPool();

        obj.Initialize(
            targetRenderer.sprite,
            transform.position,
            transform.rotation,
            transform.localScale,
            tint,
            startAlpha,
            activeTime,
            fadeSpeed,
            this
        );
    }

    private AfterImage GetFromPool()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        return Instantiate(afterImagePrefab);
    }

    public void ReturnToPool(AfterImage obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}