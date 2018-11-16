﻿using System;
using UnityEngine;

namespace Planetaria
{
    [Serializable]
    public struct SphericalRectangleUVCoordinates // similar to picture here: https://math.stackexchange.com/questions/1205927/how-to-calculate-the-area-covered-by-any-spherical-rectangle
    {
        public Vector2 uv
        {
            get { return uv_variable; }
        }

        public Rect canvas
        {
            get { return canvas_variable; }
        }

        public bool valid() // instead, create a UV clamp/wrap mechanism based on an input parameter e.g. UVMode.Clamp, UVMode.Wrap
        {
            return 0 <= uv.x && uv.x <= 1 &&
                    0 <= uv.y && uv.y <= 1;
        }

        /// <summary>
        /// Constructor - Stores the spherical rectangle's UV coordinates in a wrapper class.
        /// </summary>
        /// <param name="uv">The UV coordinates relative to the canvas center (0.5, 0.5). U/V Range: (-INF, +INF).</param>
        /// <param name="canvas">A Rect (measuring radians) representing the start and stop angles relative to Quaternion.identity. X/Y Range: (-2PI, +2PI).</param>
        public SphericalRectangleUVCoordinates(Vector2 uv, Rect canvas)
        {
            uv_variable = uv;
            canvas_variable = canvas;
        }

        /// <summary>
        /// Inspector - Converts spherical rectangle UV coordinates into normalized cartesian coordinates.
        /// </summary>
        /// <param name="uv">The coordinates in spherical rectangle UV space that will be converted.</param>
        /// <returns>The normalized cartesian coordinates.</returns>
        public static implicit operator NormalizedCartesianCoordinates(SphericalRectangleUVCoordinates uv)
        {
            return spherical_rectangle_to_cartesian(uv.uv_variable, uv.canvas_variable);
        }

        /// <summary>
        /// Inspector (Cache Mutator) - Creates a spherical rectangle UV coordinate set from a point on a unit sphere and a rectangle representing the x/y angles.
        /// </summary>
        /// <param name="cartesian">The point on the surface of a unit sphere to be converted.</param>
        /// <param name="canvas">A Rect (measuring radians) representing the start and stop angles relative to Quaternion.identity. X/Y Range: (-2PI, +2PI).</param>
        /// <returns>UV Coordinates for a spherical rectangle. Valid X/Y Range: [0, 1], although one axis may be in the (-INF, +INF) range.</returns>
        public static SphericalRectangleUVCoordinates cartesian_to_spherical_rectangle(Vector3 cartesian, Rect canvas)
        {
            // cache the canvas and derivative arcs (so they are not recomputed every time the function is called)
            cache_spherical_rectangle(canvas);

            // UV calculations are independent of one another with the focus (in theory but don't need to be) - also means Arcs may not ultimately be required (outside of initialization of cache)
            // find left/right hemisphere relative to canvas center
            // find upper/lower hemisphere relative to canvas center

            // compute Vector3.Angle(focus, cartesian) * Mathf.Deg2Rad
            // compute dot product of the respective Bearing.attractor() to find if the distance is towards or away from canvas center (in bounds or out of UV [0,1]) - away implies the angle is negated
            // fit (now positive or negative) angle to the interval angle_start -> angle_end as 0 -> 0.5 or 1 -> 0.5 depending on the left/right or upper/lower hemisphere.
            // return result

            float horizontal_ratio;
            if (Vector3.Dot(cartesian, cached_northern_hemisphere) >= 0)
            {
                Vector3 up = Bearing.attractor(cartesian, cached_upper_rail.center_axis);
                Arc intersector = ArcFactory.curve(cartesian, up, cartesian);
                optional<Vector3> intersection = PlanetariaIntersection.arc_arc_intersection(cached_upper_rail, intersector, 0);
                if (!intersection.exists) return new SphericalRectangleUVCoordinates(-Vector2.one, canvas); // FIXME:
                horizontal_ratio = (cached_upper_rail.position_to_angle(intersection.data) + cached_upper_rail.half_angle)/cached_upper_rail.angle();
            }
            else
            {
                Vector3 down = Bearing.attractor(cartesian, cached_lower_rail.center_axis);
                Arc intersector = ArcFactory.curve(cartesian, down, cartesian);
                optional<Vector3> intersection = PlanetariaIntersection.arc_arc_intersection(cached_lower_rail, intersector, 0);
                if (!intersection.exists) return new SphericalRectangleUVCoordinates(-Vector2.one, canvas); // FIXME:
                horizontal_ratio = (cached_lower_rail.position_to_angle(intersection.data) + cached_lower_rail.half_angle)/cached_lower_rail.angle();
            }
            
            Vector3 lower_point = cached_lower_rail.position(-cached_lower_rail.half_angle + horizontal_ratio*cached_lower_rail.angle());
            Vector3 middle_point = cached_middle_rail.position(-cached_middle_rail.half_angle + horizontal_ratio*cached_middle_rail.angle());
            Vector3 upper_point = cached_upper_rail.position(-cached_upper_rail.half_angle + horizontal_ratio*cached_upper_rail.angle());
            Arc vertical_rail = ArcFactory.curve(lower_point, middle_point, upper_point);
            ArcEditor.draw_simple_arc(vertical_rail);

            float vertical_ratio = (vertical_rail.position_to_angle(cartesian) + vertical_rail.half_angle)/vertical_rail.angle();

            float u = horizontal_ratio;
            float v = vertical_ratio;

            return new SphericalRectangleUVCoordinates(new Vector2(u, v), canvas);
        }

