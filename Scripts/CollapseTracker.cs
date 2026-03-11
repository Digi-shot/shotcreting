using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Obi;
using TMPro;

public class CollapseTracker : MonoBehaviour
{
    public ObiSolver solver;
    public float realSamplingInterval = 0.5f;
    public ObiEmitter emitter;
    public TMP_FontAsset fontAsset;
    public Material sphereMaterial;
    public string customFolderPath = "C:/MyCSVFiles";

    public float textSize = 24f;
    public float ballSize = 0.01f;
    public float fangdaxishu = 300f;

    public float maxXOffset = 0f;
    public float maxZOffset = 0f;

    public Vector2 chartSize = new Vector2(460f, 220f);
    public int maxDataPoints = 120;
    public float chartYMin = 0f;
    public float chartYMax = 200f;

    private float timer;
    private float maxY;
    private float maxX;
    private float maxZ;

    private Vector3 maxYPosition;
    private Vector3 maxXPosition;
    private Vector3 maxZPosition;

    private string csvPath;

    private GameObject maxYTextObject;
    private GameObject maxXTextObject;
    private GameObject maxZTextObject;

    private GameObject sphereMaxY;
    private GameObject sphereMaxX;
    private GameObject sphereMaxZ;

    private Canvas canvas;

    private List<float> dynamicSlumpHistory = new List<float>();
    private List<float> dynamicSlumpRadiusHistory = new List<float>();

    private RawImage chartDynamicSlumpImage;
    private RawImage chartDynamicSlumpRadiusImage;

    private Texture2D chartDynamicSlumpTexture;
    private Texture2D chartDynamicSlumpRadiusTexture;

    private TMP_Text chartDynamicSlumpTitle;
    private TMP_Text chartDynamicSlumpRadiusTitle;

    private List<TMP_Text> chart1YLabels = new List<TMP_Text>();
    private List<TMP_Text> chart1XLabels = new List<TMP_Text>();
    private List<TMP_Text> chart2YLabels = new List<TMP_Text>();
    private List<TMP_Text> chart2XLabels = new List<TMP_Text>();

    void Start()
    {
        if (!Directory.Exists(customFolderPath))
        {
            Directory.CreateDirectory(customFolderPath);
        }

        DateTime currentTime = DateTime.Now;
        string currentTimeString = currentTime.ToString("yyyyMMddHHmmss");
        csvPath = Path.Combine(customFolderPath, "myCollapseData_" + currentTimeString + ".csv");

        CreateCanvas();
        CreateTextObjects();
        CreateSpheres();
        CreateCharts();

        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= realSamplingInterval)
        {
            timer -= realSamplingInterval;

            int particleCount = Mathf.Min(emitter.particleCount, solver.positions.count);

            if (particleCount >= 5)
            {
                maxY = float.MinValue;
                maxX = float.MinValue;
                maxZ = float.MinValue;
                maxYPosition = Vector3.zero;
                maxXPosition = Vector3.zero;
                maxZPosition = Vector3.zero;

                for (int i = 0; i < particleCount; i++)
                {
                    Vector3 position = solver.transform.TransformPoint(solver.positions[i]);

                    if (position.y > maxY)
                    {
                        maxY = position.y;
                        maxYPosition = position;
                    }

                    if (position.x > maxX)
                    {
                        maxX = position.x;
                        maxXPosition = position;
                    }

                    if (position.z > maxZ)
                    {
                        maxZ = position.z;
                        maxZPosition = position;
                    }
                }

                float maxYMm = maxY * fangdaxishu;
                float maxXMm = maxX * fangdaxishu + maxXOffset;
                float maxZMm = maxZ * fangdaxishu + maxZOffset;

                using (StreamWriter writer = File.AppendText(csvPath))
                {
                    string log = Time.realtimeSinceStartup + "," + (300 - maxYMm) + "," + maxXMm + "," + maxZMm;
                    writer.WriteLine(log);
                }

                float dynamicSlump = maxYMm;
                float dynamicSlumpRadius = (maxXMm + maxZMm) * 0.5f;

                PushData(dynamicSlumpHistory, dynamicSlump);
                PushData(dynamicSlumpRadiusHistory, dynamicSlumpRadius);

                DrawChart(chartDynamicSlumpTexture, dynamicSlumpHistory, new Color(1f, 0.25f, 0.25f), chartYMin, chartYMax);
                DrawChart(chartDynamicSlumpRadiusTexture, dynamicSlumpRadiusHistory, new Color(0.2f, 0.85f, 1f), chartYMin, chartYMax);

                chartDynamicSlumpTitle.text = "Max Z (mm): " + dynamicSlump.ToString("F2");
                chartDynamicSlumpRadiusTitle.text = "(Max X + Max Z) / 2 (mm): " + dynamicSlumpRadius.ToString("F2");

                UpdateAxisLabels(chart1YLabels, chart1XLabels);
                UpdateAxisLabels(chart2YLabels, chart2XLabels);

                Debug.Log("Max Y: " + maxY + ", Max X: " + maxX + ", Max Z: " + maxZ);
            }
        }

