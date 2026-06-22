#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SpatialRoute.Editor
{
    [CustomEditor(typeof(NavigationGraph))]
    public sealed class NavigationGraphEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if(GUILayout.Button("Validate navigation graph"))
            {
                try{((NavigationGraph)target).ValidateGraph();Debug.Log("Navigation graph is valid.");}
                catch(System.ArgumentException error){Debug.LogError(error.Message);}
            }
        }
    }
}
#endif
