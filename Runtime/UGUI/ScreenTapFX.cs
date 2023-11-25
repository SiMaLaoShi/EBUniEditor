using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
#endif

namespace EBUniEditor.Runtime.UGUI
{
    public class ScreenTapFX : MonoBehaviour
    {

        private Vector2 point;
        public Canvas Canvas;
        public GameObject EffectEntity;
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                //将鼠标点击的屏幕坐标转换为UI坐标，最后一个输出参数为转换的点
                RectTransformUtility.ScreenPointToLocalPointInRectangle(Canvas.transform as RectTransform,
                    Input.mousePosition, Canvas.worldCamera, out point);
                var go = Instantiate(EffectEntity);
                go.GetComponent<RectTransform>().anchoredPosition = point;
#if UNITY_EDITOR
                EditorGUIUtility.PingObject(EventSystem.current.currentSelectedGameObject);          
#endif
            }
        }
    }
}