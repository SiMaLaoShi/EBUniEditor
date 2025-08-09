#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace EBUniEditor.Editor.Inspector {

    public class EBComponentCopy {

        public SerializedObject SerializedObject;
        public Type ComponentType;

        public EBComponentCopy(Component component) {
            SerializedObject = new SerializedObject(component);
            ComponentType = component.GetType();
        }
    }
}

#endif