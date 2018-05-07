﻿using System.Linq;
using UnityEngine;

namespace Planetaria
{
    // TODO: Multiton Design Pattern for multiple levels?
    public static class PlanetariaCache
    {
        public static void cache(Block block)
        {
            if (Application.isPlaying)
            {
                foreach (optional<Arc> arc in block.iterator())
                {
                    if (arc.exists)
                    {
                        GameObject game_object = new GameObject("Planetaria Collider");
                        game_object.transform.parent = block.gameObject.transform;
                        game_object.hideFlags = HideFlags.DontSave;

                        PlanetariaCollider collider = game_object.AddComponent<PlanetariaCollider>();
                        SphereCollider sphere_collider = collider.get_sphere_collider();

                        optional<Transform> transformation = (block.is_dynamic ? block.gameObject.transform : null);
                        Sphere[] colliders = Sphere.arc_collider(transformation, arc.data);
                        collider.set_colliders(colliders);
                        collider.is_field = false;
                        collider.material = block.material;
                        sphere_collider.isTrigger = true;

                        PlanetariaCache.arc_cache.cache(sphere_collider, arc.data);
                        PlanetariaCache.block_cache.cache(sphere_collider, block);
                        PlanetariaCache.collider_cache.cache(sphere_collider, collider);
                    }
                }
            }
        }

        public static void cache(Field field)
        {
            GameObject game_object = new GameObject("Planetaria Collider");
            game_object.transform.parent = field.gameObject.transform;
            game_object.hideFlags = HideFlags.DontSave;

            PlanetariaCollider collider = game_object.AddComponent<PlanetariaCollider>();
            SphereCollider sphere_collider = collider.get_sphere_collider();

            optional<Transform> transformation = (field.is_dynamic ? field.gameObject.transform : null);

            Sphere[] colliders = new Sphere[Enumerable.Count(field.iterator())];
            collider.is_field = true;
            sphere_collider.isTrigger = true;
            GeospatialCurve last_curve = Enumerable.Last(field.iterator());
            int current_index = 0;
            foreach (GeospatialCurve curve in field.iterator())
            {
                Plane plane = Arc.curve(last_curve.point, last_curve.slope, curve.point).plane();
                Plane flipped = new Plane(-plane.normal, plane.distance); // FIXME: I think my code doesn't handle negative numbers for uniform_collider
                Debug.Log(plane.distance);
                colliders[current_index] = Sphere.uniform_collider(transformation, flipped);

                // prepare for next element
                ++current_index;
                last_curve = curve;
            }
            collider.set_colliders(colliders);
        }

        public static void uncache(Block block)
        {
            if (Application.isPlaying)
            {
                foreach (SphereCollider collider in block.gameObject.GetComponentsInChildren<SphereCollider>())
                {
                    PlanetariaCache.arc_cache.uncache(collider);
                    PlanetariaCache.block_cache.uncache(collider);
                    PlanetariaCache.collider_cache.uncache(collider);
                }
            }
        }

        public static void uncache(Field field)
        {
            if (Application.isPlaying)
            {
                optional<SphereCollider> collider = field.gameObject.GetComponent<SphereCollider>();
                if (collider.exists)
                {
                    PlanetariaCache.collider_cache.uncache(collider.data);
                }
            }
        }
    
        [System.NonSerialized] public static PlanetariaSubcache<SphereCollider, Arc> arc_cache = new PlanetariaSubcache<SphereCollider, Arc>();
        [System.NonSerialized] public static PlanetariaSubcache<SphereCollider, Block> block_cache = new PlanetariaSubcache<SphereCollider, Block>();
        [System.NonSerialized] public static PlanetariaSubcache<SphereCollider, PlanetariaCollider> collider_cache = new PlanetariaSubcache<SphereCollider, PlanetariaCollider>();
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