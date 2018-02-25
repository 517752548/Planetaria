﻿using UnityEngine;

namespace Planetaria
{
    public class PlanetariaRigidbody : MonoBehaviour
    {
        private void Awake()
        {
            transform = this.GetOrAddComponent<PlanetariaTransform>();
            internal_rigidbody = this.GetOrAddComponent<Rigidbody>();
            internal_rigidbody.isKinematic = true;
            internal_rigidbody.useGravity = false;
        }

        private void Start()
        {
            position = transform.position.data;
            get_acceleration();
        }

        private void FixedUpdate()
        {
            if (!collision.exists)
            {
                // I am making a bet this is relevant in spherical coordinates (it is Euler isn't it?): http://openarena.ws/board/index.php?topic=5100.0
                // Wikipedia: https://en.wikipedia.org/wiki/Leapfrog_integration
                // "This is especially useful when computing orbital dynamics, as many other integration schemes, such as the (order-4) Runge-Kutta method, do not conserve energy and allow the system to drift substantially over time." - Wikipedia
                // http://lolengine.net/blog/2011/12/14/understanding-motion-in-games
                // http://www.richardlord.net/presentations/physics-for-flash-games.html
                velocity = velocity + acceleration * (Time.deltaTime / 2);
                aerial_move(velocity.magnitude * Time.deltaTime);
                acceleration = get_acceleration();
            }
            else
            {
                grounded_track(Time.deltaTime/2);
                grounded_position();
                grounded_accelerate(Time.deltaTime);
            }

            if (!collision.exists)
            {
                velocity = velocity + acceleration * (Time.deltaTime / 2);
            }
            else
            {
                grounded_track(Time.deltaTime/2);
            }
        }

        public Vector3 get_acceleration()
        {
            Vector3 result = Vector3.zero; // in theory this could mess things up
            for (int force = 0; force < gravity_wells.Length; ++force)
            {
                result += Bearing.attractor(position, gravity_wells[force])*gravity_wells[force].magnitude;
            }
            return result;
        }

        public bool collide(BlockCollision collision, CollisionObserver observer)
        {
            aerial_move(-collision.overshoot); // this only (truly) works with perpendicular vectors?

            this.collision = collision;
            this.observer = observer;

            horizontal_velocity = Vector3.Dot(velocity, Bearing.right(position, collision.normal().data));
            vertical_velocity = Vector3.Dot(velocity, collision.normal().data);

            if (vertical_velocity < 0)
            {
                vertical_velocity *= -collision.elasticity;
            }

            grounded_accelerate(0);

            return this.collision.exists;
        }

        private void aerial_move(float delta)
        {
            Vector3 next_position = PlanetariaMath.slerp(position, velocity.normalized, delta); // Note: when velocity = Vector3.zero, it luckily still returns "position" intact.
            Vector3 next_velocity = PlanetariaMath.slerp(position, velocity.normalized, delta + Mathf.PI/2);
            
            transform.position = new NormalizedCartesianCoordinates(next_position);
            position = transform.position.data; // get normalized data
            velocity = next_velocity.normalized * velocity.magnitude;
            
            // TODO: occasionally ensure velocity and position are orthogonal
        }

        private void grounded_position()
        {
            collision.data.move(horizontal_velocity * Time.deltaTime, transform.scale/2);
            transform.position = collision.data.position();
            position = transform.position.data; // NOTE: required so get_acceleration() functions
            // project velocity
        }

        private void grounded_track(float delta_time)
        {
            horizontal_velocity += horizontal_acceleration * delta_time;
            float friction = collision.data.friction * vertical_acceleration * delta_time;
            float speed = Mathf.Abs(horizontal_velocity);
            //horizontal_velocity -= Mathf.Sign(horizontal_velocity)*Mathf.Min(speed, friction);
        }

