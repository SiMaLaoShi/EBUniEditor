#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace EBUniEditor.Editor.Inspector {

    public class EBComponentClipboard {

        public readonly List<EBComponentCopy> Copies = new List<EBComponentCopy>();
        
        public void CopyComponents(List<Component> components) {
            if (components == null) return;
            
            Copies.Clear();
            foreach (var component in components) {
                Copies.Add(new EBComponentCopy(component));
            }
        }
    }
}
#endif