using System;
using System.Reflection;
using EBA.Ebunieditor.Editor.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EBUniEditor.Editor.Inspector
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(RectTransform), true)]
	internal class RectTransformEditor : Editor
	{
		private const string RECORD_NAME = nameof( RectTransformEditor );

		private Editor     editorInstance;
		private Type       nativeEditor;
		private MethodInfo onSceneGui;
		private MethodInfo onValidate;

		public override void OnInspectorGUI()
		{
			editorInstance.OnInspectorGUI();

			var rectTransform = target as RectTransform;

			if ( rectTransform == null ) return;

			/*using ( new EditorGUILayout.HorizontalScope( EditorStyles.helpBox ) )
			{
				GUI.enabled = rectTransform.localRotation != Quaternion.identity;
				if ( GUILayout.Button( "Reset Rotation" ) )
				{
					Undo.RecordObject( rectTransform, RECORD_NAME );
					rectTransform.localRotation = Quaternion.identity;
				}

				GUI.enabled = rectTransform.localScale != Vector3.one;
				if ( GUILayout.Button( "Reset Scale" ) )
				{
					Undo.RecordObject( rectTransform, RECORD_NAME );
					rectTransform.localScale = Vector3.one;
				}

				GUI.enabled =
					rectTransform.anchorMin != Vector2.zero ||
					rectTransform.anchorMax != Vector2.one ||
					rectTransform.offsetMin != Vector2.zero ||
					rectTransform.offsetMax != Vector2.zero ||
					rectTransform.pivot != new Vector2( 0.5f, 0.5f ) ||
					rectTransform.rotation != Quaternion.identity ||
					rectTransform.localScale != Vector3.one
					;

				if ( GUILayout.Button( "Fill" ) )
				{
					Undo.RecordObject( rectTransform, RECORD_NAME );

					rectTransform.anchorMin  = Vector2.zero;
					rectTransform.anchorMax  = Vector2.one;
					rectTransform.offsetMin  = Vector2.zero;
					rectTransform.offsetMax  = Vector2.zero;
					rectTransform.pivot      = new Vector2( 0.5f, 0.5f );
					rectTransform.rotation   = Quaternion.identity;
					rectTransform.localScale = Vector3.one;
				}

				GUI.enabled =
					rectTransform.localPosition.HasAfterDecimalPoint() ||
					rectTransform.sizeDelta.HasAfterDecimalPoint() ||
					rectTransform.offsetMin.HasAfterDecimalPoint() ||
					rectTransform.offsetMax.HasAfterDecimalPoint() ||
					rectTransform.localScale.HasAfterDecimalPoint()
					;

				if ( GUILayout.Button( "Round" ) )
				{
					Undo.RecordObject( rectTransform, RECORD_NAME );

					rectTransform.localPosition = rectTransform.localPosition.Round();
					rectTransform.sizeDelta     = rectTransform.sizeDelta.Round();
					rectTransform.offsetMin     = rectTransform.offsetMin.Round();
					rectTransform.offsetMax     = rectTransform.offsetMax.Round();
					rectTransform.localScale    = rectTransform.localScale.Round();
				}
			}*/

			if (GlobalScriptableObject.instance.isShowRectTransformExtension)
			{
				DrawRectTransformInspectorExtension();
				var creator = new ComponentButtonCreator( rectTransform.gameObject );
				var oldEnabled = GUI.enabled;
				using ( new EditorGUILayout.HorizontalScope( EditorStyles.helpBox ) )
				{
					creator.Create<CanvasGroup, CanvasGroup>( "CanvasGroup Icon" );
					creator.Create<HorizontalLayoutGroup, LayoutGroup>( "HorizontalLayoutGroup Icon" );
					creator.Create<VerticalLayoutGroup, LayoutGroup>( "VerticalLayoutGroup Icon" );
					creator.Create<GridLayoutGroup, LayoutGroup>( "GridLayoutGroup Icon" );
					creator.Create<ContentSizeFitter, ContentSizeFitter>( "ContentSizeFitter Icon" );
				}

				GUI.enabled = oldEnabled;
			}
		}
		
		void DrawRectTransformInspectorExtension()
        {
	        GUILayout.Space(10);
	        var concertTarget = target as RectTransform;
            if (null == concertTarget)
	            return;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Copy");
            if (GUILayout.Button("P"))
            {
                GUIUtility.systemCopyBuffer = CommonUtility.FormatVector3(concertTarget.localPosition, GlobalScriptableObject.instance.Vector3Fmt);
                
                Debug.Log(GUIUtility.systemCopyBuffer);
            }

            if (GUILayout.Button("D"))
            {
                GUIUtility.systemCopyBuffer = CommonUtility.FormatVector2(concertTarget.sizeDelta, GlobalScriptableObject.instance.Vector2Fmt);

                Debug.Log(GUIUtility.systemCopyBuffer);
            }

            if (GUILayout.Button("R"))
            {
                GUIUtility.systemCopyBuffer = CommonUtility.FormatVector3(concertTarget.localRotation.eulerAngles, GlobalScriptableObject.instance.Vector3Fmt);
                Debug.Log(GUIUtility.systemCopyBuffer);
            }

            if (GUILayout.Button("S"))
            {
                GUIUtility.systemCopyBuffer = CommonUtility.FormatVector3(concertTarget.localScale, GlobalScriptableObject.instance.Vector3Fmt);
            }

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Reset");
            if (GUILayout.Button("P"))
            {
                Undo.RecordObject(concertTarget.transform, GetType().FullName);

                concertTarget.localPosition = Vector3.zero;
            }

            if (GUILayout.Button("D"))
            {
                Undo.RecordObject(concertTarget.transform, GetType().FullName);

                concertTarget.sizeDelta = Vector2.zero;
            }

            if (GUILayout.Button("R"))
            {
                Undo.RecordObject(concertTarget.transform, GetType().FullName);

                concertTarget.localRotation = Quaternion.identity;
            }

            if (GUILayout.Button("S"))
            {
                Undo.RecordObject(concertTarget.transform, GetType().FullName);

                concertTarget.localScale = Vector3.one;
            }

            EditorGUILayout.EndHorizontal();
        }

		private void OnEnable()
		{
			if ( nativeEditor == null ) Initialize();

			nativeEditor.GetMethod( "OnEnable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic )?.Invoke( editorInstance, null );
			onSceneGui = nativeEditor.GetMethod( "OnSceneGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
			onValidate = nativeEditor.GetMethod( "OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
		}

		private void OnSceneGUI()
		{
			onSceneGui.Invoke( editorInstance, null );
		}

		private void OnDisable()
		{
			nativeEditor.GetMethod( "OnDisable", BindingFlags.NonPublic | BindingFlags.Instance )?.Invoke( editorInstance, null );
		}

		private void Awake()
		{
			Initialize();
			nativeEditor.GetMethod( "Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )?.Invoke( editorInstance, null );
		}

		private void Initialize()
		{
			nativeEditor   = Assembly.GetAssembly( typeof( Editor ) ).GetType( "UnityEditor.RectTransformEditor" );
			editorInstance = CreateEditor( target, nativeEditor );
		}

		private void OnDestroy()
		{
			nativeEditor.GetMethod( "OnDestroy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )?.Invoke( editorInstance, null );
		}

		private void OnValidate()
		{
			if ( nativeEditor == null ) Initialize();

			onValidate?.Invoke( editorInstance, null );
		}

		private void Reset()
		{
			if ( nativeEditor == null ) Initialize();

			nativeEditor.GetMethod( "Reset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )?.Invoke( editorInstance, null );
		}

		private sealed class ComponentButtonCreator
		{
			private readonly GameObject m_gameObject;

			public ComponentButtonCreator( GameObject gameObject ) => m_gameObject = gameObject;

			public void Create<T1, T2>( string iconName )
				where T1 : Component
				where T2 : Component
			{
				var hasComponent = m_gameObject.GetComponent<T2>() != null;

				GUI.enabled = !hasComponent;

				if ( GUILayout.Button( EditorGUIUtility.IconContent( iconName ), GUILayout.Height( 20 ) ) )
				{
					Undo.AddComponent<T1>( m_gameObject );
				}
			}
		}
	}
    
    /*// [CustomEditor(typeof(RectTransform))]
    public class RectTransformInspectorExtension : UnityProvideEditor 
    {
        
        float mCacheDeltaSizeRatio;

        bool mIsLockedScalingRatio;
        bool mIsBoundEditorUpdate;

        protected override void OnEnable()
        {
            if (target == null)
                return;
            base.OnEnable();
        }

        private void OnDisable()
        {
            if (target == null)
                return;
            base.OnEnable();
        }

        void OnDestroy()
        {
            // EditorApplication.update -= OnEditorApplicationUpdate;
        }
        
        protected override UnityProvideEditorType EditorType => UnityProvideEditorType.RectTransformEditor;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GlobalScriptableObject.instance.isShowRectTransformExtension) 
                DrawRectTransformInspectorExtension();
        }

        void DrawRectTransformInspectorExtension()
        {
            var lineStyle = new GUIStyle();
            lineStyle.normal.background = EditorGUIUtility.whiteTexture;
            lineStyle.stretchWidth = true;
            lineStyle.margin = new RectOffset(0, 0, 7, 7);

            var c = GUI.color;
            var p = GUILayoutUtility.GetRect(GUIContent.none, lineStyle, GUILayout.Height(1));
            p.width -= 70;
            if (Event.current.type == EventType.Repaint)
            {
                GUI.color = EditorGUIUtility.isProSkin
                    ? new Color(0.157f, 0.157f, 0.157f)
                    : new Color(0.5f, 0.5f, 0.5f);
                lineStyle.Draw(p, false, false, false, false);
            }

            EditorGUI.LabelField(new Rect(p.xMax, p.y - 7, 70, 20), "Extensions");
            GUI.color = c;

            var concertTarget = target as RectTransform;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Copy");
            if (GUILayout.Button("P"))
            {
                GUIUtility.systemCopyBuffer = CommonUtility.FormatVector3(concertTarget.localPosition, GlobalScriptableObject.instance.Vector3Fmt);
                
                Debug.Log(GUIUtility.systemCopyBuffer);
            }

            if (GUILayout.Button("D"))
            {
                GUIUtility.systemCopyBuffer = CommonUtility.FormatVector2(concertTarget.sizeDelta, GlobalScriptableObject.instance.Vector2Fmt);

                Debug.Log(GUIUtility.systemCopyBuffer);
            }

            if (GUILayout.Button("R"))
            {
                GUIUtility.systemCopyBuffer = CommonUtility.FormatVector3(concertTarget.localRotation.eulerAngles, GlobalScriptableObject.instance.Vector3Fmt);
                Debug.Log(GUIUtility.systemCopyBuffer);
            }

            if (GUILayout.Button("S"))
            {
                GUIUtility.systemCopyBuffer = CommonUtility.FormatVector3(concertTarget.localScale, GlobalScriptableObject.instance.Vector3Fmt);
            }

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Reset");
            if (GUILayout.Button("P"))
            {
                Undo.RecordObject(concertTarget.transform, GetType().FullName);

                concertTarget.localPosition = Vector3.zero;
            }

            if (GUILayout.Button("D"))
            {
                Undo.RecordObject(concertTarget.transform, GetType().FullName);

                concertTarget.sizeDelta = Vector2.zero;
            }

            if (GUILayout.Button("R"))
            {
                Undo.RecordObject(concertTarget.transform, GetType().FullName);

                concertTarget.localRotation = Quaternion.identity;
            }

            if (GUILayout.Button("S"))
            {
                Undo.RecordObject(concertTarget.transform, GetType().FullName);

                concertTarget.localScale = Vector3.one;
            }

            EditorGUILayout.EndHorizontal();

            // EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            // EditorGUILayout.LabelField("Locked Scaling Ratio");

            // var rect = EditorGUILayout.GetControlRect(false);
            // var xMax = rect.xMax;
            // rect.xMin = xMax - Mathf.Min(149, rect.width);
            // mIsLockedScalingRatio = GUI.Toggle(rect, mIsLockedScalingRatio, "Locked", EditorStyles.miniButton);
            //
            // if (mIsLockedScalingRatio)
            // {
            //     if (!mIsBoundEditorUpdate)
            //     {
            //         EditorApplication.update -= OnEditorApplicationUpdate;
            //         EditorApplication.update += OnEditorApplicationUpdate;
            //
            //         mCacheDeltaSizeRatio = (concertTarget.sizeDelta.y / concertTarget.sizeDelta.x + 0.00001f);
            //     }
            // }
            // else
            // {
            //     if (mIsBoundEditorUpdate)
            //     {
            //         EditorApplication.update -= OnEditorApplicationUpdate;
            //         mIsBoundEditorUpdate = false;
            //     }
            // }

            // EditorGUILayout.EndHorizontal();
        }

        void OnEditorApplicationUpdate()
        {
            mIsBoundEditorUpdate = true;

            var concertTarget = target as RectTransform;
            if (concertTarget == null)
                return;
            var y = concertTarget.sizeDelta.x * mCacheDeltaSizeRatio;
            concertTarget.sizeDelta = new Vector2(concertTarget.sizeDelta.x, y);
        }
    }*/
}