        private void grounded_accelerate(float delta)
        {
            Vector3 normal = collision.data.normal().data;
            Vector3 right = Bearing.right(collision.data.position().data, normal);

            acceleration = get_acceleration();
            horizontal_acceleration = Vector3.Dot(acceleration, right);
            vertical_acceleration = Vector3.Dot(acceleration, normal) - collision.data.magnetism;
            if (!collision.data.grounded(internal_velocity)) // TODO: check centripedal force
            {
                derail(0, vertical_acceleration*delta); // Force OnCollisionExit, "un-collision" (and accelerate for a frame)
            }
        }
        
        public void derail(float x_velocity, float y_velocity)
        {
            if (collision.exists && observer.exists)
            {
                collision.data.move(0, transform.scale/2 * (1 + 1e-3f)); // extrude the player so they do not accidentally re-collide (immediately) // FIXME: magic number, move to Precision.*
                x_velocity += horizontal_velocity;
                //y_velocity += vertical_velocity;
                position = collision.data.position().data;
                transform.position = new NormalizedCartesianCoordinates(position);
                Vector3 normal = collision.data.normal().data;
                Vector3 right = Bearing.right(position, normal);
                velocity = right*x_velocity + normal*y_velocity;
                Debug.DrawRay(position, velocity, Color.yellow, 1f);
                acceleration = get_acceleration();
                // TODO: accelerate vertically
                observer.data.exit_block(collision.data);
                collision = new optional<BlockCollision>();
                observer = new optional<CollisionObserver>();
            }
        }

        private void synchronize_velocity_ground_to_air()
        {
            if (collision.exists)
            {
                Vector3 x = horizontal_velocity * Bearing.right(position, collision.data.normal().data);
                Vector3 y = vertical_acceleration * Time.deltaTime * collision.data.normal().data;
                velocity = x + y;
            }
        }

        private void synchronize_velocity_air_to_ground()
        {
            if (collision.exists)
            {
                horizontal_velocity = Vector3.Dot(velocity, Bearing.right(position, collision.data.normal().data));
                vertical_velocity = Vector3.Dot(velocity, collision.data.normal().data);
            }
        }

        public Vector2 relative_velocity
        {
            get
            {
                synchronize_velocity_ground_to_air();
                Vector3 right = Bearing.normal(position, transform.rotation);
                Vector3 up = Bearing.normal(position, transform.rotation + Mathf.PI/2);
                float x = Vector3.Dot(velocity, right);
                float y = Vector3.Dot(velocity, up);
                return new Vector2(x, y);
            }
            set
            {
                Vector3 x = Bearing.normal(position, transform.rotation) * value.x;
                Vector3 y = Bearing.normal(position, transform.rotation + Mathf.PI/2) * value.y;
                velocity = x + y;
                synchronize_velocity_air_to_ground();
            }
        }

        public Vector2 absolute_velocity
        {
            get
            {
                synchronize_velocity_ground_to_air();
                float x = Vector3.Dot(velocity, Bearing.east(position));
                float y = Vector3.Dot(velocity, Bearing.north(position));
                return new Vector2(x, y);
            }
            set
            {
                Vector3 x = Bearing.east(position) * value.x;
                Vector3 y = Bearing.north(position) * value.y;
                velocity = x + y;
                synchronize_velocity_air_to_ground();
            }
        }

        private Vector3 internal_velocity
        {
            get
            {
                synchronize_velocity_ground_to_air();
                return velocity;
            }
        }

        // position
        private Vector3 position; // magnitude = 1
        private Vector3 velocity; // magnitude in [0, infinity]
        private Vector3 acceleration; // magnitude in [0, infinity]

        /// <summary>
        /// public Vector2 velocity - set velocity based on absolute values; up is north, right is east
        /// public Vector3 position - set velocity based on relative values; begin attractor is south, end repeller is north
        /// </summary>

        // gravity
        [SerializeField] public Vector3[] gravity_wells;

        private new PlanetariaTransform transform;
        private optional<CollisionObserver> observer;
        private Rigidbody internal_rigidbody;
        
        private optional<BlockCollision> collision;
        private float horizontal_velocity;
        private float vertical_velocity;
        private float horizontal_acceleration;
        private float vertical_acceleration;
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