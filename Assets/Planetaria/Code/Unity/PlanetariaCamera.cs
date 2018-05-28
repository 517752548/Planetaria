﻿using System;
using UnityEngine;
using UnityEngine.XR;

namespace Planetaria
{
    [DisallowMultipleComponent]
    [Serializable]
    public class PlanetariaCamera : MonoBehaviour
    {
        private void Awake()
        {
            initialize();
            XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);
        }

        private void Reset()
        {
            initialize();
        }

        private void initialize()
        {
            if (transform == null)
            {
                transform = this.GetOrAddComponent<PlanetariaTransform>();
                GameObject dolly = this.GetOrAddChild("CameraDolly");
                GameObject camera = dolly.transform.GetOrAddChild("Camera");
                dolly_transform = dolly.GetComponent<Transform>();
                internal_camera = camera.transform.GetOrAddComponent<Camera>();
                camera.transform.GetOrAddComponent<AudioListener>();
            }
            internal_camera.useOcclusionCulling = false;
            dolly_transform.position = Vector3.forward * zoom;
            dolly_transform.localScale = Vector3.one; // CONSIDER: setting this to zero mirrors `XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);`
            internal_camera.nearClipPlane = near_clip_plane;
            internal_camera.farClipPlane = far_clip_plane;
        }

        public const float near_clip_plane = 0.0078125f;
        public const float far_clip_plane = 2.0f;

        [SerializeField] public float zoom = 0;
        [SerializeField] [HideInInspector] protected new PlanetariaTransform transform;
        [SerializeField] [HideInInspector] protected Transform dolly_transform;
        [SerializeField] [HideInInspector] protected Camera internal_camera;
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