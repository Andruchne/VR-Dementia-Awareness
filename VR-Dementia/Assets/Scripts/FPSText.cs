using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FPSText : MonoBehaviour
{
    private TextMeshProUGUI text;
    private List<int> fpsBuffer = new List<int>();
    private const int MaxEntries = 30;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        int currentFPS = (int)(1.0f / Time.unscaledDeltaTime);

        if (fpsBuffer.Count >= MaxEntries)
        {
            fpsBuffer.RemoveAt(0);
        }
        fpsBuffer.Add(currentFPS);

        text.text = $"FPS: {GetModalFPS()}";
    }

    private int GetModalFPS()
    {
        if (fpsBuffer.Count == 0) return 0;

        return fpsBuffer
            .GroupBy(n => n)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
    }
}