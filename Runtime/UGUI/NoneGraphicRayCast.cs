using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EBA.Runtime.UGUI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class NoneGraphicRayCast : Graphic
    {
        protected NoneGraphicRayCast()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}