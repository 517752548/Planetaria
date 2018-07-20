﻿using System;
using UnityEditor;

namespace Planetaria
{
    [CustomEditor(typeof(ArcBuilder))]
    [Serializable]
    public class ArcBuilderEditor : Editor
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void draw_arc_builder_gizmos(ArcBuilder self, GizmoType gizmo_type)
        {
            foreach (Arc arc in self.debug_shape.arcs)
            {
                ArcEditor.draw_arc(arc);
            }
        }
    }
}

/*
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/