using UnityEngine;

public class HandFanLayout : MonoBehaviour
{
    public float cardSpacing = 115f;

    public float maxRotation = 12f;

    public float arcHeight = 35f;

    public void Refresh()
    {
        int count = transform.childCount;

        if (count == 0)
            return;

        float center = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            RectTransform rt = transform.GetChild(i) as RectTransform;

            if (rt == null)
                continue;

            float offset = i - center;

            float normalized = count == 1
                ? 0f
                : offset / center;

            float x = offset * cardSpacing;

            float y = -Mathf.Abs(normalized) * arcHeight;

            float rot = -normalized * maxRotation;

            rt.anchoredPosition = new Vector2(x, y);

            rt.localRotation = Quaternion.Euler(0f, 0f, rot);

            rt.localScale = Vector3.one;
        }
    }
}