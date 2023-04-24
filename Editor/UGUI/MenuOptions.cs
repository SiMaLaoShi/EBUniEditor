using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//自定义UGUI组件创建方式
namespace UnityEditor.UI
{
    /// <summary>
    ///     This script adds the UI menu options to the Unity Editor.
    /// </summary
    internal static class MenuOptions
    {
        private const string kUILayerName = "UI";

        private const string kStandardSpritePath = "UI/Skin/UISprite.psd";
        private const string kBackgroundSpritePath = "UI/Skin/Background.psd";
        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string kKnobPath = "UI/Skin/Knob.psd";
        private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";
        private const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";
        private const string kMaskPath = "UI/Skin/UIMask.psd";

        //---------------------UGUI快捷键配置-------------------------
        // % => [ctor,command] | # => [shift,shift] | & => [alt,option]
        // Unity 2017.4.25f1 (64-bit) 2017的基本上能适用
        // Text
        private const string TEXTKEY = "GameObject/UI/My/Text %#&t";

        // Image
        private const string IMAGEKEY = "GameObject/UI/My/Image %#&i";

        // Raw Image
        private const string RAWIMAGEKEY = "GameObject/UI/My/Raw Image %#&r";

        // Button
        private const string BUTTONKEY = "GameObject/UI/Button %#&b";

        // Toggle
        private const string TOGGLEKEY = "GameObject/UI/My/Toggle";

        // Slider
        private const string SLIDERKEY = "GameObject/UI/My/Slider";

        // ScrollBar
        private const string SCROLLBARKEY = "GameObject/UI/Scrollbar";

        // DropDown
        private const string DROPDOWNKEY = "GameObject/UI/Dropdown";

        // Input Field
        private const string INPUTFIELDKEY = "GameObject/UI/Input Field";

        // Canvas
        private const string CANVASKEY = "GameObject/UI/Canvas %#&C";

        // Panel
        private const string PANELKEY = "GameObject/UI/Panel %#&P";

        // ScrollView
        private const string SCROLLVIEWKEY = "GameObject/UI/Scroll View %#&S";

        // Event System
        private const string EVENTSYSTEMKEY = "GameObject/UI/Event System";

        // MyScrollView
        private const string MYSCROLLVIEWKEY = "GameObject/UI/My/Scroll";

        // MyButton
        private const string MYBUTTONKEY = "GameObject/UI/My/Button";

        private const string MYBUTTONIMGKEY = "GameObject/UI/My/ButtonIMG";

        // 遮罩
        private const string ADDMASKKEY = "GameObject/UI/My/Mask";

        // ui
        private const string WINDOWKEY = "GameObject/UI/My/StandUI";

        // 圆形Image
        private const string CircleImage = "GameObject/UI/My/CircleImage";

        private static DefaultControls.Resources s_StandardResources;

        private static DefaultControls.Resources GetStandardResources()
        {
            if (s_StandardResources.standard == null)
            {
                s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
                s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpritePath);
                s_StandardResources.inputField =
                    AssetDatabase.GetBuiltinExtraResource<Sprite>(kInputFieldBackgroundPath);
                s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
                s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(kCheckmarkPath);
                s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(kDropdownArrowPath);
                s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(kMaskPath);
            }

            return s_StandardResources;
        }

        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            // Find the best scene view
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;

