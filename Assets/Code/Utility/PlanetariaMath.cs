﻿using UnityEngine;

public static class PlanetariaMath
{
    /// <summary>
    /// The result of the modulo operation when using Euclidean division. The remainder will always be positive.
    /// </summary>
    /// <param name="dividend">The element that will be used to calculate the remainder.</param>
    /// <param name="divisor">The interval upon which the dividend will be divided.</param>
    /// <returns>The remainder.</returns>
    public static float EuclideanDivisionModulo(float dividend, float divisor)
    {
        divisor = Mathf.Abs(divisor);

        float quotient = dividend / divisor;

        float result = dividend - divisor*Mathf.Floor(quotient);

        if (result < 0f)
        {
            Debug.Log("ERROR: PlanetariaMath: EuclideanDivisionModulo(): " + result);
        }

        return result;
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