using UnityEditor;
using UnityEngine;

namespace EBA.Ebunieditor.Editor.Scene
{
    class SceneEditor
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            bool is_handled = false;

            if (sceneView.in2DMode)
            {
                Handles.BeginGUI();
                if (GUI.Button(new Rect(5, 8, 60, 20), $"ShowUI"))
                    SetSceneViewZoom(sceneView, 50f);
                Handles.EndGUI();
                SceneView.RepaintAll ();
            }
        }
    
        static float GetSceneViewHeight(SceneView sceneView)
        {
            // Don't use sceneView.position.height, as it does not account for the space taken up by
            // toolbars.
            return sceneView.position.width / sceneView.camera.aspect;
        }

        static void SetSceneViewZoom(SceneView sceneView, float zoom)
        {
            float orthoHeight = GetSceneViewHeight(sceneView) / 2f / zoom;

            // We can't set camera.orthographicSize directly, because SceneView overrides it
            // every frame based on SceneView.size, so set SceneView.size instead.
            //
            // See SceneView.GetVerticalOrthoSize for the source of these sqrts.
            //sceneView.size = orthoHeight * Mathf.Sqrt(2f) * Mathf.Sqrt(sceneView.camera.aspect);
            float size = orthoHeight * Mathf.Sqrt(2f) * Mathf.Sqrt(sceneView.camera.aspect);
            sceneView.LookAt(Vector3.zero, sceneView.rotation, size);
        }
    }
}