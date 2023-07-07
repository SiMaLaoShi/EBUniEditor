using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//注：unity3dUGUI空对象实现射线检测  
//参考 https://blog.csdn.net/weixin_33943347/article/details/89591903
namespace EBA.Runtime.UGUI
{
    public class NoDrawingRayCast : Graphic
    {
        protected NoDrawingRayCast()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}