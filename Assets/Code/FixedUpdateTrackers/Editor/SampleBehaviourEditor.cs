using UnityEngine;
using UnityEditor;
using SimpleSample.Editor;

namespace SimpleSample.Behaviours.Editor
{
    [CustomEditor(typeof(SampleBehaviour), true)]
    public class SampleBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SampleBehaviour script = (SampleBehaviour)target;

            if (GUILayout.Button("Open in Graph Window"))
            {
                if (!Selection.Contains(script))
                    Selection.activeObject = script;

                GraphWindow.ShowWindow();
            }
        }
    }
}