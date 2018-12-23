﻿using UnityEngine;
using Planetaria;

namespace DebrisNoirs
{
    /// <summary>
    /// Small debris collides with medium and small debris.
    /// It is almost as fast as medium debris, smaller than the player, and predictable
    /// (although it could collide with another rock just before hitting the player).
    /// It has a cool-down (see TemperatureCooling) where it cannot collide with other rocks.
    /// </summary>
    public class SmallDebris : PlanetariaMonoBehaviour
    {
        protected override void OnConstruction() { }
        protected override void OnDestruction() { }

        private void OnValidate()
        {
            planetaria_collider = this.GetComponent<PlanetariaCollider>();
            planetaria_rigidbody = this.GetComponent<PlanetariaRigidbody>();
            planetaria_transform = this.GetComponent<PlanetariaTransform>();
            planetaria_renderer_foreground = this.GetComponent<AreaRenderer>();
            planetaria_renderer_background = planetaria_transform.Find("Silhouette").GetComponent<AreaRenderer>();
        }

        private void Start()
        {
            planetaria_renderer_foreground.angle = UnityEngine.Random.Range(0, 2 * Mathf.PI); // this is random rotation for the sprite image.
            planetaria_renderer_background.angle = planetaria_renderer_foreground.angle; // and the silhouette

            // apply random rotation
            float deviation_angle = UnityEngine.Random.Range(-Mathf.PI / 4, +Mathf.PI / 4);
            Vector2 local_direction = new Vector2(Mathf.Sin(deviation_angle), Mathf.Cos(deviation_angle));
            Vector3 direction = this.gameObject.internal_game_object.transform.rotation * local_direction;
            planetaria_transform.direction = direction;

            // apply random speed multiplier
            float speed_multiplier = UnityEngine.Random.Range(0.75f, 1);
            speed *= speed_multiplier;

            // set velocity
            planetaria_rigidbody.relative_velocity = new Vector2(0, speed);

            OnFieldEnter.data = on_field_enter;
        }

        public void on_field_enter(PlanetariaCollider collider)
        {
            DebrisNoirs.request_death(this.gameObject, DebrisNoirs.DebrisSize.Small);
            PlanetariaGameObject.Destroy(this.gameObject);
        }

        [SerializeField] [HideInInspector] private AreaRenderer planetaria_renderer_foreground;
        [SerializeField] [HideInInspector] private AreaRenderer planetaria_renderer_background;
        [SerializeField] [HideInInspector] private PlanetariaCollider planetaria_collider;
        [SerializeField] [HideInInspector] private PlanetariaRigidbody planetaria_rigidbody;
        [SerializeField] [HideInInspector] private PlanetariaTransform planetaria_transform;

        [SerializeField] public float speed = 1;
    }
}

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.