        /// <summary>
        /// Inspector (Cache Mutator) - Creates a cartesian point on the surface of a sphere coordinate set from a uv coordinate and a rectangle.
        /// </summary>
        /// <param name="uv">UV Coordinates for a spherical rectangle. Valid X/Y Range: [0, 1], although one axis may be in the (-INF, +INF) range.</param>
        /// <param name="canvas">A Rect (measuring radians) representing the start and stop angles relative to Quaternion.identity. X/Y Range: (-2PI, +2PI).</param>
        /// <returns>The cartesian point on the surface of a unit sphere the uv represents.</returns>
        public static NormalizedCartesianCoordinates spherical_rectangle_to_cartesian(Vector2 uv, Rect canvas) // I really can't figure this out
        {
            // cache the canvas and derivative arcs (so they are not recomputed every time the function is called)
            cache_spherical_rectangle(canvas);

            Vector3 lower_point = cached_lower_rail.position(-cached_lower_rail.half_angle + uv.x*cached_lower_rail.angle());
            Vector3 middle_point = cached_middle_rail.position(-cached_middle_rail.half_angle + uv.x*cached_middle_rail.angle());
            Vector3 upper_point = cached_upper_rail.position(-cached_upper_rail.half_angle + uv.x*cached_upper_rail.angle());
            Arc vertical_rail = ArcFactory.curve(lower_point, middle_point, upper_point);

            // I thought there might be a way to remove Arcs from the equation here, but I can't think of it.
            // Arc interpolation is a pretty effective way to compute the position from UVs.

            // The algorithm would probably have something to do with intersection(canvas.min, Vector2.Scale(canvas.size, uv)), but I thought I removed that for a reason (I thought I was using something similar at least).

            return new NormalizedCartesianCoordinates(vertical_rail.position(-vertical_rail.half_angle + uv.y*vertical_rail.angle()));
        }

        
        /// <summary>
        /// Inspector (Cache Mutator) - Updates the cache so that spherical rectangle calculations avoid recomputing old values.
        /// </summary>
        /// <param name="canvas">A Rect (measuring radians) representing the start and stop angles relative to Quaternion.identity. X/Y Range: (-2PI, +2PI).</param>
        public static void cache_spherical_rectangle(Rect canvas)
        {
            if (cached_canvas != canvas)
            {
                Vector3 lower_left = intersection(canvas.xMin, canvas.yMin);
                Vector3 lower_center = intersection(canvas.center.x, canvas.yMin);
                Vector3 lower_right = intersection(canvas.xMax, canvas.yMin);

                // construct middle rail from point closest to latitude=0 [ensures the widest/longest arc is used as the centerpoint since 0 is the equator]
                float center_elevation = PlanetariaMath.median(canvas.yMin, 0, canvas.yMax);

                Vector3 middle_left = intersection(canvas.xMin, center_elevation);
                Vector3 middle_center = intersection(canvas.center.x, center_elevation);
                Vector3 middle_right = intersection(canvas.xMax, center_elevation);

                Vector3 upper_left = intersection(canvas.xMin, canvas.yMax);
                Vector3 upper_center = intersection(canvas.center.x, canvas.yMax);
                Vector3 upper_right = intersection(canvas.xMax, canvas.yMax);

                cached_lower_rail = ArcFactory.curve(lower_left, lower_center, lower_right);
                cached_middle_rail = ArcFactory.curve(middle_left, middle_center, middle_right);
                cached_upper_rail = ArcFactory.curve(upper_left, upper_center, upper_right);

                //Arc biangle_segment1 = ArcFactory.curve(upper_center, upper_left, -upper_center);
                //Arc biangle_segment2 = ArcFactory.curve(lower_center, lower_left, -lower_center);
                //cached_left_biangle_focus = PlanetariaIntersection.arc_arc_intersection(biangle_segment1, biangle_segment2).data;

                // similar for cached_right/upper/lower_biangle_focus

                // find cached_left_angle_start = Vector3.Angle(cached_left_biangle_focus, true middle_left (note median!)) * Mathf.Deg2Rad;
                // find cached_left_angle_end = Vector3.Angle(cached_left_biangle_focus, true middle_center (note median!)) * Mathf.Deg2Rad;

                // similar for cached_right/upper/lower_angle_start and end

                // find Bearing.attractor(cached_left_biangle_focus, true middle_left (note median!)) [used for a dot product to determine if the angle as applied for UV is +/-]
                
                // similar for right/upper/lower

                Vector3 canvas_center = intersection(canvas.center.x, canvas.center.y);
                cached_northern_hemisphere = Bearing.attractor(canvas_center, cached_upper_rail.position(0));

                ArcEditor.draw_simple_arc(cached_lower_rail);
                ArcEditor.draw_simple_arc(cached_middle_rail);
                ArcEditor.draw_simple_arc(cached_upper_rail);

                cached_canvas = canvas;
            }
        }

