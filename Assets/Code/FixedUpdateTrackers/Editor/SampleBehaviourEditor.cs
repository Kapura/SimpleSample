using System;
using UnityEngine;
using UnityEditor;
using SimpleSample.Editor;
using Object = UnityEngine.Object;

namespace SimpleSample.Behaviours.Editor
{
    [CanEditMultipleObjects]
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
                {
                    // TODO: is there a faster way to add to the selection in a non-destructive way?
                    Object[] oldSelection = Selection.objects;
                    Object[] newSelection = new Object[oldSelection.Length + 1];

                    Array.Copy(oldSelection, newSelection, oldSelection.Length);
                }

                GraphWindow.ShowWindow();
            }
        }
    }
}