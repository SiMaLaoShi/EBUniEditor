using System.Collections.Generic;
using UnityEngine;
namespace EBA.Ebunieditor.Editor.GlyphEditor
{
    public class GlyphInfoScriptableObject : ScriptableObject
    {
        public string FontName = "Default_Font";
        public List<Glyph> GlyphInfos = new List<Glyph>();
    }
}