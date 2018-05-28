﻿using System;
using UnityEngine;

namespace Planetaria
{
    [Serializable] // While this is one of my favorite types, I might have to forgo serializing it [I can still use it, just not between Unity sessions]
    public struct optional<Type> // https://stackoverflow.com/questions/16199227/optional-return-in-c-net
    {
        public bool exists
        {
            get
            {
                return exists_variable;
            }
        }

        public Type data
        {
            get
            {
                if (!exists)
                {
                    throw new NullReferenceException();
                }
                return data_variable;
            }
            set
            {
                exists_variable = !(value == null);
                data_variable = value;
            }
        }

        public optional(Type original) // FIXME: (?) This is far more expensive than it used to be
        {
            // Unity overrides the definition of nullity, so check for Unity-specific "nulls"
            exists_variable = original != null && !original.Equals(null); // 2nd half isn't redundant: // https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
            data_variable = original;
        }

        public static implicit operator optional<Type>(Type original)
        {
            return new optional<Type>(original);
        } 

        public static bool operator ==(optional<Type> left, optional<Type> right)
        {
            bool inequal_existance = (left.exists != right.exists);
            bool inequal_value = (left.exists && right.exists && left.data.Equals(right.data));
            bool inequal = inequal_existance || inequal_value;
            return !inequal;
        }

        public static bool operator !=(optional<Type> left, optional<Type> right)
        {
            return !(left == right);
        }

        public override bool Equals(System.Object other)
        {
            return other is optional<Type> && this == (optional<Type>) other;
        }

        public override int GetHashCode() 
        {
            return exists_variable.GetHashCode() ^ data_variable.GetHashCode();
        }


        public override string ToString()
        {
            if (!exists)
            {
                return "nonexistent " + typeof(Type).Name;
            }

            return data.ToString();
        }

        [SerializeField] private bool exists_variable;
        [SerializeField] private Type data_variable;
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