﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Planetaria
{
    /// <summary>
    /// A group of miscellaneous helper functions.
    /// </summary>
    public static class Miscellaneous
    {
        /// <summary>
        /// Counts the true booleans in a sequence. E.g. count_true_booleans(true, false, true) returns 2.
        /// </summary>
        /// <param name="boolean_list">A comma separated list of (or array of) booleans.</param>
        /// <returns>The number of true values in the sequence.</returns>
        public static int count_true_booleans(params bool[] boolean_list)
        {
           return boolean_list.Count(is_true => is_true);
        }

        public static optional<Texture2D> fetch_image(string image_file)
        {
            optional<Texture2D> texture = new optional<Texture2D>();
            if (File.Exists(image_file))
            {
                 byte[] raw_file_binary = File.ReadAllBytes(image_file);
                 texture = new Texture2D(0,0);
                 texture.data.LoadImage(raw_file_binary);
            }
            return texture;
        }

        /// <summary>
        /// Mutator - Gets the designated object from the root of the scene if it exists; otherwise it creates and returns it.
        /// </summary>
        /// <param name="name">The name of the object (without double leading underscores i.e. "__").</param>
        /// <param name="hidden_internal">Whether the GameObject should be hidden unless debugging.</param>
        /// <returns>The found or newly added object from the root of the scene.</returns>
        public static GameObject GetOrAddObject(string name, bool hidden_internal = true)
        {
            if (hidden_internal)
            {
                name = "__" + name;
            }
            optional<GameObject> game_object = GameObject.Find("/" + name);
            if (!game_object.exists)
            {
                game_object = new GameObject(name);
            }
            if (hidden_internal && !Global.self.show_inspector)
            {
                game_object.data.hideFlags = (HideFlags.HideInHierarchy | HideFlags.HideInInspector);
            }
            return game_object.data;
        }

        /// <summary>
        /// Mutator - Gets the attached child by its name if it exists; otherwise it adds and returns it.
        /// </summary>
        /// <param name="self">Calling object - extension method (implicit).</param>
        /// <param name="name">The name of the object (without double leading underscores i.e. "__").</param>
        /// <param name="hidden_internal">Whether the GameObject should be hidden unless debugging.</param>
        /// <returns>The found or newly added child with given name.</returns>
        public static GameObject GetOrAddChild(this Component self, string name, bool hidden_internal = true)
        {
            if (hidden_internal)
            {
                name = "__" + name;
            }
            optional<Transform> child = self.transform.Find(name);
            if (!child.exists)
            {
                GameObject child_object = new GameObject(name);
                child_object.transform.parent = self.transform;
                child = child_object.transform;
            }
            if (hidden_internal && Global.self.show_inspector)
            {
                Debug.Log("Before Hiding: HideFlags." + child.data.hideFlags);
                child.data.hideFlags = (HideFlags.HideInHierarchy | HideFlags.HideInInspector);
                Debug.Log("After Hiding: HideFlags." + child.data.hideFlags);
            }
            return child.data.gameObject;
        }

        /// <summary>
        /// Mutator - Gets the attached Component if it exists; otherwise it adds and returns it.
        /// </summary>
        /// <typeparam name="Subtype">The type of the Component to be fetched.</typeparam>
        /// <param name="self">Calling object - extension method (implicit).</param>
        /// <returns>The found or newly added Component.</returns>
        public static Subtype GetOrAddComponent<Subtype>(this Component self) where Subtype : Component
        {
            optional<Subtype> result = self.GetComponent<Subtype>();
            if (!result.exists)
            {
                result = self.gameObject.AddComponent<Subtype>();
            }
            return result.data;
        }

        /// <summary>
        /// Inspector - finds the last instance of inner_text between prefix and suffix, if it exists, and returns it.
        /// </summary>
        /// <param name="text">The text that will be searched.</param>
        /// <param name="prefix">The prefix before the desired text.</param>
        /// <param name="suffix">The suffix after the desired text.</param>
        /// <returns>The last instance of text between prefix and suffix (including neither prefix phrase nor suffix phrase).</returns>
        public static optional<string> inner_text(string text, string prefix, string suffix)
        {
            int end_index = text.LastIndexOf(suffix);
            if (end_index != -1)
            {
                int start_index = text.LastIndexOf(prefix, end_index-1);
                if (start_index != -1)
                {
                    start_index += prefix.Length;
                    int length = end_index - start_index; // off by one? start==0, end==1 (e.g. "A\0") has length (1-0) 
                    return text.Substring(start_index, length);
                }
            }
            return new optional<string>();
        }

        /// <summary>
        /// Inspector - Checks if bit is set in integer.
        /// </summary>
        /// <param name="value">The integer value to be read as binary.</param>
        /// <param name="bit_index">The index of the bit index (i.e. [0,31]).</param>
        /// <returns>True if bit is set; false otherwise.</returns>
        public static bool is_bit_set(int value, int bit_index)
        {
            return (value & (1 << bit_index)) != 0;
        }

        public static int power(int base_, int exponent)
        {
            if (exponent <= 0)
            {
                return 1;
            }
            if (is_bit_set(exponent, 0)) // odd number
            {
                return power(base_, exponent - 1);
            }
            return power(base_, exponent / 2) * power(base_, exponent / 2); // even
        }

        public static void serialize<Key, Value>(this Dictionary<Key, Value> dictionary, List<Key> keys, List<Value> values)
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<Key, Value> pair in dictionary)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public static void deserialize<Key, Value>(this Dictionary<Key, Value> dictionary, List<Key> keys, List<Value> values)
        {
            dictionary.Clear();
            for (int pair_index = 0; pair_index < keys.Count; ++pair_index)
            {
                dictionary[keys[pair_index]] = values[pair_index];
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Mutator (Operating System level) - Writes (or overwrites!) the file at location "file" with the string "text_contents". 
        /// </summary>
        /// <param name="file">The relative file path starting in the Unity directory (e.g. "Assets/important_file.txt"). Note: will be overwritten.</param>
        /// <param name="text_contents">The contents that will be placed in the file (overwrites file).</param>
        /// <param name="add_global_unique_identifier">Adds "_[0-9a-f]{32}" before file extension (last dot) in "file".</param>
        public static optional<TextAsset> write_file(string file, string text_contents, bool add_global_unique_identifier)
        {
            using (StreamWriter writer = new StreamWriter(file, false))
            {
                string suffix = "";
                writer.Write(text_contents);
                if (add_global_unique_identifier)
                {
                    writer.Dispose();
                    UnityEditor.AssetDatabase.Refresh();
                    string global_unique_identifier = text_contents.GetHashCode().ToString("X");
                    Debug.Log("Creating " + global_unique_identifier);
                    suffix = "_" + global_unique_identifier;
                    optional<string> name = Miscellaneous.inner_text(file, "/", ".");
                    if (name.exists)
                    {
                        UnityEditor.AssetDatabase.RenameAsset(file, name.data + suffix); // FIXME: INVESTIGATE: why does optional<string> + string work? (for safety reasons, it shouldn't)
                    }
                    else
                    {
                        Debug.Log("Critical Error");
                    }
                }
                UnityEditor.AssetDatabase.Refresh();
                optional<string> resource = Miscellaneous.inner_text(file, "/Resources/", ".");
                if (resource.exists)
                {
                    resource = resource.data + suffix;
                    return Resources.Load<TextAsset>(resource.data);
                }
                return new optional<TextAsset>();
            }
        }
#endif
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