            // Create world space Plane from canvas position.
            Vector2 localPlanePosition;
            var camera = sceneView.camera;
            var position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform,
                new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) +
                                     itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) +
                                     itemTransform.sizeDelta.y * itemTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) -
                                     itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) -
                                     itemTransform.sizeDelta.y * itemTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            // 这个是让快捷键设置在Prefab模式下也能生效
            if (parent == null)
                parent = Selection.activeGameObject;

            if (parent == null || parent.GetComponentInParent<Canvas>() == null) parent = GetOrCreateCanvasGameObject();

            var uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
            if (parent != menuCommand.context) // not a context click, so center in sceneview
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(),
                    element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
        }

        //-----------------------------------------------------------------
        [MenuItem(CircleImage, false, 1)]
        public static void CreateCircle(MenuCommand menuCommand)
        {
            var go = new GameObject("Img_Circle");
            go.AddComponent<RectTransform>();
            // go.AddComponent<UICircleImage>();
            PlaceUIElementRoot(go, menuCommand);
        }

        // Graphic elements
        [MenuItem(ADDMASKKEY, false, 10)]
        public static void AddMask(MenuCommand command)
        {
            var go = DefaultControls.CreateImage(GetStandardResources());
            go.name = "@ImgMask";
            go.GetComponent<Image>().raycastTarget = true;
            go.GetComponent<Image>().sprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(
                    @"Assets/AssetFolder/GUIAsset/Textures/CommonImg/Common_zhezhao_quanping.png");
            var tr = command.context as Transform;
            PlaceUIElementRoot(go, command);
            go.transform.SetSiblingIndex(0);

            var rect = go.GetComponent<RectTransform>();
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -100, 200);
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -100, 200);
            rect.anchorMax = Vector2.one;
            rect.anchorMin = Vector2.zero;
        }

        [MenuItem(TEXTKEY, false, 1001)]
        public static void AddText(MenuCommand menuCommand)
        {
            var go = DefaultControls.CreateText(GetStandardResources());
            InitText(go.GetComponent<Text>());
            go.name = "Txt_";
            PlaceUIElementRoot(go, menuCommand);
            //go.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
            var ContentSizeFitter = go.AddComponent<ContentSizeFitter>();
            ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static void InitText(Text text)
        {
            text.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/AssetFolder/GUIAsset/Fonts/fzbwk.ttf");
            text.fontSize = 30;
            text.color = Color.black;
            text.fontStyle = FontStyle.Normal;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = "1211";
            text.raycastTarget = false;
        }

        [MenuItem(IMAGEKEY, false, 1002)]
        public static void AddImage(MenuCommand menuCommand)
        {
            var go = DefaultControls.CreateImage(GetStandardResources());
            go.name = "Img_";
            go.GetComponent<Image>().raycastTarget = false;
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem(RAWIMAGEKEY, false, 1003)]
        public static void AddRawImage(MenuCommand menuCommand)
        {
            var go = DefaultControls.CreateRawImage(GetStandardResources());
            go.GetComponent<RawImage>().raycastTarget = false;
            PlaceUIElementRoot(go, menuCommand);
        }

        // Controls

        // Button and toggle are controls you just click on.

        [MenuItem(BUTTONKEY, false, 1004)]
        public static void AddButton(MenuCommand menuCommand)
        {
            var go = DefaultControls.CreateButton(GetStandardResources());
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
            go.GetComponent<Image>().sprite = null;
            PlaceUIElementRoot(go, menuCommand);
            go.GetComponent<RawImage>().raycastTarget = false;
            InitText(go.transform.Find("Text").GetComponent<Text>());
        }


        [MenuItem(TOGGLEKEY, false, 1006)]
        public static void AddToggle(MenuCommand menuCommand)
        {
            var go = DefaultControls.CreateToggle(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        // Slider and Scrollbar modify a number

        [MenuItem(SLIDERKEY, false, 1007)]
        public static void AddSlider(MenuCommand menuCommand)
        {
            var go = DefaultControls.CreateSlider(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem(SCROLLBARKEY, false, 1008)]
        public static void AddScrollbar(MenuCommand menuCommand)
        {
            var go = DefaultControls.CreateScrollbar(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        // More advanced controls below

        [MenuItem(DROPDOWNKEY, false, 1009)]
        public static void AddDropdown(MenuCommand menuCommand)
        {
            var go = DefaultControls.CreateDropdown(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem(INPUTFIELDKEY, false, 1010)]
        public static void AddInputField(MenuCommand menuCommand)
        {
            var go = DefaultControls.CreateInputField(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            var size = new Vector2(600, 100);
            go.GetComponent<RectTransform>().sizeDelta = size;
            go.GetComponent<Image>().sprite = null;
            go.GetComponent<Image>().color = new Color(98, 168, 202, 184);
            InitText(go.transform.Find("Placeholder").GetComponent<Text>());
            go.transform.Find("Placeholder").GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            //            go.transform.Find("Placeholder").GetComponent<RectTransform>().sizeDelta = size;
            //            go.transform.Find("Text").GetComponent<RectTransform>().sizeDelta = size;
            InitText(go.transform.Find("Text").GetComponent<Text>());
            go.transform.Find("Text").GetComponent<Text>().text = "";
            go.transform.Find("Text").GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        }

        // Containers

        [MenuItem(CANVASKEY, false, 1011)]
        public static void AddCanvas(MenuCommand menuCommand)
        {
            var go = CreateNewUI();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            if (go.transform.parent as RectTransform)
            {
                var rect = go.transform as RectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
            }

            Selection.activeGameObject = go;
        }

        [MenuItem(PANELKEY, false, 1012)]
        public static void AddPanel(MenuCommand menuCommand)
        {
            var go = DefaultControls.CreatePanel(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);

            // Panel is special, we need to ensure there's no padding after repositioning.
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        [MenuItem(SCROLLVIEWKEY, false, 1013)]
        public static void AddScrollView(MenuCommand menuCommand)
        {
            var go = DefaultControls.CreateScrollView(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem(MYSCROLLVIEWKEY, false, 1014)]
        public static void AddMyScrollView(MenuCommand menuCommand)
        {
            //GameObject go = DefaultControls.CreateScrollView(GetStandardResources());
            //GameObject.DestroyImmediate(go.transform.Find("Scrollbar Horizontal").gameObject);
            //GameObject.DestroyImmediate(go.transform.Find("Scrollbar Vertical").gameObject);
            //GameObject.DestroyImmediate(go.transform.Find("Viewport").GetComponent<Image>());

            var go = new GameObject("ScrollView");
            var scroll = go.AddComponent<ScrollRect>();
            go.AddComponent<RectMask2D>();
            scroll.viewport = go.GetComponent<RectTransform>();
            var content = new GameObject("Content");
            var img = content.AddComponent<Image>();
            Object.DestroyImmediate(img);
            content.transform.SetParent(go.transform);
            content.AddComponent<CanvasRenderer>();
            // content.AddComponent<NoDrawingRayCast>();
            var contentSizeFitter = content.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content.GetComponent<RectTransform>();
            var rect = content.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            content.AddComponent<GridLayoutGroup>();
            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem(MYBUTTONKEY, false, 1015)]
        public static void CreateButton(MenuCommand menuCommand)
        {
            // CreatButtonCore(false, menuCommand);
        }

        [MenuItem(MYBUTTONIMGKEY, false, 1016)]
        public static void CreateButtonImage(MenuCommand menuCommand)
        {
            // CreatButtonCore(true, menuCommand);
        }

        // Helper methods

        public static GameObject CreateNewUI()
        {
            // Root for the UI
            var root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer(kUILayerName);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            // if there is no event system add one...
            CreateEventSystem(false);
            return root;
        }

        [MenuItem(EVENTSYSTEMKEY, false, 1016)]
        public static void CreateEventSystem(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            CreateEventSystem(true, parent);
        }

        private static void CreateEventSystem(bool select)
        {
            CreateEventSystem(select, null);
        }

        private static void CreateEventSystem(bool select, GameObject parent)
        {
            var esys = Object.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

            if (select && esys != null) Selection.activeGameObject = esys.gameObject;
        }

        // Helper function that returns a Canvas GameObject; preferably a parent of the selection, or other existing Canvas.
        public static GameObject GetOrCreateCanvasGameObject()
        {
            var selectedGo = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            var canvas = selectedGo != null ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use just any canvas..
            canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in the scene at all? Then create a new one.
            return CreateNewUI();
        }
        
        #region UGUI 类似PS的移动，ijkl，

#if UNITY_2017
        private const string up = "UGUI/Move/MoveUp %i";
        private const string down = "UGUI/Move/MoveDown %k";
        private const string left = "UGUI/Move/MoveLeft %j";
        private const string right = "UGUI/Move/MoveRight %l";
#else
    private const string up = "GameObject/Move/MoveUp &UP";
    private const string down = "GameObject/Move/MoveDown &DOWN";
    private const string left = "GameObject/Move/MoveLeft &LEFT";
    private const string right = "GameObject/Move/MoveRight &RIGHT";
#endif

        [MenuItem(up, false, 10)]
        public static void MoveUp()
        {
            Move(Vector3.up);
        }

        [MenuItem(down, false, 10)]
        public static void MoveDown()
        {
            Move(Vector3.down);
        }

        [MenuItem(left, false, 10)]
        public static void MoveLeft()
        {
            Move(Vector3.left);
        }

        [MenuItem(right, false, 10)]
        public static void MoveRight()
        {
            Move(Vector3.right);
        }

        private static void Move(Vector3 v3)
        {
            foreach (var item in Selection.gameObjects)
                item.transform.localPosition += v3;
        }

        #endregion
    }
}