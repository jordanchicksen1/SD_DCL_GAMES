using UnityEngine;

public class DustCloud : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float lifetime = 0.45f;

    [Header("Movement")]
    [SerializeField] private float floatSpeed = 2.2f;
    [SerializeField] private float growSpeed = 12f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeedMin = 220f;
    [SerializeField] private float rotationSpeedMax = 500f;

    [Header("Scale")]
    [SerializeField] private float minScale = 0.10f;
    [SerializeField] private float maxScale = 0.15f;

    [Header("Spawn Offset")]
    [SerializeField] private float spawnOffset = 0.05f;

    private float rotationSpeed;
    private float timer;

    private Vector3 driftDirection;
    private Vector3 targetScale;

    public void Spawn(Vector3 position)
    {
        // Small random offset so every cloud is slightly different.
        transform.position = position + new Vector3(
            Random.Range(-spawnOffset, spawnOffset),
            0f,
            Random.Range(-spawnOffset, spawnOffset)
        );

        timer = 0f;

        // Random size.
        float randomScale = Random.Range(minScale, maxScale);
        targetScale = Vector3.one * randomScale;

        // Start tiny.
        transform.localScale = Vector3.zero;

        // Random rotation.
        transform.rotation = Quaternion.Euler(
            0f,
            Random.Range(0f, 360f),
            0f
        );

        // Random spin speed and direction.
        rotationSpeed = Random.Range(
            rotationSpeedMin,
            rotationSpeedMax
        );

        if (Random.value > 0.5f)
            rotationSpeed *= -1f;

        // Slight upward/random drift.
        driftDirection = new Vector3(
            Random.Range(-0.5f, 0.5f),
            1f,
            Random.Range(-0.5f, 0.5f)
        ).normalized;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float lifePercent = timer / lifetime;

        // Pop in.
        if (lifePercent < 0.2f)
        {
            transform.localScale = Vector3.MoveTowards(
                transform.localScale,
                targetScale,
                growSpeed * 3f * Time.deltaTime
            );
        }
        // Hold.
        else if (lifePercent < 0.6f)
        {
            transform.localScale = targetScale;
        }
        // Shrink away.
        else
        {
            float shrink = Mathf.InverseLerp(
                0.6f,
                1f,
                lifePercent
            );

            transform.localScale = Vector3.Lerp(
                targetScale,
                Vector3.zero,
                shrink
            );
        }

        // Float upward.
        transform.position +=
            driftDirection * floatSpeed * Time.deltaTime;

        // Spin.
        transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.World
        );

        // Return to pool.
        if (timer >= lifetime)
        {
            gameObject.SetActive(false);
        }
    }
}