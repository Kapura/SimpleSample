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

            int numStreams = script.NumSampleStreams;

            if (numStreams == 1)
            {
                if (GUILayout.Button("Copy stream CSV to clipboard"))
                {
                    GUIUtility.systemCopyBuffer = script.StreamToCSV(0);
                    Debug.LogFormat("Copied CSV of stream '{0}' to clipboard", script.GetSampleName(0));
                }
            }
            else if (numStreams > 1)
            {
                GUILayout.Label("Copy stream CSV to clipboard:");
                
                // TODO: something to keep this from breaking on large no. of samples
                for (int i = 0; i < numStreams; i++)
                {
                    string name = script.GetSampleName(i);
                    if (GUILayout.Button(name))
                    {
                        GUIUtility.systemCopyBuffer = script.StreamToCSV(i);
                        Debug.LogFormat("Copied CSV of stream '{0}' to clipboard", name);
                    }
                }
            }
        }
    }
}