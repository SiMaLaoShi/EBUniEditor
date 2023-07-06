using System.Collections.Generic;
using EBA.Runtime.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace EBA.Runtime.UGUI
{
    public class UILayoutRebuildLogger : MonoBehaviour
    {
        IList<ICanvasElement> m_LayoutRebuildQueue = new List<ICanvasElement>();

        IList<ICanvasElement> m_GraphicRebuildQueue = new List<ICanvasElement>();

        private void Awake()
        {
            var o = CanvasUpdateRegistry.instance as object;
            m_LayoutRebuildQueue = o.GetFieldValue<IList<ICanvasElement>>("m_LayoutRebuildQueue");
            m_GraphicRebuildQueue = o.GetFieldValue<IList<ICanvasElement>>("m_GraphicRebuildQueue");
        }

        private void Update()
        {
            for (int j = 0; j < m_LayoutRebuildQueue.Count; j++)

            {
                var rebuild = m_LayoutRebuildQueue[j];

                if (ObjectValidForUpdate(rebuild))

                {
                    Debug.LogErrorFormat("{0}引起{1}网格重建", rebuild.transform.name,
                        rebuild.transform.GetComponent<Graphic>().canvas.name);
                }
            }


            for (int j = 0; j < m_GraphicRebuildQueue.Count; j++)

            {
                var element = m_GraphicRebuildQueue[j];

                if (ObjectValidForUpdate(element))

                {
                    Debug.LogErrorFormat("{0}引起{1}网格重建", element.transform.name,
                        element.transform.GetComponent<Graphic>().canvas.name);
                }
            }
        }

        private bool ObjectValidForUpdate(ICanvasElement element)
        {
            var valid = element != null;

            var isUnityObject = element is Object;

            if (isUnityObject)

                valid = (element as Object) !=
                        null; //Here we make use of the overloaded UnityEngine.Object == null, that checks if the native object is alive.
            return valid;
        }
    }
}