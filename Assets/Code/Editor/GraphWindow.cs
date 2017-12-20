using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SimpleSample.Editor
{
    public class GraphWindow : EditorWindow
    {
        private const float GRAPH_MARGIN_LEFT = 80f;
        private const float GRAPH_MARGIN_TOP = 40f;
        private const float GRAPH_INNER_MARGIN = 25f;
        private const float GRAPH_GRID_WIDTH = 50f;
        
        private const float KEY_HORIZONTAL_MARGIN = 2f;
        private const float KEY_VERTICAL_MARGIN = 2f;
        private const float KEY_ENTRY_HEIGHT = 60f;
        private const float KEY_ENTRY_WIDTH = 100f;
        private const float KEY_AREA_ROW_HEIGHT = KEY_ENTRY_HEIGHT + (2 * KEY_VERTICAL_MARGIN);

        private const float WINDOW_MIN_X = GRAPH_MARGIN_LEFT + GRAPH_INNER_MARGIN + GRAPH_INNER_MARGIN + GRAPH_GRID_WIDTH;
        private const float WINDOW_MIN_Y = GRAPH_MARGIN_TOP + GRAPH_INNER_MARGIN + GRAPH_INNER_MARGIN + GRAPH_GRID_WIDTH + KEY_AREA_ROW_HEIGHT;

        private static readonly Color KEY_BACKGROUND_COLOR = new Color(0, 0, 0);
        private static readonly Color GRAPH_BACKGROUND_COLOR = new Color(0, 0, 0);
        private static readonly Color GRAPH_BORDER_COLOR = new Color(1, 1, 1);
        private static readonly Color GRAPH_GRID_COLOR = new Color (0.2f, 0.3f, 0.2f);
        private static readonly Color GRAPH_LABEL_COLOR = new Color (0, 0, 0);
        private static readonly Color GRAPH_HELP_COLOR = new Color(1, 1, 1);
        
        [SerializeField]
        private List<ISampleProvider> _sampleProviders = new List<ISampleProvider>();

        
        // These lists starting to get ridiculus.
        // TODO: make SampleInfo or w/e struct
        [SerializeField]
        private int _totalSamples = 0;
        [SerializeField]
        private List<Vector2[]> _allStreams = new List<Vector2[]>();
        [SerializeField]
        private List<Color> _allColors = new List<Color>();
        [SerializeField]
        private List<string> _allNames = new List<string>();

        [Serializable]
        private struct SampleRange
        {
            public float min;
            public float max;

            public SampleRange(float min, float max)
            {
                this.min = min;
                this.max = max;
            }

            public static SampleRange Range01 { get { return new SampleRange(0, 1); } }
        }

        [SerializeField]
        private List<SampleRange> _allRanges = new List<SampleRange>();
        
        [SerializeField]
        private List<bool> _showSample = new List<bool>();

        [SerializeField]
        private Dictionary<int, string> _cachedValueStrings = new Dictionary<int, string>();

        private Material lineMat = null;

        [SerializeField]
        private bool _lockSamples = false;
        
        [MenuItem("Window/Graph Window")]
        public static void ShowWindow()
        {
            GraphWindow window = GetWindow<GraphWindow>(false, "Graph Window", true);
            window.minSize = new Vector2(WINDOW_MIN_X, WINDOW_MIN_Y);
        }

        [UnityEditor.Callbacks.OnOpenAsset(0)]  // Double click on an ISampleProvider to open the graph window
        private static bool OnOpenAsset(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) as ISampleProvider != null)
            {
                ShowWindow();
                return true;
            }
            return false;
        }

        private string GetRoundedString(float value)
        {
            int rounded = Mathf.RoundToInt(1000 * value);
            if (!_cachedValueStrings.ContainsKey(rounded))
                _cachedValueStrings[rounded] = value.ToString("0.000");
            return _cachedValueStrings[rounded];
        }

        private void OnEnable()
        {
            float minX = GRAPH_MARGIN_LEFT + GRAPH_INNER_MARGIN + GRAPH_INNER_MARGIN + GRAPH_GRID_WIDTH;
            float minY = GRAPH_MARGIN_TOP + GRAPH_INNER_MARGIN + GRAPH_INNER_MARGIN + GRAPH_GRID_WIDTH + KEY_AREA_ROW_HEIGHT;
            minSize.Set(minX, minY);
        }

        private void Update()
        {
            Repaint();
        }

        private void OnGUI()
        {
            Profiler.BeginSample("GraphWindow");
            bool selectionHasSamples = false;  // We won't clear the list until we find at least one sample provider in the new selection
            
            Profiler.BeginSample("UpdateSampleProviders");
            int[] ids = Selection.instanceIDs;
            int numIds = ids.Length;
            for (int i = 0; i < numIds; i++)
            {
                Object obj = EditorUtility.InstanceIDToObject(ids[i]);
                ISampleProvider provider = obj as ISampleProvider;
                if (provider != null && provider.NumSampleStreams > 0)
                {
                    if (!selectionHasSamples)
                        _sampleProviders.Clear();

                    selectionHasSamples = true;
                    _sampleProviders.Add(provider);
                }

                GameObject go = obj as GameObject;
                if (go != null)
                {
                    MonoBehaviour[] behaviours = go.GetComponents<MonoBehaviour>();
                    int numBehaviours = behaviours.Length;
                    for (int j = 0; j < numBehaviours; j++)
                    {
                        ISampleProvider p = behaviours[j] as ISampleProvider;
                        if (p != null && p.NumSampleStreams > 0)
                        {
                            if (!selectionHasSamples)
                                _sampleProviders.Clear();

                            selectionHasSamples = true;
                            _sampleProviders.Add(behaviours[j] as ISampleProvider);
                        }
                    }
                }
            }

            // Update the aggregate listings
            if (!_lockSamples && (selectionHasSamples || _totalSamples > 0))
            {
                // All old state is assumed to be stale...
                _totalSamples = 0;
                _allStreams.Clear();
                _allColors.Clear();
                _allNames.Clear();
                
                IEnumerator<ISampleProvider> enumerator = _sampleProviders.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    int numStreams = enumerator.Current.NumSampleStreams;
                    for (int i = 0; i < numStreams; i++)
                    {
                        Vector2[] sampleStream = enumerator.Current.GetSampleStream(i);
                        _allStreams.Add(sampleStream);
                        _allColors.Add(enumerator.Current.GetSampleColor(i));
                        _allNames.Add(enumerator.Current.GetSampleName(i));

                        // _totalSamples is the index of the most recent stream; == _allStreams.Count - 1
                        if (_showSample.Count <= _totalSamples)
                            _showSample.Add(true);

                        if (_allRanges.Count <= _totalSamples)
                            _allRanges.Add(SampleRange.Range01);
                        
                        _totalSamples++;
                    }
                }
            }
            Profiler.EndSample();

            // Draw the lock checkbox
            Rect lockRect = new Rect(0, 0, position.width, GRAPH_MARGIN_TOP);
            _lockSamples = EditorGUI.ToggleLeft(lockRect, "Lock samples", _lockSamples);

            Profiler.BeginSample("Key");
            // Draw key
            float keyAreaHeight = KEY_AREA_ROW_HEIGHT;
            {
                // Determine the number of rows
                int entriesPerRow = Mathf.FloorToInt(position.width / (KEY_HORIZONTAL_MARGIN + KEY_ENTRY_WIDTH));
                int numRows = Mathf.CeilToInt((float)_totalSamples / (float)entriesPerRow);
                keyAreaHeight = numRows * KEY_AREA_ROW_HEIGHT;

                Rect keyRect = new Rect(0, position.height - keyAreaHeight, position.width, keyAreaHeight);
                EditorGUI.DrawRect(keyRect, KEY_BACKGROUND_COLOR);

                // Draw each entry
                Rect entryRect = new Rect(KEY_HORIZONTAL_MARGIN, keyRect.y + KEY_VERTICAL_MARGIN, KEY_ENTRY_WIDTH, KEY_ENTRY_HEIGHT);
                int i = 0;
                while (i < _totalSamples)
                {
                    Color backgroundColor = _allColors[i];
                    Color textColor = (((backgroundColor.r + backgroundColor.g + backgroundColor.b) * backgroundColor.a) > 1.35f) ? Color.black : Color.white;
                    string name = _allNames[i];
                    EditorGUI.DrawRect(entryRect, backgroundColor);

                    Rect labelRect = new Rect(entryRect);
                    labelRect.height /= 3f;
                    GUIStyle labelStyle = GetCenteredStyle(textColor);
                    EditorGUI.LabelField(labelRect, name, labelStyle);

                    Rect checkRect = new Rect(labelRect);
                    checkRect.y += checkRect.height;
                    EditorGUIUtility.labelWidth = KEY_ENTRY_WIDTH - 20f;  // Constrain the label to allow the checkbox to be drawn correctly
                    _showSample[i] = EditorGUI.Toggle(checkRect, "Display?", _showSample[i]);

                    Rect sliderRect = new Rect(checkRect);
                    sliderRect.y += checkRect.height;
                    sliderRect.x += KEY_HORIZONTAL_MARGIN;
                    sliderRect.width -= (2 * KEY_HORIZONTAL_MARGIN);
                    // TODO: this seems a dumb way to interact with their API
                    float min = _allRanges[i].min;
                    float max = _allRanges[i].max;
                    EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, 0f, 1f);
                    _allRanges[i] = new SampleRange(min, max);
                    
                    // Iteration bounds checks
                    i++;
                    entryRect.x += KEY_ENTRY_WIDTH + KEY_HORIZONTAL_MARGIN;

                    if (entryRect.x + KEY_ENTRY_WIDTH > position.width - KEY_HORIZONTAL_MARGIN)
                    {
                        entryRect.x = KEY_HORIZONTAL_MARGIN;
                        entryRect.y += KEY_AREA_ROW_HEIGHT;
                    }
                }
                
                EditorGUIUtility.labelWidth = KEY_ENTRY_WIDTH - 0f;
            }
            Profiler.EndSample();

            Profiler.BeginSample("GraphBackground");
            // Draw graph background
            Profiler.BeginSample("BackgroundColor");
            Rect graphRect = new Rect(GRAPH_MARGIN_LEFT, GRAPH_MARGIN_TOP, position.width - GRAPH_MARGIN_LEFT, position.height - GRAPH_MARGIN_TOP - keyAreaHeight);
            EditorGUI.DrawRect(graphRect, GRAPH_BACKGROUND_COLOR);
            Profiler.EndSample();

            // Grid lines
            Profiler.BeginSample("Grid Lines");
            bool haveSamples = false;
            Vector2 dataMin = new Vector2(Mathf.Infinity, Mathf.Infinity);
            Vector2 dataMax = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);
            {
                Profiler.BeginSample("Min/Max");
                for (int i = 0; i < _totalSamples; i++)
                {
                    if (!_showSample[i]) continue;

                    int numPoints = _allStreams[i].Length;
                    int min = Mathf.RoundToInt(_allRanges[i].min * numPoints);
                    int max = Mathf.RoundToInt(_allRanges[i].max * numPoints);
                    for (int j = min; j < max; j++)
                    {
                        if (_allStreams[i][j].x < dataMin.x) dataMin.x = _allStreams[i][j].x;
                        if (_allStreams[i][j].y < dataMin.y) dataMin.y = _allStreams[i][j].y;
                        if (_allStreams[i][j].x > dataMax.x) dataMax.x = _allStreams[i][j].x;
                        if (_allStreams[i][j].y > dataMax.y) dataMax.y = _allStreams[i][j].y;
                        haveSamples = true;
                    }
                }
                Profiler.EndSample();

                if (haveSamples)
                {
                    GUIStyle labelStyle = GetCenteredStyle(GRAPH_LABEL_COLOR);

                    Profiler.BeginSample("Vertical");
                    // Vertical lines
                    if (dataMin.x == dataMax.x)  // Single x value
                    {
                        Vector2 startPoint = new Vector2(graphRect.x + (graphRect.width * 0.5f), graphRect.y + graphRect.height);
                        Vector2 endPoint = new Vector2(startPoint.x, graphRect.y);
                        DrawLine(startPoint, endPoint, GRAPH_GRID_COLOR);
                        
                        Rect labelRect = new Rect(startPoint.x - (GRAPH_GRID_WIDTH * 0.5f), 0f, GRAPH_GRID_WIDTH, GRAPH_MARGIN_TOP);
                        EditorGUI.LabelField(labelRect, GetRoundedString(dataMin.x), labelStyle);                    }
                    else
                    {
                        float innerWidth = graphRect.width - (2 * GRAPH_INNER_MARGIN);
                        int numGridLines = Mathf.FloorToInt(innerWidth / GRAPH_GRID_WIDTH);
                        float gridWidth = innerWidth / (float)numGridLines;

                        Vector2 startPoint = new Vector2(graphRect.x + GRAPH_INNER_MARGIN, graphRect.y + graphRect.height);
                        Vector2 endPoint = new Vector2(startPoint.x, graphRect.y);
                        Rect labelRect = new Rect(startPoint.x - (GRAPH_GRID_WIDTH * 0.5f), 0f, GRAPH_GRID_WIDTH, GRAPH_MARGIN_TOP);
                        for (int i = 0; i < numGridLines + 1; i++)
                        {
                            float t = dataMin.x + (((float)i / (float)numGridLines) * (dataMax.x - dataMin.x));
                            DrawLine(startPoint, endPoint, GRAPH_GRID_COLOR);
                            EditorGUI.LabelField(labelRect, GetRoundedString(t), labelStyle);

                            startPoint.x += gridWidth;
                            endPoint.x += gridWidth;
                            labelRect.x += gridWidth;
                        }
                    }
                    Profiler.EndSample();

                    Profiler.BeginSample("Horizontal");
                    // Horizontal lines
                    if (dataMin.y == dataMax.y)  // Single y value
                    {
                        Vector2 startPoint = new Vector2(graphRect.x, GRAPH_MARGIN_TOP + (0.5f * graphRect.height));
                        Vector2 endPoint = new Vector2(position.width, startPoint.y);
                        DrawLine(startPoint, endPoint, GRAPH_GRID_COLOR);
                        
                        Rect labelRect = new Rect(0f, startPoint.y, GRAPH_MARGIN_LEFT, GRAPH_GRID_WIDTH);
                        EditorGUI.LabelField(labelRect, GetRoundedString(dataMin.y), labelStyle);  
                    }
                    else
                    {
                        float innerHeight = graphRect.height - (2 * GRAPH_INNER_MARGIN);
                        int numGridLines = Mathf.FloorToInt(innerHeight / GRAPH_GRID_WIDTH);
                        float gridHeight = innerHeight / (float)numGridLines;

                        Vector2 startPoint = new Vector2(graphRect.x, graphRect.y + GRAPH_INNER_MARGIN);
                        Vector2 endPoint = new Vector2(position.width, startPoint.y);
                        Rect labelRect = new Rect(0f, startPoint.y - (GRAPH_GRID_WIDTH * 0.5f), GRAPH_MARGIN_LEFT, GRAPH_GRID_WIDTH);
                        for (int i = 0; i < numGridLines + 1; i++)
                        {
                            float t = dataMin.y + ((1 - ((float)i / (float)numGridLines)) * (dataMax.y - dataMin.y));
                            DrawLine(startPoint, endPoint, GRAPH_GRID_COLOR);
                            EditorGUI.LabelField(labelRect, GetRoundedString(t), labelStyle);

                            startPoint.y += gridHeight;
                            endPoint.y += gridHeight;
                            labelRect.y += gridHeight;
                        }
                    }
                    Profiler.EndSample();
                }
            }

            Profiler.EndSample();

            // Borders
            DrawLine(new Vector2(0, position.height - keyAreaHeight), new Vector2(position.width, position.height - keyAreaHeight), GRAPH_BORDER_COLOR);
            Profiler.EndSample();

            Profiler.BeginSample("Data");
            // Draw graph data
            {
                if (!haveSamples)
                {
                    EditorGUI.LabelField(graphRect, "No data to display", GetCenteredStyle(GRAPH_HELP_COLOR));
                }
                else
                {
                    for (int i = 0; i < _totalSamples; i++)
                    {
                        if (!_showSample[i]) continue;

                        Color lineColor = _allColors[i];
                        Vector2[] stream = _allStreams[i];

                        if (stream == null || stream.Length == 0)
                            continue;
#if UNITY_5_6_OR_NEWER
                        if (lineMat == null)
                            lineMat = new Material(Shader.Find("Sprites/Default"));

                        lineMat.SetPass(0);
                        GL.Begin(GL.LINE_STRIP);
                        GL.Color(lineColor);

                        int numPoints = stream.Length;
                        int min = Mathf.RoundToInt(_allRanges[i].min * numPoints);
                        int max = Mathf.RoundToInt(_allRanges[i].max * numPoints);

                        for (int j = min; j < max; j++)
                        {
                            Vector2 point = new Vector2(
                            graphRect.x + GRAPH_INNER_MARGIN + (Mathf.InverseLerp(dataMin.x, dataMax.x, stream[j].x) * (graphRect.width - (2 * GRAPH_INNER_MARGIN))),
                            graphRect.y + GRAPH_INNER_MARGIN + (Mathf.InverseLerp(dataMax.y, dataMin.y, stream[j].y) * (graphRect.height - (2 * GRAPH_INNER_MARGIN))));

                            if (dataMin.x == dataMax.x)
                            {
                                point.x = graphRect.x + (graphRect.width * 0.5f);
                            }

                            if (dataMin.y == dataMax.y)
                            {
                                point.y = graphRect.y + (graphRect.height * 0.5f);
                            }

                            GL.Vertex(point);
                        }
                        GL.End();
#else
                        
                        int numPoints = stream.Length;
                        int min = Mathf.RoundToInt(_allRanges[i].min * numPoints);
                        int max = Mathf.RoundToInt(_allRanges[i].max * numPoints);

                        Vector2 startPoint = new Vector2(
                            graphRect.x + GRAPH_INNER_MARGIN + (Mathf.InverseLerp(dataMin.x, dataMax.x, stream[min].x) * (graphRect.width - (2 * GRAPH_INNER_MARGIN))),
                            graphRect.y + GRAPH_INNER_MARGIN + (Mathf.InverseLerp(dataMax.y, dataMin.y, stream[min].y) * (graphRect.height - (2 * GRAPH_INNER_MARGIN))));

                        for (int j = min + 1; j < max; j++)
                        {
                            Vector2 point = new Vector2(
                            graphRect.x + GRAPH_INNER_MARGIN + (Mathf.InverseLerp(dataMin.x, dataMax.x, stream[j].x) * (graphRect.width - (2 * GRAPH_INNER_MARGIN))),
                            graphRect.y + GRAPH_INNER_MARGIN + (Mathf.InverseLerp(dataMax.y, dataMin.y, stream[j].y) * (graphRect.height - (2 * GRAPH_INNER_MARGIN))));

                            if (dataMin.x == dataMax.x)
                            {
                                startPoint.x = graphRect.x + (graphRect.width * 0.5f);
                                point.x = startPoint.x;
                            }

                            if (dataMin.y == dataMax.y)
                            {
                                startPoint.y = graphRect.y + (graphRect.height * 0.5f);
                                point.y = startPoint.y;
                            }

                            DrawLine(startPoint, point, lineColor);
                            startPoint = point;
                        }
#endif
                    }
                }
            }
            Profiler.EndSample();

            Profiler.EndSample();
        }

        private GUIStyle GetCenteredStyle(Color color)
        {
            return GetCenteredStyle(color, color);
        }

        private static GUIStyle GetCenteredStyle(Color normalColor, Color hoverColor)
        {
            GUIStyle newStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState() { textColor = normalColor },
                hover = new GUIStyleState() { textColor = hoverColor }
            };
            return newStyle;
        }

        private void DrawLine(Vector2 startPoint, Vector2 endPoint, Color color)
        {
            if (lineMat == null)
                 lineMat = new Material(Shader.Find("Sprites/Default"));

            lineMat.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex(startPoint);
            GL.Vertex(endPoint);
            GL.End();
        }
    }
}