        UpdateTextPositions();
        UpdateSpherePositions();
    }

    void OnApplicationQuit()
    {
        Debug.Log("Saved collapse data to " + csvPath);
    }

    void PushData(List<float> dataList, float value)
    {
        dataList.Add(value);
        if (dataList.Count > maxDataPoints)
        {
            dataList.RemoveAt(0);
        }
    }

    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
    }

    void CreateTextObjects()
    {
        maxYTextObject = CreateTextObject("Max Y: ", Vector3.zero);
        maxXTextObject = CreateTextObject("Max X: ", Vector3.zero);
        maxZTextObject = CreateTextObject("Max Z: ", Vector3.zero);
    }

    GameObject CreateTextObject(string labelText, Vector3 position)
    {
        GameObject textObject = new GameObject(labelText + "Text");
        textObject.transform.SetParent(canvas.transform, false);

        TMP_Text textMeshPro = textObject.AddComponent<TextMeshProUGUI>();
        textMeshPro.fontSize = textSize;
        textMeshPro.font = fontAsset;
        textMeshPro.alignment = TextAlignmentOptions.Left;
        textMeshPro.enableAutoSizing = false;
        textMeshPro.text = labelText;

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(220, 50);
        textObject.transform.position = position;
        textObject.SetActive(false);

        return textObject;
    }

    void CreateSpheres()
    {
        sphereMaxY = CreateSphere(Vector3.zero, ballSize);
        sphereMaxX = CreateSphere(Vector3.zero, ballSize);
        sphereMaxZ = CreateSphere(Vector3.zero, ballSize);
    }

    GameObject CreateSphere(Vector3 position, float scale)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.GetComponent<Renderer>().material = sphereMaterial;
        sphere.SetActive(false);
        return sphere;
    }

    void UpdateTextPositions()
    {
        UpdateTextPosition(maxYTextObject, maxYPosition, maxY * fangdaxishu, "Y");
        UpdateTextPosition(maxXTextObject, maxXPosition, maxX * fangdaxishu + maxXOffset, "X");
        UpdateTextPosition(maxZTextObject, maxZPosition, maxZ * fangdaxishu + maxZOffset, "Z");
    }

    void UpdateTextPosition(GameObject textObject, Vector3 position, float value, string axis)
    {
        if (Camera.main == null) return;

        Vector3 offset = new Vector3(0.02f, 0f, 0f);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(position + offset);

        if (screenPos.z > 0f)
        {
            textObject.transform.position = screenPos;
            textObject.GetComponentInChildren<TMP_Text>().text = "Max " + axis + ": " + value.ToString("F2") + " mm";
            textObject.SetActive(true);
        }
        else
        {
            textObject.SetActive(false);
        }
    }

    void UpdateSpherePositions()
    {
        UpdateSpherePosition(sphereMaxY, maxYPosition, Color.red, ballSize);
        UpdateSpherePosition(sphereMaxX, maxXPosition, Color.green, ballSize);
        UpdateSpherePosition(sphereMaxZ, maxZPosition, Color.blue, ballSize);
    }

    void UpdateSpherePosition(GameObject sphere, Vector3 position, Color color, float scale)
    {
        sphere.transform.position = position;
        sphere.GetComponent<Renderer>().material.color = color;
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.SetActive(true);
    }

    void CreateCharts()
    {
        CreateChartPanel(
            "Chart_DynamicSlump",
            new Vector2(20f, -20f),
            "Dynamic Slump (mm)",
            out chartDynamicSlumpImage,
            out chartDynamicSlumpTitle,
            out chart1YLabels,
            out chart1XLabels
        );

        CreateChartPanel(
            "Chart_DynamicSlumpRadius",
            new Vector2(20f, -(chartSize.y + 120f)),
            "Dynamic Slump Radius (mm)",
            out chartDynamicSlumpRadiusImage,
            out chartDynamicSlumpRadiusTitle,
            out chart2YLabels,
            out chart2XLabels
        );

        chartDynamicSlumpTexture = CreateChartTexture((int)chartSize.x, (int)chartSize.y);
        chartDynamicSlumpRadiusTexture = CreateChartTexture((int)chartSize.x, (int)chartSize.y);

        chartDynamicSlumpImage.texture = chartDynamicSlumpTexture;
        chartDynamicSlumpRadiusImage.texture = chartDynamicSlumpRadiusTexture;

        DrawChart(chartDynamicSlumpTexture, dynamicSlumpHistory, new Color(1f, 0.25f, 0.25f), chartYMin, chartYMax);
        DrawChart(chartDynamicSlumpRadiusTexture, dynamicSlumpRadiusHistory, new Color(0.2f, 0.85f, 1f), chartYMin, chartYMax);

        UpdateAxisLabels(chart1YLabels, chart1XLabels);
        UpdateAxisLabels(chart2YLabels, chart2XLabels);
    }

    void CreateChartPanel(
        string panelName,
        Vector2 anchoredPosition,
        string title,
        out RawImage chartImage,
        out TMP_Text titleText,
        out List<TMP_Text> yLabels,
        out List<TMP_Text> xLabels)
    {
        yLabels = new List<TMP_Text>();
        xLabels = new List<TMP_Text>();

        GameObject panel = new GameObject(panelName);
        panel.transform.SetParent(canvas.transform, false);

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.35f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = anchoredPosition;
        panelRect.sizeDelta = new Vector2(chartSize.x + 105f, chartSize.y + 90f);

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);

        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.font = fontAsset;
        titleText.fontSize = 24f;
        titleText.alignment = TextAlignmentOptions.Left;
        titleText.text = title;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(0f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(62f, -8f);
        titleRect.sizeDelta = new Vector2(chartSize.x, 30f);

        GameObject chartObj = new GameObject("ChartImage");
        chartObj.transform.SetParent(panel.transform, false);

        chartImage = chartObj.AddComponent<RawImage>();

        RectTransform chartRect = chartObj.GetComponent<RectTransform>();
        chartRect.anchorMin = new Vector2(0f, 1f);
        chartRect.anchorMax = new Vector2(0f, 1f);
        chartRect.pivot = new Vector2(0f, 1f);
        chartRect.anchoredPosition = new Vector2(62f, -40f);
        chartRect.sizeDelta = chartSize;

        CreateAxisLabels(panel.transform, chartRect, ref yLabels, ref xLabels);
    }

    void CreateAxisLabels(Transform parent, RectTransform chartRect, ref List<TMP_Text> yLabels, ref List<TMP_Text> xLabels)
    {
        int yTickCount = 5;
        int xTickCount = 6;

        for (int i = 0; i < yTickCount; i++)
        {
            GameObject yObj = new GameObject("YLabel_" + i);
            yObj.transform.SetParent(parent, false);

            TMP_Text yText = yObj.AddComponent<TextMeshProUGUI>();
            yText.font = fontAsset;
            yText.fontSize = 18f;
            yText.alignment = TextAlignmentOptions.Right;
            yText.text = "0";

            RectTransform yRect = yObj.GetComponent<RectTransform>();
            yRect.anchorMin = new Vector2(0f, 1f);
            yRect.anchorMax = new Vector2(0f, 1f);
            yRect.pivot = new Vector2(1f, 0.5f);

            float yPos = -40f - chartSize.y + (chartSize.y * i / (float)(yTickCount - 1));
            yRect.anchoredPosition = new Vector2(54f, yPos);
            yRect.sizeDelta = new Vector2(42f, 24f);

            yLabels.Add(yText);
        }

        for (int i = 0; i < xTickCount; i++)
        {
            GameObject xObj = new GameObject("XLabel_" + i);
            xObj.transform.SetParent(parent, false);

            TMP_Text xText = xObj.AddComponent<TextMeshProUGUI>();
            xText.font = fontAsset;
            xText.fontSize = 18f;
            xText.alignment = TextAlignmentOptions.Center;
            xText.text = "0";

            RectTransform xRect = xObj.GetComponent<RectTransform>();
            xRect.anchorMin = new Vector2(0f, 1f);
            xRect.anchorMax = new Vector2(0f, 1f);
            xRect.pivot = new Vector2(0.5f, 1f);

            float xPos = 62f + (chartSize.x * i / (float)(xTickCount - 1));
            xRect.anchoredPosition = new Vector2(xPos, -40f - chartSize.y - 4f);
            xRect.sizeDelta = new Vector2(60f, 24f);

            xLabels.Add(xText);
        }

        GameObject yAxisName = new GameObject("YAxisName");
        yAxisName.transform.SetParent(parent, false);

        TMP_Text yAxisText = yAxisName.AddComponent<TextMeshProUGUI>();
        yAxisText.font = fontAsset;
        yAxisText.fontSize = 18f;
        yAxisText.alignment = TextAlignmentOptions.Center;
        yAxisText.text = "mm";
        yAxisText.enableWordWrapping = false;

        RectTransform yAxisRect = yAxisName.GetComponent<RectTransform>();
        yAxisRect.anchorMin = new Vector2(0f, 1f);
        yAxisRect.anchorMax = new Vector2(0f, 1f);
        yAxisRect.pivot = new Vector2(0.5f, 0.5f);
        yAxisRect.anchoredPosition = new Vector2(18f, -40f - chartSize.y * 0.5f);
        yAxisRect.sizeDelta = new Vector2(60f, 24f);
        yAxisRect.localEulerAngles = new Vector3(0f, 0f, 90f);

        GameObject xAxisName = new GameObject("XAxisName");
        xAxisName.transform.SetParent(parent, false);

        TMP_Text xAxisText = xAxisName.AddComponent<TextMeshProUGUI>();
        xAxisText.font = fontAsset;
        xAxisText.fontSize = 20f;
        xAxisText.alignment = TextAlignmentOptions.Center;
        xAxisText.text = "s";

        RectTransform xAxisRect = xAxisName.GetComponent<RectTransform>();
        xAxisRect.anchorMin = new Vector2(0f, 1f);
        xAxisRect.anchorMax = new Vector2(0f, 1f);
        xAxisRect.pivot = new Vector2(0.5f, 1f);
        xAxisRect.anchoredPosition = new Vector2(62f + chartSize.x * 0.5f, -40f - chartSize.y - 28f);
        xAxisRect.sizeDelta = new Vector2(40f, 24f);
    }

    void UpdateAxisLabels(List<TMP_Text> yLabels, List<TMP_Text> xLabels)
    {
        int yTickCount = yLabels.Count;
        int xTickCount = xLabels.Count;

        for (int i = 0; i < yTickCount; i++)
        {
            float value = chartYMin + (chartYMax - chartYMin) * i / (float)(yTickCount - 1);
            yLabels[i].text = value.ToString("F0");
        }

        float visibleDuration = (maxDataPoints - 1) * realSamplingInterval;
        for (int i = 0; i < xTickCount; i++)
        {
            float value = visibleDuration * i / (float)(xTickCount - 1);
            xLabels[i].text = value.ToString("F1");
        }
    }

    Texture2D CreateChartTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    void DrawChart(Texture2D tex, List<float> data, Color lineColor, float minY, float maxYValue)
    {
        int width = tex.width;
        int height = tex.height;

        Color bgColor = new Color(0.08f, 0.08f, 0.08f, 1f);
        Color gridColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        Color axisColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = bgColor;
        }
        tex.SetPixels(pixels);

        int leftPad = 1;
        int rightPad = 1;
        int topPad = 1;
        int bottomPad = 1;

        int plotXMin = leftPad;
        int plotXMax = width - rightPad - 1;
        int plotYMin = bottomPad;
        int plotYMax = height - topPad - 1;

        for (int i = 0; i <= 4; i++)
        {
            int y = Mathf.RoundToInt(Mathf.Lerp(plotYMin, plotYMax, i / 4f));
            DrawLine(tex, plotXMin, y, plotXMax, y, gridColor);
        }

        for (int i = 0; i <= 5; i++)
        {
            int x = Mathf.RoundToInt(Mathf.Lerp(plotXMin, plotXMax, i / 5f));
            DrawLine(tex, x, plotYMin, x, plotYMax, gridColor);
        }

        DrawLine(tex, plotXMin, plotYMin, plotXMin, plotYMax, axisColor);
        DrawLine(tex, plotXMin, plotYMin, plotXMax, plotYMin, axisColor);

        if (data.Count >= 2)
        {
            for (int i = 1; i < data.Count; i++)
            {
                float x0 = Mathf.Lerp(plotXMin, plotXMax, (i - 1) / (float)(maxDataPoints - 1));
                float x1 = Mathf.Lerp(plotXMin, plotXMax, i / (float)(maxDataPoints - 1));

                float v0 = Mathf.Clamp(data[i - 1], minY, maxYValue);
                float v1 = Mathf.Clamp(data[i], minY, maxYValue);

                float y0 = Mathf.Lerp(plotYMin, plotYMax, (v0 - minY) / (maxYValue - minY));
                float y1 = Mathf.Lerp(plotYMin, plotYMax, (v1 - minY) / (maxYValue - minY));

                DrawLine(tex, Mathf.RoundToInt(x0), Mathf.RoundToInt(y0), Mathf.RoundToInt(x1), Mathf.RoundToInt(y1), lineColor);
            }
        }

        tex.Apply();
    }

    void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            SetPixelSafe(tex, x0, y0, color);
            SetPixelSafe(tex, x0 + 1, y0, color);
            SetPixelSafe(tex, x0, y0 + 1, color);
            SetPixelSafe(tex, x0 + 1, y0 + 1, color);

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = err * 2;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    void SetPixelSafe(Texture2D tex, int x, int y, Color color)
    {
        if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
        {
            tex.SetPixel(x, y, color);
        }
    }
}