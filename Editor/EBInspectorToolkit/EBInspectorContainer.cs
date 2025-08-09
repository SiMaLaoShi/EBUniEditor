#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace EBUniEditor.Editor.Inspector {

    public class EBInspectorContainer {

        public static GUIStyle BoldLabelStyle;
        public static float SearchBarHeight;
        public static EBInspectorPersistentData PersistentData;
        public static Texture IconAtlas;
        public static Texture AllComponentsIcon;
        public static Texture CloseIcon;
        
        public static GUIStyle LeftToolBarStyle;
        public static GUIContent CopyButtonContent;
        
        public static GUIStyle RightToolBarStyle;
        public static GUIContent PasteButtonContent;

        private const string AllButtonName = "All";
        private const float DragThreshold = 12f;
        private const float ContainerMargin = 4f;
        private const float SearchComponentListSpace = 4f;
        private const float RowHeight = 25f;
        private const float InspectorScrollBarWidth = 12.666666667f;
        private const float ToolBarButtonWidth = 50f;

        private const string InspectorListClassName = "unity-inspector-editors-list";
        private const string InspectorScrollClassName = "unity-inspector-root-scrollview";
        private const string InspectorNoMultiEditClassName = "unity-inspector-no-multi-edit-warning";
        private const string MainContainerName = "EBInspector Main";
        private const string SearchResultsName = "EBInspector SearchResults";
        
        private static Vector2 iconSize = new Vector2(12, 12);
        private static Vector2 toolBarIconSize = new Vector2(12, 12);
        
        public readonly EditorWindow InspectorWindow;
        
        private Object targetObject;
        private VisualElement editorListVisual;
        private IMGUIContainer mainGuiContainer;
        private IMGUIContainer pinnedHeaderContainer;
        private IMGUIContainer searchResultsGuiContainer;
        private IMGUIContainer pinnedDividerContainer;
        private ScrollView inspectorScrollView;

        private List<int> selectedComponentIds;
        private List<int> validComponentIds = new List<int>();
        private List<int> previousValidComponentIds = new List<int>();
        private Dictionary<int, Component> componentFromIndex = new Dictionary<int, Component>();
        private HashSet<string> noMultiEditVisualElements = new HashSet<string>();
        
        private Vector2 mainScrollPosition;
        
        private int lastComponentCount;
        private int lastRowCount;

        private enum AssetType { NotImportant, HierarchyGameObject, HierarchyPrefab, HierarchyModel, ProjectPrefab }
        private AssetType targetAssetType;
        
        private List<ComponentSearchResults> searchResults = new List<ComponentSearchResults>();
        private const double TimeAfterLastKeyPressToSearch = 0.15;
        private double timeOfLastSearchUpdate;
        private bool performSearchFlag;
        
        private bool inspectorWasLocked;
        private PropertyInfo lockedPropertyInfo;
        
        private bool multiSelectModifier;
        private bool rangeSelectModifier;
        private int rangeModifierPivot;

        private const string DragAndDropKey = "EBInspectorDragAndDrop";
        private bool isDragging;
        private bool dragHandlerSet;
        private bool canStartDrag;
        private int dragId;
        private Vector2 initialDragMousePos;
        
        public EBInspectorContainer(EditorWindow window, Object obj) {
            InspectorWindow = window;
            lockedPropertyInfo = window.GetType().GetProperty("isLocked", BindingFlags.Public | BindingFlags.Instance);
            inspectorWasLocked = IsInspectorLocked();
            inspectorScrollView = (ScrollView)InspectorWindow.rootVisualElement.Q(null, InspectorScrollClassName);
            SetTargetObject(obj);
        }

        public bool IsInspectorLocked() {
            return (bool)lockedPropertyInfo.GetValue(InspectorWindow);
        }

        public void RemoveGUI() {
            if (!IsTargetObjectValid()) return;

            if (IsShowingMainGUI()) {
                editorListVisual?.RemoveAt(GetMainContainerIndex());
            }

            if (IsShowingSearchResults()) {
                editorListVisual?.RemoveAt(GetSearchResultsIndex());
            }
        }

        public void SetTargetObject(Object obj) {
            targetObject = obj;
            
            if (!targetObject) {
                targetAssetType = AssetType.NotImportant;
                return;
            }
            
            // Determine asset type
            {
                bool isAsset = AssetDatabase.Contains(targetObject);
                PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(targetObject);
                
                if (isAsset && prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant) {
                    targetAssetType = AssetType.ProjectPrefab;
                }
                else if (!isAsset && prefabType is PrefabAssetType.Model) {
                    targetAssetType = AssetType.HierarchyModel;
                }
                else if (!isAsset && prefabType == PrefabAssetType.Regular) {
                    targetAssetType = AssetType.HierarchyPrefab;
                }
                else if (!isAsset && prefabType is PrefabAssetType.NotAPrefab) {
                    targetAssetType = AssetType.HierarchyGameObject;
                }
                else {
                    targetAssetType = AssetType.NotImportant;
                }
            }
            
            searchResults.Clear();
            RefreshNoMultiInspectVisualsSet();
            PersistentData.AddDataForObject(targetObject);
            selectedComponentIds = PersistentData.GetSelectedComponentIds(targetObject);
            
            if (HasTextInSearchField()) {
                PerformSearch();
                if (!HasSearchResults()) {
                    PersistentData.SetSearchString(targetObject, string.Empty);
                }
            }
        }

        public void UpdateContainer() {
            if (!IsTargetObjectValid()) return;

            if (EBInspectorSettings.TransformOnlyDisable && OnlyHasTransform()) return;

            editorListVisual ??= InspectorWindow.rootVisualElement.Q(null, InspectorListClassName);
            
            if (editorListVisual == null) return;

            if (performSearchFlag && EditorApplication.timeSinceStartup - timeOfLastSearchUpdate > TimeAfterLastKeyPressToSearch) {
                PerformSearch();
                performSearchFlag = false;
                searchResultsGuiContainer?.MarkDirtyRepaint();
            }
            
            if (WasJustUnlocked() && Selection.activeObject != targetObject) {
                SetTargetObject(Selection.activeObject); 
                UpdateComponentVisibility();
            }

            if (!IsShowingMainGUI() && editorListVisual.childCount > GetMainContainerIndex()) {
                float containerHeight = CalculateMainContainerHeight();
                
                mainGuiContainer = new IMGUIContainer();
                mainGuiContainer.name = MainContainerName;
                mainGuiContainer.style.width = FullLength();
                mainGuiContainer.style.height = containerHeight;
                mainGuiContainer.style.minHeight = containerHeight; 
                mainGuiContainer.onGUIHandler = DrawMainGUI;
                
                SetMargin(mainGuiContainer.style, ContainerMargin);
                editorListVisual.Insert(GetMainContainerIndex(), mainGuiContainer);
                UpdateComponentVisibility();
            }

            bool showingSearchResults = IsShowingSearchResults();
            
            if (!showingSearchResults && HasSearchResults() && editorListVisual.childCount > GetSearchResultsIndex()) {
                searchResultsGuiContainer = new IMGUIContainer();
                searchResultsGuiContainer.name = SearchResultsName;
                searchResultsGuiContainer.style.width = FullLength();
                searchResultsGuiContainer.style.height = FullLength(); 
                searchResultsGuiContainer.onGUIHandler = DrawSearchResultsGUI;
                editorListVisual.Insert(GetSearchResultsIndex(), searchResultsGuiContainer);
            }
            
            if (showingSearchResults && !HasSearchResults()) {
                RemoveSearchGUI();
                ToggleAllComponentVisibility(true);
            }
            
#if UNITY_2021
            Fix2021EditorMargins();
#endif
        }

        public void OnHierarchyGUI() {
            // if (DragAndDrop.GetGenericData(DragAndDropKey) is not bool initiatedDrag || !initiatedDrag) return;
            //
            // if (Event.current.type == EventType.DragUpdated && !dragHandlerSet) {
            //     DragAndDrop.AddDropHandler(HierarchyDropHandler);
            //     dragHandlerSet = true;
            //     Event.current.Use();
            // }
            //
            // if (Event.current.type == EventType.DragExited && dragHandlerSet) {
            //     DragAndDrop.RemoveDropHandler(HierarchyDropHandler);
            //     dragHandlerSet = false;
            //     Event.current.Use();
            // }
        }

        private void DrawMainGUI() {
            if (!IsTargetObjectValid()) return;
            
            Rect reservedRect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            bool showCopyPasteOnly = EBInspectorSettings.TransformOnlyKeepCopyPaste && OnlyHasTransform();
            if (!EBInspectorSettings.HideToolbar || showCopyPasteOnly) {
                DrawToolBar(reservedRect, showCopyPasteOnly);
                reservedRect = ShiftRectStartVertically(reservedRect, SearchBarHeight + SearchComponentListSpace);
            }

            List<Component> components = GetAllVisibleComponents();
            float[] buttonWidths = GetButtonWidths(components);
            
            int newComponentCount = components.Count;
            int newRowCount = GetRowCount(reservedRect.width, buttonWidths);
            
            // Create component data mapping
            componentFromIndex.Clear();
            validComponentIds.Clear();
            for (int i = 0; i < components.Count; i++) {
                componentFromIndex.Add(i, components[i]);
                validComponentIds.Add(components[i].GetInstanceID());
            }

            // Check for container resizing
            bool resizeRequired = newComponentCount != lastComponentCount || newRowCount != lastRowCount;
            if (resizeRequired) {
                ResizeGUIContainer();
            }
            
            // Remove deleted components from selection
            if (newComponentCount < lastComponentCount) {
                for (int i = selectedComponentIds.Count - 1; i >= 0; i--) {
                    if (!validComponentIds.Contains(selectedComponentIds[i])) {
                        selectedComponentIds.RemoveAt(i);
                    }
                }
            }
            
            bool componentsChanged = newComponentCount < lastComponentCount || !CompareComponentIds(validComponentIds, previousValidComponentIds);
            
            // Update tracking variables
            previousValidComponentIds.Clear();
            foreach (int validComponentId in validComponentIds) {
                previousValidComponentIds.Add(validComponentId);
            }
            lastComponentCount = newComponentCount;
            lastRowCount = newRowCount;
            
            GetScrollViewDimensions(reservedRect, newRowCount, out Rect innerScrollRect, out Rect outerScrollRect);
            List<Rect> buttonPlacements = GetButtonPlacements(innerScrollRect, components, buttonWidths);

            if (Event.current.type is EventType.MouseDown && Event.current.button is 1) {
                ShowContextMenu(components, buttonPlacements);
                Event.current.Use();
            }
            
            if (showCopyPasteOnly) return;

            // Update input modifiers
            EventModifiers modifiers = Event.current.modifiers;
            multiSelectModifier = modifiers.HasFlag(EventModifiers.Control);
            rangeSelectModifier = modifiers.HasFlag(EventModifiers.Shift);
            
            UpdateDragAndDrop();

            EditorGUI.BeginChangeCheck();
            DrawComponentScrollView(buttonPlacements, components, innerScrollRect, outerScrollRect);
            if (EditorGUI.EndChangeCheck() || componentsChanged) {
                UpdateComponentVisibility();
            }
        }

        private void DrawComponentScrollView(List<Rect> placementRects, List<Component> components, Rect innerScrollRect, Rect outerScrollRect) {
            mainScrollPosition = GUI.BeginScrollView(outerScrollRect, mainScrollPosition, innerScrollRect, GUIStyle.none, GUIStyle.none);
            
            // Handle the All button
            { 
                const int allButtonId = -1;
                bool prevAllButtonToggle = AllIsSelected() && !HasTextInSearchField();
                Rect allButtonRect = placementRects[0];
                
                if (allButtonRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown) {
                    canStartDrag = true;
                    dragId = allButtonId;
                    ClearSearchOnComponentButtonPress();
                }
                
                bool draggingAll = dragId == allButtonId && !prevAllButtonToggle;

                if (DrawToggleButton(allButtonRect, AllComponentsIcon, AllButtonName, prevAllButtonToggle, draggingAll)) {
                    selectedComponentIds.Clear();
                    rangeModifierPivot = 0;
                }
            }
            
            for (int i = 0; i < components.Count; i++) {
                Component component = components[i];
                Rect buttonRect = placementRects[i + 1];
                int componentId = component.GetInstanceID();
                
                if (buttonRect.Contains(Event.current.mousePosition)) {
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                        canStartDrag = true;
                        dragId = componentId;
                    }
                }
                
                string componentName = component.GetType().Name;
                GUIContent content = EditorGUIUtility.ObjectContent(component, component.GetType());
                bool prevToggle = selectedComponentIds.Contains(componentId);
                bool draggingButton = componentId == dragId && !prevToggle;
                
                bool toggled = DrawToggleButton(buttonRect, content.image, componentName, prevToggle, draggingButton);
                
                if (toggled && !prevToggle) {
                    OnButtonToggleOn(i);
                    ClearSearchOnComponentButtonPress();
                }
                else if (!toggled && prevToggle) {
                    OnButtonToggleOff(i);
                    ClearSearchOnComponentButtonPress();
                }
            }
            
            GUI.EndScrollView();
        }

        private void GetScrollViewDimensions(Rect reservedRect, int rowCount, out Rect innerScrollRect, out Rect outerScrollRect) {
            innerScrollRect = new Rect(reservedRect) { height = rowCount * RowHeight };
            outerScrollRect = new Rect(reservedRect) { height = RowHeight * EBInspectorSettings.MaxNumberOfRows };
        }

        private List<Rect> GetButtonPlacements(Rect scrollViewRect, List<Component> components, float[] buttonWidths) {
            List<Rect> placements = new List<Rect>(); 
            
            Rect placementRect = scrollViewRect;
            
            float usableWidth = scrollViewRect.width;
            if (!ShowingVerticalScrollBar()) {
                usableWidth -= InspectorScrollBarWidth;
            }
            
            Rect allButtonRect = new Rect(placementRect.position, new Vector2(buttonWidths[0], RowHeight));
            placements.Add(allButtonRect);
            
            float currentWidth = usableWidth;
            currentWidth -= buttonWidths[0];
            placementRect.position += new Vector2(buttonWidths[0], 0f);

            for (int i = 0; i < components.Count; i++) {
                float buttonWidth = buttonWidths[i + 1];
                
                if (currentWidth < buttonWidth) {
                    placementRect.position = new Vector2(scrollViewRect.position.x, placementRect.position.y + RowHeight);
                    currentWidth = usableWidth;
                }
                currentWidth -= buttonWidth;

                Rect buttonRect = new Rect(placementRect.position, new Vector2(buttonWidth, RowHeight));
                placements.Add(buttonRect);

                placementRect.position += new Vector2(buttonWidth, 0f);
            }

            return placements;
        }
        
        private void ClearSearchOnComponentButtonPress() {
            if (HasTextInSearchField()) {
                PersistentData.SetSearchString(targetObject, string.Empty);
                searchResults.Clear();
                GUI.changed = true;
                RemoveSearchGUI();
                ToggleAllComponentVisibility(true);
            }
        }

        private bool DrawToggleButton(Rect placement, Texture icon, string label, bool toggled, bool beingDragged) {
            if (!toggled && isDragging && beingDragged) {
                toggled = true;
                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseUp && placement.Contains(Event.current.mousePosition) && Event.current.button == 0) {
                toggled = !toggled;
            }

            int uniqueControlId = GUIUtility.GetControlID(FocusType.Passive);
            GUI.Toggle(placement, uniqueControlId, toggled, GUIContent.none, GUI.skin.button);
            
            Vector2 iconPos = new Vector2(placement.position.x + BoldLabelStyle.margin.right, 0f);
            Rect iconRect = CenterRectVertically(placement, new Rect(iconPos, iconSize));
            GUI.DrawTexture(iconRect, icon);
            
            Vector2 labelSize = BoldLabelStyle.CalcSize(new GUIContent(label));
            Vector2 labelPos = new Vector2(iconRect.xMax, 0f);
            Rect labelRect = new Rect(labelPos, labelSize);
            labelRect = CenterRectVertically(placement, labelRect);
            GUI.Label(labelRect, label, BoldLabelStyle);

            return toggled;
        }
        
        private void OnButtonToggleOn(int componentIndex) {
            int componentId = ComponentIdFromIndex(componentIndex);
            
            if (multiSelectModifier && !rangeSelectModifier) {
                rangeModifierPivot = componentIndex;
                selectedComponentIds.Add(componentId);
                return;
            }
            
            if (rangeSelectModifier) {
                if (AllIsSelected()) {
                    rangeModifierPivot = componentIndex;
                    selectedComponentIds.Add(componentId);
                    return;
                }
                
                AddRangeToSelected(componentIndex);
                return;
            }

            selectedComponentIds.Clear();
            selectedComponentIds.Add(componentId);
            rangeModifierPivot = componentIndex;
        }
        
        private void OnButtonToggleOff(int componentIndex) {
            int componentId = ComponentIdFromIndex(componentIndex);
            
            if (rangeSelectModifier && selectedComponentIds.Count <= 1) return;
            
            if (!multiSelectModifier && !rangeSelectModifier && selectedComponentIds.Count > 1) {
                selectedComponentIds.Clear();
                selectedComponentIds.Add(componentId);
                rangeModifierPivot = componentIndex;
                return;
            }
            
            if (rangeSelectModifier) {
                if (componentIndex == rangeModifierPivot) {
                    selectedComponentIds.Clear();
                    selectedComponentIds.Add(componentId);
                    return;
                }
                
                AddRangeToSelected(componentIndex);

                if (componentIndex < rangeModifierPivot) {
                    int islandMin = componentIndex;
                    while (selectedComponentIds.Contains(ComponentIdFromIndex(islandMin - 1))) {
                        islandMin -= 1;
                    }

                    for (int i = islandMin; i < componentIndex; i++) {
                        selectedComponentIds.Remove(ComponentIdFromIndex(i));
                    }
                }
                else {
                    int islandMax = componentIndex;
                    while (selectedComponentIds.Contains(ComponentIdFromIndex(islandMax + 1))) {
                        islandMax += 1;
                    }
                    
                    for (int i = componentIndex + 1; i <= islandMax; i++) {
                        selectedComponentIds.Remove(ComponentIdFromIndex(i));
                    }
                }
                
                return;
            }
            
            selectedComponentIds.Remove(componentId);
        }
        
        private void AddRangeToSelected(int componentIndex) {
            (int min, int max) = rangeModifierPivot < componentIndex ? (rangeModifierPivot, componentIndex) : (componentIndex, rangeModifierPivot);
            for (int i = min; i <= max; i++) {
                int id = ComponentIdFromIndex(i);
                if (!selectedComponentIds.Contains(id)) {
                    selectedComponentIds.Add(id);
                }
            }
        }
        
        private void DrawToolBar(Rect placementRect, bool showCopyPasteOnly) {
            placementRect.height = SearchBarHeight;
            
            float fullWidth = placementRect.width;
            float xStartPos = placementRect.position.x;
            
            if (!EBInspectorSettings.HideCopyPaste || showCopyPasteOnly) {
                if (DrawToolBarButton(placementRect, true)) {
                    CopySelectedToClipboard();
                }
                placementRect.position += new Vector2(ToolBarButtonWidth, 0f);
                if (DrawToolBarButton(placementRect, false)) {
                    PasteFromClipboard();
                }
                placementRect.position += new Vector2(ToolBarButtonWidth + ContainerMargin, 0f);
            }
            
            if (showCopyPasteOnly) return;
            
            placementRect.width = fullWidth - (placementRect.position.x - xStartPos);

            const float crossSize = 11;
            const float crossDistFromEndOfSearch = 16;
            Rect crossPlacement = placementRect;
            crossPlacement.width = crossSize;
            crossPlacement.height = crossSize;
            crossPlacement.position = new Vector2(placementRect.xMax - crossDistFromEndOfSearch, placementRect.position.y);
            crossPlacement = CenterRectVertically(placementRect, crossPlacement);
            
            // Handle X input before drawing search field because it eats the input of overlayed elements
            string searchText = PersistentData.GetSearchString(targetObject);
            bool showX = searchText != string.Empty;
            bool pressedX = false;
            if (showX) {
                if (crossPlacement.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp) {
                    searchText = string.Empty;
                    searchResults.Clear();
                    pressedX = true;
                }
            }
            
            int prevSearchLen = searchText.Length;
            GUI.SetNextControlName("SearchField");
            searchText = GUI.TextField(placementRect, searchText, EditorStyles.toolbarSearchField);

            // Deselect any selected components when typing in search 
            if (!string.IsNullOrWhiteSpace(searchText)) {
                selectedComponentIds.Clear();
            }
            
            // If we click outside of the search bar unfocus it
            if (pressedX || !placementRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown) {
                GUI.FocusControl(null);
                if (string.IsNullOrWhiteSpace(searchText)) {
                    searchText = string.Empty;
                }
            }

            // Draw X after search field so it shows on top
            if (showX) {
                Color prevColor = GUI.color;
                GUI.color = new Vector4(prevColor.r, prevColor.g, prevColor.b, 0.7f);
                GUI.Button(crossPlacement, CloseIcon, GUIStyle.none);
                GUI.color = prevColor;
            }
            
            if (prevSearchLen != searchText.Length) {
                performSearchFlag = true;
                timeOfLastSearchUpdate = EditorApplication.timeSinceStartup;
            }

            PersistentData.SetSearchString(targetObject, searchText);
        }
        
        private bool DrawToolBarButton(Rect placement, bool copy) {
            placement.width = ToolBarButtonWidth; // 增加宽度以容纳中文文字
    
            // 使用中文文字内容
            GUIContent buttonContent = new GUIContent(
                copy ? "复制" : "粘贴",
                copy ? "复制选中的组件到剪贴板" : "从剪贴板粘贴组件"
            );

            bool pressed = GUI.Button(placement, buttonContent);

            return pressed;
        }
        
        private List<Component> GetComponentsFromSelection() {
            if (!IsTargetObjectValid()) {
                return null;
            }
            
            List<Component> allComponents = GetAllVisibleComponents();
            
            if (AllIsSelected()) {
                return allComponents;
            }
            
            List<Component> selectedComponents = new List<Component>(selectedComponentIds.Count);
            foreach (int componentId in selectedComponentIds) {
                selectedComponents.Add(ComponentFromId(componentId));
            }
            return selectedComponents;
        }
        
        private class ComponentSearchResults {
            public Component Component;
            public SerializedObject SerializedComponent;
            public List<SerializedProperty> Fields = new List<SerializedProperty>();
        }
        
        private void PerformSearch() {
            string searchText = PersistentData.GetSearchString(targetObject);
            if (string.IsNullOrWhiteSpace(searchText)) {
                searchResults.Clear();
                return;
            }

            List<Component> components = GetAllVisibleComponents();
            if (components == null) return;
            
            searchResults.Clear();
            
            foreach (Component component in components) {
                ComponentSearchResults results = null;
                SerializedObject serializedComponent = new SerializedObject(component);
                List<SerializedProperty> fields = GetComponentFields(serializedComponent);
                
                if (fields == null) continue;
                
                foreach (SerializedProperty field in fields) {
                    if (FuzzyMatch(field.displayName, searchText)) {
                        searchResults ??= new List<ComponentSearchResults>();
                        results ??= new ComponentSearchResults {
                            Component = component, 
                            SerializedComponent = serializedComponent 
                        };
                        results.Fields.Add(field);
                    }
                }

                if (results != null) {
                    searchResults.Add(results);
                }
            }
        }
        
        private bool FuzzyMatch(string stringToSearch, string pattern) {
            const int adjacencyBonus = 5;      
            const int separatorBonus = 10;      
            const int camelBonus = 10;           

            const int leadingLetterPenalty = -3;  
            const int maxLeadingLetterPenalty = -9;
            const int unmatchedLetterPenalty = -1;

            int score = 0;
            int patternIdx = 0;
            int patternLength = pattern.Length;
            int strIdx = 0;
            int strLength = stringToSearch.Length;
            bool prevMatched = false;
            bool prevLower = false;
            bool prevSeparator = true;                   

            char? bestLetter = null;
            char? bestLower = null;
            int bestLetterScore = 0;

            while (strIdx != strLength) {
                char? patternChar = patternIdx != patternLength ? pattern[patternIdx] as char? : null;
                char strChar = stringToSearch[strIdx];

                char? patternLower = patternChar != null ? char.ToLower((char)patternChar) as char? : null;
                char strLower = char.ToLower(strChar);
                char strUpper = char.ToUpper(strChar);

                bool nextMatch = patternChar != null && patternLower == strLower;
                bool rematch = bestLetter != null && bestLower == strLower;

                bool advanced = nextMatch && bestLetter != null;
                bool patternRepeat = bestLetter != null && patternChar != null && bestLower == patternLower;
                if (advanced || patternRepeat) {
                    score += bestLetterScore;
                    bestLetter = null;
                    bestLower = null;
                    bestLetterScore = 0;
                }

                if (nextMatch || rematch) {
                    int newScore = 0;

                    if (patternIdx == 0) {
                        int penalty = Math.Max(strIdx * leadingLetterPenalty, maxLeadingLetterPenalty);
                        score += penalty;
                    }

                    if (prevMatched) {
                        newScore += adjacencyBonus;
                    }

                    if (prevSeparator) {
                        newScore += separatorBonus;
                    }

                    if (prevLower && strChar == strUpper && strLower != strUpper) {
                        newScore += camelBonus;
                    }

                    if (nextMatch) {
                        ++patternIdx;
                    }

                    if (newScore >= bestLetterScore) {
                        if (bestLetter != null) {
                            score += unmatchedLetterPenalty;
                        }

                        bestLetter = strChar;
                        bestLower = char.ToLower((char)bestLetter);
                        bestLetterScore = newScore;
                    }

                    prevMatched = true;
                }
                else {
                    score += unmatchedLetterPenalty;
                    prevMatched = false;
                }

                prevLower = strChar == strLower && strLower != strUpper;
                prevSeparator = strChar == '_' || strChar == ' ';

                ++strIdx;
            }

            if (bestLetter != null) {
                score += bestLetterScore;
            }

            const int idealScore = -10;
            return patternIdx == patternLength && score >= idealScore;
        }

        // private DragAndDropVisualMode HierarchyDropHandler(int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform) {
        //     const int hierarchyId = -1314;
        //     
        //     bool copying = dropMode == HierarchyDropFlags.DropUpon && dropTargetInstanceID != hierarchyId;
        //     bool creating = dropTargetInstanceID == hierarchyId || dropMode == HierarchyDropFlags.DropBetween || dropMode == HierarchyDropFlags.None;
        //
        //     DragAndDropVisualMode visualMode = DragAndDropVisualMode.None;
        //     if (copying) {
        //         visualMode = DragAndDropVisualMode.Copy;
        //     }
        //     else if (creating) {
        //         visualMode = DragAndDropVisualMode.Move;
        //     }
        //
        //     if (!perform || (!copying && !creating)) {
        //         return visualMode;
        //     }
        //     
        //     List<Component> components = GetComponentsFromSelection();
        //     if (components == null) {
        //         return visualMode;
        //     }
        //     
        //     if (copying && EditorUtility.InstanceIDToObject(dropTargetInstanceID) is GameObject gameObject) {
        //         GroupUndoAction("Copy Components", () => gameObject.PasteComponents(components));
        //         EditorApplication.delayCall += () => Selection.activeObject = gameObject;
        //         return visualMode;
        //     }
        //     
        //     GroupUndoAction("Create Object from Components", () => {
        //         GameObject newGameObject = new GameObject("GameObject");
        //         Undo.RegisterCreatedObjectUndo(newGameObject, string.Empty);
        //         newGameObject.PasteComponentsFromEmpty(components);
        //         EditorApplication.delayCall += () => Selection.activeObject = newGameObject;
        //     });
        //
        //     return visualMode;
        // }

        private void GroupUndoAction(string undoName, Action action) {
            Undo.IncrementCurrentGroup();
            int currentUndoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(undoName);
            action.Invoke();
            Undo.CollapseUndoOperations(currentUndoGroup);
        }
        
        private void UpdateDragAndDrop() {
            bool mouseDragEvent = Event.current.type == EventType.MouseDrag;

            if (!isDragging && canStartDrag && mouseDragEvent) {
                initialDragMousePos = Event.current.mousePosition;
                canStartDrag = false;
                return;
            }

            if (initialDragMousePos != Vector2.zero && mouseDragEvent && Vector2.Distance(initialDragMousePos, Event.current.mousePosition) >= DragThreshold) {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(DragAndDropKey, true);
                DragAndDrop.StartDrag(MainContainerName);
                isDragging = true;
            }
            
            // DragExited is set when we drag out of the container or stop dragging inside it
            if (Event.current.type == EventType.DragExited) {
                canStartDrag = false;
                isDragging = false;
                initialDragMousePos = Vector2.zero;
                Event.current.Use();
            }
        }

        private bool CompareComponentIds(List<int> list0, List<int> list1) {
            if (list0.Count != list1.Count) {
                return false;
            }

            for (int i = 0; i < list0.Count; i++) {
                if (list0[i] != list1[i]) {
                    return false;
                }
            }

            return true;
        }

        private void ResizeGUIContainer() {
            float height = CalculateMainContainerHeight();
            mainGuiContainer.style.height = height; 
            mainGuiContainer.style.minHeight = height; 
            mainGuiContainer.style.width = FullLength();
        }
        
        private void DrawSearchResultsGUI() {
            if (!HasSearchResults() || !IsTargetObjectValid()) return;

            ToggleAllComponentVisibility(false);
            
            foreach (ComponentSearchResults result in searchResults) {
                
                EditorGUILayout.InspectorTitlebar(true, result.Component, false);
                
                EditorGUI.indentLevel++;
                foreach (SerializedProperty property in result.Fields) {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(property, true);
                    if (EditorGUI.EndChangeCheck()) {
                        result.SerializedComponent.ApplyModifiedProperties();
                    }
                }
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space();
            }
        }
        
        private void UpdateComponentVisibility() {
            int startIndex = GetComponentStartIndex();
            int skippedCount = 0;
            
            for (int i = startIndex; i < editorListVisual.childCount; i++) {
                if (noMultiEditVisualElements.Contains(editorListVisual[i].name)) {
                    skippedCount++;
                    continue;
                }
                
                int componentIndex = i - startIndex - skippedCount;
                if (componentFromIndex.TryGetValue(componentIndex, out Component component)) {
                    bool showComponent = selectedComponentIds.Count <= 0 || selectedComponentIds.Contains(component.GetInstanceID());
                    editorListVisual[i].style.display = showComponent ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }

        private void ToggleAllComponentVisibility(bool show) {
            int startIndex = IsShowingSearchResults() ? GetSearchResultsIndex() + 1 : GetMainContainerIndex() + 1;
            for (int i = startIndex; i < editorListVisual.childCount; i++) {
                editorListVisual[i].style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private bool IsShowingMainGUI() {
            int insertIndex = GetMainContainerIndex();

            if (insertIndex >= editorListVisual.childCount) {
                return false;
            }

            VisualElement potentialMainContainer = editorListVisual.hierarchy.ElementAt(insertIndex);
            return potentialMainContainer != null && potentialMainContainer.name == MainContainerName;
        }

        private bool IsShowingSearchResults() {
            int insertIndex = GetSearchResultsIndex();
            
            if (insertIndex >= editorListVisual.childCount) {
                return false;
            }
            
            VisualElement potentialSearchResults = editorListVisual.hierarchy.ElementAt(insertIndex);
            return potentialSearchResults != null && potentialSearchResults.name == SearchResultsName;
        }
        
        private bool HasSearchResults() {
            return searchResults != null && searchResults.Count > 0;
        }

        private bool OnlyHasTransform() {
#if UNITY_2022 || UNITY_2022_1_OR_NEWER
            return ((GameObject)targetObject).GetComponentCount() == 1;
#else
            return ((GameObject)targetObject).GetComponents<Component>().Length == 1;
#endif
        }

        private int GetRowCount(float rowWidth, float[] buttonWidths) {
            if (!ShowingVerticalScrollBar()) {
                rowWidth -= InspectorScrollBarWidth;
            }
            
            int rowCount = 1;
            float currentWidth = rowWidth;

            foreach (float buttonWidth in buttonWidths) {
                if (currentWidth < buttonWidth) {
                    currentWidth = rowWidth;
                    rowCount++;
                }
                currentWidth -= buttonWidth;
            }

            return rowCount;
        }

        private float[] GetButtonWidths(List<Component> components) {
            float[] buttonWidths = new float[components.Count + 1];
            buttonWidths[0] = GetButtonWidth(AllButtonName);
            for (int i = 1; i < buttonWidths.Length; i++) {
                buttonWidths[i] = GetButtonWidth(components[i - 1].GetType().Name);
            }
            return buttonWidths;
        }
        
        private float GetButtonWidth(string text) {
            float totalPadding = BoldLabelStyle.margin.right * 2f;
            Vector2 guiSize = BoldLabelStyle.CalcSize(new GUIContent(text));
            return iconSize.x + guiSize.x + totalPadding;
        }
        
        private List<SerializedProperty> GetComponentFields(SerializedObject serializedComponent) {
            SerializedProperty iter = serializedComponent.GetIterator();

            if (iter == null || !iter.NextVisible(true)) {
                return null;
            }

            List<SerializedProperty> fields = new List<SerializedProperty>();
            
            do {
                fields.Add(iter.Copy());
            }
            while (iter.NextVisible(false));
            
            return fields;
        }
        
        private Rect CenterRectVertically(Rect parent, Rect child) {
            float yDiff = parent.height - child.height;
            float yPos = parent.position.y + (yDiff / 2f);
            child.position = new Vector2(child.position.x, yPos);
            return child;
        }

        private Rect CenterRectHorizontally(Rect parent, Rect child) {
            float xDiff = parent.width - child.width;
            float xPos = parent.position.x + (xDiff / 2f);
            child.position = new Vector2(xPos, child.position.y);
            return child;
        }
        
        private void SetMargin(IStyle style, float margin) {
            style.marginTop = margin;
            style.marginBottom = margin;
            style.marginLeft = margin;
            style.marginRight = margin;
        }
        
        private bool ShowingVerticalScrollBar() {
            return inspectorScrollView.verticalScroller.resolvedStyle.display == DisplayStyle.Flex;
        }
        
        private List<Component> GetAllVisibleComponents() {
            if (!IsTargetObjectValid()) {
                return null;
            }

            GameObject selectedGameObject = targetObject as GameObject;
            
            if (Selection.gameObjects.Length == 1) {
                return GetAllVisibleComponents(selectedGameObject);
            }

            { // Get all visible components that each selected object shares
                List<Component> components = GetAllVisibleComponents(selectedGameObject);

                if (IsInspectorLocked()) {
                    return components;
                }

                foreach (GameObject otherGameObject in Selection.gameObjects) {
                    if (otherGameObject == selectedGameObject) continue;

                    List<Component> otherComponents = GetAllVisibleComponents(otherGameObject);

                    for (int i = components.Count - 1; i >= 0; i--) {
                        if (!ComponentListContainsType(otherComponents, components[i].GetType())) {
                            components.RemoveAt(i);
                        }
                    }
                }
                
                return components;
            }
        }
        
        private bool ComponentListContainsType(List<Component> list, Type componentType) {
            foreach (Component component in list) {
                if (component.GetType() == componentType) {
                    return true;
                }
            }
            return false;
        }

        private List<Component> GetAllVisibleComponents(GameObject gameObject) {
            Component[] components = gameObject.GetComponents<Component>();
            List<Component> result = new List<Component>(components.Length);
            foreach (Component component in components) {
                if (ComponentIsVisible(component)) {
                    result.Add(component);
                }
            }
            return result;
        }

        private bool ComponentIsVisible(Component component) {
            // Component can be null if the associated script cannot be loaded
            return component && !component.hideFlags.HasFlag(HideFlags.HideInInspector) && !ComponentIsOnBanList(component);
        }

        private bool ComponentIsOnBanList(Component component) {
            return component is ParticleSystemRenderer;
        }

        private int ComponentIdFromIndex(int index) {
            return componentFromIndex[index].GetInstanceID();
        }

        private Component ComponentFromId(int componentId) {
            int index = 0;
            for (int i = 0; i < validComponentIds.Count; i++) {
                if (validComponentIds[i] == componentId) {
                    index = i;
                }
            }
            return componentFromIndex[index];
        }

        private bool AllIsSelected() {
            return selectedComponentIds.Count == 0;
        }
        
        private bool WasJustUnlocked() {
            bool currentlyLocked = IsInspectorLocked();
            bool result = inspectorWasLocked && !currentlyLocked;
            inspectorWasLocked = currentlyLocked;
            return result;
        }

        private int GetMainContainerIndex() {
            return targetAssetType is AssetType.ProjectPrefab ? 2 : 1;
        }

        private int GetSearchResultsIndex() {
            return targetAssetType is AssetType.ProjectPrefab ? 3 : 2;
        }

        private int GetComponentStartIndex() {
            return targetAssetType == AssetType.ProjectPrefab ? 3 : 2;
        }

        private void RemoveSearchGUI() {
            if (IsShowingSearchResults()) {
                editorListVisual.RemoveAt(GetSearchResultsIndex());
                searchResultsGuiContainer = null;
            }
        }

        private bool HasTextInSearchField() {
            return !string.IsNullOrWhiteSpace(PersistentData.GetSearchString(targetObject));
        }

        private float CalculateMainContainerHeight() {
            float searchBarAndPadding = SearchBarHeight + SearchComponentListSpace;
            
            if (EBInspectorSettings.TransformOnlyKeepCopyPaste && OnlyHasTransform()) {
                return SearchBarHeight;
            }
            
            float[] buttonWidths = GetButtonWidths(GetAllVisibleComponents());
            
            // Important! Use editor list width as container width as mainGuiContainer.layout
            // is not always as up to date as it should be (if it were just created).
            // This prevents the container from flickering when changing objects.
            float guiContainerWidth = editorListVisual.layout.width - ContainerMargin * 2f;
            float rowCount = Mathf.Clamp(GetRowCount(guiContainerWidth, buttonWidths), 1, EBInspectorSettings.MaxNumberOfRows);
            return (rowCount * RowHeight) + (EBInspectorSettings.HideToolbar ? 0f : searchBarAndPadding);
        }
        
        private StyleLength FullLength() {
            return new StyleLength(StyleKeyword.Auto);
        }
        
        private bool IsTargetObjectValid() {
            return targetObject && targetObject is GameObject && targetAssetType != AssetType.NotImportant;
        }
        
        // Add all visual elements to the noMultiEditVisualElements set so we know which components are not
        // being displayed in the inspector when multi-inspecting is occurring.
        // During multi-inspecting the editor list may have non-shared (hidden) components inserted as children 
        // that we need to skip over when updating component visibility to not throw off component indexing.
        // Any visual element after no-multi-edit warning tells us what is being hidden in the inspector.
        private void RefreshNoMultiInspectVisualsSet() {
            noMultiEditVisualElements.Clear();

            if (Selection.gameObjects.Length <= 1 || editorListVisual == null) return;
            
            int noMultiEditIndex = editorListVisual.childCount;

            for (int i = 0; i < editorListVisual.childCount; i++) {
                if (editorListVisual[i].ClassListContains(InspectorNoMultiEditClassName)) {
                    noMultiEditIndex = i;
                    break;
                }
            }
                
            for (int i = noMultiEditIndex + 1; i < editorListVisual.childCount; i++) {
                noMultiEditVisualElements.Add(editorListVisual[i].name);
            }
        }

        private void ShowContextMenu(List<Component> components, List<Rect> buttonRects) {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy Selection"), false, CopySelectedToClipboard);
            menu.AddItem(new GUIContent("Paste Clipboard"), false, PasteFromClipboard);
            
            Component componentUnderCursor = null;
            for (int i = 1; i < buttonRects.Count; i++) {
                if (buttonRects[i].Contains(Event.current.mousePosition + mainScrollPosition)) {
                    componentUnderCursor = components[i - 1];
                    break;
                }
            }

            if (componentUnderCursor) {
                menu.AddSeparator("");
                string componentName = componentUnderCursor.GetType().Name;
                
                // Copy component
                menu.AddItem(new GUIContent($"Copy { componentName }"), false, () => {
                    PersistentData.Clipboard.CopyComponents(new List<Component>{ componentUnderCursor });
                });
                
                // Open component as script
                if (componentUnderCursor is MonoBehaviour) {
                    menu.AddItem(new GUIContent($"Edit { componentName } Script"), false, () => {
                        MonoScript script = MonoScript.FromMonoBehaviour(componentUnderCursor as MonoBehaviour);
                        if (script) AssetDatabase.OpenAsset(script);
                    });
                }

                // Remove component
                if (!(componentUnderCursor is Transform)) {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent($"Remove { componentName }"), false, () => {
                        RemoveComponentTypeFromSelection(componentUnderCursor.GetType());
                    });
                }
            }
            
            menu.ShowAsContext();
        }

        private void RemoveComponentTypeFromSelection(Type componentType) {
            GroupUndoAction("Remove Component", () => {
                foreach (GameObject gameObject in Selection.gameObjects) {
                    if (gameObject.TryGetComponent(componentType, out Component component)) {
                        Undo.DestroyObjectImmediate(component);
                    }
                }
            });
        }

        private void CopySelectedToClipboard() {
            PersistentData.Clipboard.CopyComponents(GetComponentsFromSelection());
        }

        private void PasteFromClipboard() {
            if (IsInspectorLocked()) {
                (targetObject as GameObject).PasteComponents(PersistentData.Clipboard.Copies);
                return;
            }
            
            foreach (GameObject gameObject in Selection.gameObjects) {
                gameObject.PasteComponents(PersistentData.Clipboard.Copies);
            }
        }

        private Rect ShiftRectStartVertically(Rect rect, float length) { 
            rect.position += new Vector2(0f, length);
            rect.height -= length;
            return rect;
        }
        
        private void Fix2021EditorMargins() {
            bool ShowingTransform() {
                if (!IsTargetObjectValid()) {
                    return false;
                }

                int componentStartIndex = GetComponentStartIndex();
                if (editorListVisual.childCount <= componentStartIndex) {
                    return false;
                }
                
                return editorListVisual[componentStartIndex].style.display ==  DisplayStyle.Flex;
            }
            
            if (ShowingTransform()) {
                const float transformHeaderMissingHeight = 7f;
                mainGuiContainer.style.marginTop = 0f;
                mainGuiContainer.style.marginBottom = transformHeaderMissingHeight + ContainerMargin;
            }
            else {
                SetMargin(mainGuiContainer.style, ContainerMargin);
                mainGuiContainer.style.marginTop = 0f;
            }
        }
    }
}
#endif

