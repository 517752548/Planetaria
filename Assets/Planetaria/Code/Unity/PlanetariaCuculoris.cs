﻿using System;
using UnityEngine;

namespace Planetaria
{
    /// <summary>
	/// Similar to UnityEngine.Cookie, this is an overlay across a light region, but instead of being 2-dimensional, it is 1-dimensional.
    /// </summary>
    [Serializable]
	public class PlanetariaCuculoris // FIXME: HACK: only working with read/write enabled textures (I don't know how to fix it yet)
	{
        public enum ColorLerpMode { Clamp, Interpolate }

		// Properties (Public)

        /// <summary>A 1-dimensional Texture (only the full width of the first row used). The texture represents the 360 degrees or sector angle's pixel colors. The center pixel(s) (i.e. width/2) represents forward relative to the light.</summary>
        public Texture2D cuculoris
        {
            get
            {
                return user_cuculoris;
            }
            set
            {
                user_cuculoris = value;
            }
        }
		
		// Methods (Public)

        public void apply_to(ref Texture2D lightmap, float total_angle = 2*Mathf.PI, ColorLerpMode color_mode = ColorLerpMode.Interpolate)
        {
            cache();            
            Color[] original_pixels = lightmap.GetPixels();
            Color[] replacement_pixels = new Color[lightmap.width*lightmap.height];
            int pixel = 0;
            Vector2 center = new Vector2((lightmap.width-1)/2, (lightmap.height-1)/2);

            // go through the entire texture (all pixels)
            for (int row = 0; row < lightmap.width; ++row)
            {
                for (int column = 0; column < lightmap.height; ++column)
                {
                    // find the angle of the pixel to determine how the cuculoris will be applied
                    Vector2 relative_position = new Vector2(column, row) - center; // remember columns are x and rows are y
                    float angle = 0;
                    if (relative_position != Vector2.zero)
                    {
                        angle = Mathf.Atan2(relative_position.x, relative_position.y); // x/y inverted because the angle is relative to "forward" not "right"
                    }
                    // fetch closest user_cuculoris pixel (with special considerations for sector light) and multiply by original pixel
                    replacement_pixels[pixel] = original_pixels[pixel] * get_color(angle, total_angle, color_mode);
                    pixel += 1;
                }
            }
            lightmap.SetPixels(replacement_pixels);
            lightmap.Apply();
        }

        // Methods (non-Public)

        private void cache()
        {
            pixels = user_cuculoris.GetPixels32();
            // CONSIDER: I don't think the RGB components matter here (no need to reset them to zero).
        }

        private int get_closest_pixel_index(float angle, float total_angle = 2*Mathf.PI)
        {
            if (Mathf.Abs(angle) > total_angle/2)
            {
                Debug.LogError("Interface misuse: Assert: |angle| <= total_angle/2");
            }
            float ratio = 0.5f + angle/total_angle;
            int pixel_index = Mathf.FloorToInt(ratio * user_cuculoris.width);
            return Mathf.Clamp(pixel_index, 0, user_cuculoris.width-1);
        }

        private float get_pixel_angle(int pixel_index, float total_angle = 2*Mathf.PI)
        {
            float pixel_center = 0.5f + pixel_index;
            float ratio = pixel_center/user_cuculoris.width;
            return (ratio - 0.5f)*total_angle;
        }

        private Color32 get_color(float angle, float total_angle, ColorLerpMode color_mode)
        {
            if (Mathf.Abs(angle) > total_angle/2) // for sector lights when the angle is outside of the light's field of view
            {
                return Color.clear;
            }
            int pixel_index = get_closest_pixel_index(angle);
            if (color_mode == ColorLerpMode.Clamp) // return the nearest pixel
            {
                return pixels[pixel_index];
            }
            float pixel_angle = get_pixel_angle(pixel_index);
            float relative_angle = Mathf.DeltaAngle(pixel_angle, angle);
            int adjacent_pixel_direction = (int) Mathf.Sign(relative_angle);
            int pixel_index2 = pixel_index + adjacent_pixel_direction; // find the next closest pixel (may be unwrapped i.e. -1 or user_cuculoris.width)
            if (pixel_index2 <= -1) // outside left boundary (before user_cuculoris's leftmost pixel)
            {
                if (total_angle != 2*Mathf.PI) // sector light when there is no adjacent pixel (aside from Color.clear)
                {
                    return pixels[pixel_index]; // Clamp pixel, instead of interpolating with Color.clear
                }
                pixel_index2 = user_cuculoris.width-1;
            }
            else if (pixel_index2 >= user_cuculoris.width) // outside right boundary (after user_cuculoris's rightmost pixel)
            {
                if (total_angle != 2*Mathf.PI) // sector light when there is no adjacent pixel (aside from Color.clear)
                {
                    return pixels[pixel_index]; // Clamp pixel, instead of interpolating with Color.clear
                }
                pixel_index2 = 0;
            }
            float interpolator = Mathf.Abs(relative_angle)/(total_angle/user_cuculoris.width); // interpolator in range [0, 0.5] // NOTE: there are two adjacent pixels so the interpolator is effectively [-.5, +.5] per each pixel
            return Color32.Lerp(pixels[pixel_index], pixels[pixel_index2], interpolator);
        }

		// Variables (Public)
		
        [SerializeField] public Texture2D user_cuculoris; // FIXME: OnValidate() // this is a functionally-1D (width only of 1st row)
        
        // Variables (non-Public)
		
        [NonSerialized] private Color32[] pixels;
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