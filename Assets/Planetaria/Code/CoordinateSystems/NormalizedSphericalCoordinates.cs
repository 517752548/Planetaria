﻿using UnityEngine;

public class NormalizedSphericalCoordinates
{
    public Vector2 data
    {
        get { return data_variable; }
        set { data_variable = value; normalize(); }
    }

    /// <summary>
    /// Constructor - Stores a set of spherical coordinates on a unit sphere (i.e. rho=1) in a wrapper class.
    /// </summary>
    /// <param name="elevation">The angle in radians between the negative y-axis and the vector in Cartesian space.</param>
    /// <param name="azimuth">The angle in radians between the positive x-axis and the vector in Cartian space measured counterclockwise around the y-axis (viewing angle is downward along y-axis).</param>
    public NormalizedSphericalCoordinates(float elevation, float azimuth)
    {
        data_variable = new Vector2(elevation, azimuth);
        normalize();
    }

    /// <summary>
    /// Inspector - Converts spherical coordinates into Cartesian coordinates.
    /// </summary>
    /// <param name="spherical">The spherical coordinates that will be converted</param>
    /// <returns>The Cartesian coordinates.</returns> 
    public static implicit operator NormalizedCartesianCoordinates(NormalizedSphericalCoordinates octahedral)
    {
        Vector3 cartesian = new Vector3();
        cartesian.x = -Mathf.Sin(octahedral.data.x) * Mathf.Cos(octahedral.data.y);
        cartesian.y = -Mathf.Cos(octahedral.data.x);
        cartesian.z = Mathf.Sin(octahedral.data.x) * Mathf.Sin(octahedral.data.y);
        return new NormalizedCartesianCoordinates(cartesian);
    }

    /// <summary>
    /// Inspector - Converts spherical coordinates into octahedral coordinates.
    /// </summary>
    /// <param name="spherical">The spherical coordinates that will be converted</param>
    /// <returns>The octahedral coordinates.</returns> 
    public static implicit operator NormalizedOctahedralCoordinates(NormalizedSphericalCoordinates spherical)
    {
        NormalizedCartesianCoordinates cartesian = spherical;
        return cartesian;
    }

    /// <summary>
    /// Inspector - Converts spherical coordinates into octahedron UV space.
    /// </summary>
    /// <param name="spherical">The spherical coordinates that will be converted</param>
    /// <returns>The UV coordinates for an octahedron.</returns> 
    public static implicit operator OctahedralUVCoordinates(NormalizedSphericalCoordinates spherical)
    {
        NormalizedCartesianCoordinates cartesian = spherical;
        return cartesian;
    }

    /// <summary>
    /// Mutator - Wrap elevation and azimuth so they are within [0, PI] and [0, 2*PI) respectively. 
    /// </summary>
    private void normalize()
    {
        if (data_variable.x < 0 || data_variable.x > Mathf.PI)
        {
            data_variable.x = Mathf.PingPong(data_variable.x, 2*Mathf.PI); //TODO: test that 1) Vector2 is properly assigned 2) PingPong works for negative numbers
            if (data_variable.x > Mathf.PI)
            {
                data_variable.x -= Mathf.PI;
                data_variable.y += Mathf.PI; // going through a pole changes the azimuth
            }
        }

        if (data_variable.y < 0 || data_variable.y >= 2*Mathf.PI)
        {
            data_variable.y = PlanetariaMath.modolo_using_euclidean_division(data_variable.y, 2*Mathf.PI);
        }
    }

    private Vector2 data_variable;
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