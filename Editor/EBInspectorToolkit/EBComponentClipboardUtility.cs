#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace EBUniEditor.Editor.Inspector {

    public static class EBComponentClipboardUtility {

        private static readonly HashSet<Type> singleInstanceComponents = new HashSet<Type> {
            typeof(Transform), typeof(RectTransform), typeof(Rigidbody), typeof(Rigidbody2D),
            typeof(Collider), typeof(Collider2D), typeof(CharacterController), typeof(Animator),
            typeof(Animation), typeof(Camera), typeof(AudioSource), typeof(AudioListener),
            typeof(Light), typeof(MeshFilter), typeof(MeshRenderer), typeof(SkinnedMeshRenderer),
            typeof(SpriteRenderer), typeof(Canvas), typeof(CanvasRenderer), typeof(NavMeshAgent), 
            typeof(NavMeshObstacle)
        };

        public static void PasteComponentsFromEmpty(this GameObject gameObject, List<Component> components) {
            foreach (Component referenceComponent in components) {
                Type componentType = referenceComponent.GetType();

                if (componentType == typeof(Transform)) {
                    gameObject.GetComponent<Transform>().CopyFields(referenceComponent);
                    continue;
                }

                if (CanOnlyHaveOneInstance(componentType) && gameObject.TryGetComponent(componentType, out Component _)) {
                    continue;
                }

                Component newComponent = Undo.AddComponent(gameObject, componentType);
                newComponent.CopyFields(referenceComponent);
            }
        }

        public static void PasteComponents(this GameObject gameObject, List<Component> components) {
            foreach (Component referenceComponent in components) {
                gameObject.PasteComponent(referenceComponent.GetType(), new SerializedObject(referenceComponent));
            }
        }
        
        public static void PasteComponents(this GameObject gameObject, List<EBComponentCopy> componentCopies) {
            foreach (EBComponentCopy componentCopy in componentCopies) {
                gameObject.PasteComponent(componentCopy.ComponentType, componentCopy.SerializedObject);
            }
        }

        private static void PasteComponent(this GameObject gameObject, Type componentType, SerializedObject serializedReference) {
            bool hasComponent = gameObject.TryGetComponent(componentType, out Component existingComponent);
            
            if (hasComponent) {
                existingComponent.CopyFields(serializedReference);
                return;
            }
            
            Undo.AddComponent(gameObject, componentType).CopyFields(serializedReference);
        }

        private static bool CanOnlyHaveOneInstance(Type componentType) {
            if (singleInstanceComponents.Contains(componentType)) {
                return true;
            }
            return componentType.GetCustomAttribute<DisallowMultipleComponent>() != null;
        }
        
        private static void CopyFields(this Component target, SerializedObject serializedReference) {
            SerializedObject serializedTarget = new SerializedObject(target);
            
            SerializedProperty property = serializedReference.GetIterator();
            if (property.NextVisible(true)) {
                do {
                    serializedTarget.CopyFromSerializedProperty(property);
                }
                while (property.NextVisible(false));
            }

            serializedTarget.ApplyModifiedProperties();
        }

        private static void CopyFields(this Component target, Component reference) {
            target.CopyFields(new SerializedObject(reference));
        }
    }
}

#endif