        private static Vector3 equator_longitude(float longitude)
        {
            Arc equator = ArcFactory.curve(Vector3.back, Vector3.left, Vector3.back); // The center of the arc is between begin and endpoint hence Vector3.back (not Vector3.forward) and Vector3.left (not Vector3.right)
            return equator.position(longitude);
        }

        private static Vector3 prime_meridian_latitude(float latitude)
        {
            Arc prime_meridian = ArcFactory.curve(Vector3.back, Vector3.down, Vector3.back); // same as equator_longitude comment
            return prime_meridian.position(latitude);
        }

        private static Vector3 intersection(float longitude, float latitude) // FIXME: Rect canvas
        {
            Arc x_boundary = ArcFactory.curve(Vector3.down, equator_longitude(longitude), Vector3.up); // full-circle
            Arc y_boundary = ArcFactory.curve(Vector3.left, prime_meridian_latitude(latitude), Vector3.left); // semi-circle
            Vector3 corner = PlanetariaIntersection.arc_arc_intersection(x_boundary, y_boundary, 0).data;
            return corner;
        }

        private static Arc boundary(Vector2 start_longitude_latitude, Vector2 end_longitude_latitude)
        {
            Vector3 first_corner = intersection(start_longitude_latitude.x, start_longitude_latitude.y);
            Vector3 end_corner = intersection(end_longitude_latitude.x, end_longitude_latitude.y);
            Vector2 middle_longitude_latitude = (start_longitude_latitude + end_longitude_latitude)/2;
            Vector3 axis = intersection(middle_longitude_latitude.x, middle_longitude_latitude.y);
            return ArcFactory.curve(first_corner, axis, end_corner);
        }

        // CONSIDER: re-add normalize() for scaling width/height? Clamp vs Wrap

        [SerializeField] private Vector2 uv_variable; // FIXME: UVCoordinates would be normalized - TODO: should probably refactor UVCoordinates
        [SerializeField] private Rect canvas_variable;

        // cache (to avoid recomputation every frame)
        private static Rect cached_canvas = new Rect(float.NaN, float.NaN, float.NaN, float.NaN); // will never equal any Rect (so data will always be re-cached)
        private static Vector3 cached_northern_hemisphere; // this should be relative to canvas center for new version (can't tell if it's a bug that it wasn't relative to middle rail.
        private static Arc cached_lower_rail; // TODO: Arcs will probably be unnecessary soon
        private static Arc cached_middle_rail; 
        private static Arc cached_upper_rail;
        private static Vector3 cached_left_biangle_focus;
        private static Vector3 cached_right_biangle_focus;
        private static Vector3 cached_lower_biangle_focus;
        private static Vector3 cached_upper_biangle_focus;
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