﻿using UnityEngine;

public struct Planetarium
{
    public static Planetarium planetarium(int level_identifier)
    {
        Planetarium result = new Planetarium();

        result.level_index = level_identifier;

        return result;
    }

    public void load_room()
    {
        toggle_room(level_index, true);
    }

    public void unload_room()
    {
        toggle_room(level_index, false);
    }

    private static void toggle_room(int level_index, bool state)
    {
        GameObject geometry_root = GameObject.Find("/" + level_index.ToString()); // TODO: make this more elegant...
        GameObject graphics_root = GameObject.Find("/" + level_index.ToString() + "g");

        // geometry
        for (int geometry_index = 0; geometry_index < geometry_root.transform.childCount; geometry_index++)
        {
            geometry_root.transform.GetChild(geometry_index).gameObject.SetActive(state);
        }

        // graphics
        for (int graphics_index = 0; graphics_index < graphics_root.transform.childCount; graphics_index++)
        {
            graphics_root.transform.GetChild(graphics_index).gameObject.SetActive(state);
        }
    }

    private int level_index;
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