using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// A concrete subclass of the Unity UI `Graphic` class that just skips drawing.
/// Useful for providing a raycast target without actually drawing anything.
public class NonDrawingGraphic : Graphic
{
        public override void SetMaterialDirty() { return; }
        public override void SetVerticesDirty() { return; }

       
        protected override void OnFillVBO(List<UIVertex> vbo)
        {
                vbo.Clear();
                return;
        }
}