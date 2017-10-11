﻿using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlanetariaMonoBehaviour))]
public abstract class PlanetariaActor : PlanetariaMonoBehaviour
{
    protected sealed override void Awake()
    {
        collision_map = new Dictionary<Block, BlockInteractor>();
        trigger_map = new Dictionary<Field, FieldInteractor>();
        on_existance();
    }

    protected sealed override void Start()
    {
        // add to collision_map and trigger_map for all objects currently intersecting (via Physics.OverlapBox())
        on_time_zero();
    }
    
    protected sealed override void FixedUpdate() // always calling FixedUpdate is less than ideal
    {
        on_every_frame(); // if undefined, this will error out
        transform.move();
    }

    protected sealed override void OnTriggerStay(Collider collider)
    {
        BoxCollider box_collider = collider as BoxCollider;
        if (!box_collider)
        {
            return;
        }

        optional<Arc> arc = PlanetariaCache.arc_cache.get(box_collider); // C++17 if statements are so pretty compared to this...
        if (arc.exists) // block
        {
            optional<Block> block = PlanetariaCache.block_cache.get(arc.data);
            if (!block.exists)
            {
                Debug.LogError("Critical Err0r.");
                return;
            }

            if (!collision_map.ContainsKey(block.data) && arc.data.contains(transform.position.data, transform.scale))
            {
                float half_height = transform.scale / 2;
                BlockInteractor collision = new BlockInteractor(arc.data, transform.previous_position.data, transform.position.data, half_height);
                collision_map.Add(block.data, collision);
                on_block_enter(collision);
            }
            else if (collision_map.ContainsKey(block.data) && !block.data.contains(transform.position.data, transform.scale))
            {
                BlockInteractor collision = collision_map[block.data];
                on_block_exit(collision);
                collision_map.Remove(block.data);
            }

            if (collision_map.ContainsKey(block.data))
            {
                BlockInteractor collision = collision_map[block.data];
                on_block_stay(collision);
            }
        }
        else // field
        {
            optional<Field> field = PlanetariaCache.zone_cache.get(box_collider);
            if (!field.exists)
            {
                Debug.LogError("This is likely an Err0r or setup issue.");
                return;
            }

            if (!trigger_map.ContainsKey(field.data) && arc.data.contains(transform.position.data, transform.scale))
            {
                float half_height = transform.scale / 2;
                FieldInteractor trigger = new FieldInteractor(field.data, half_height);
                trigger_map.Add(field.data, trigger);
                on_field_enter(trigger);
            }
            else if (trigger_map.ContainsKey(field.data) && !field.data.contains(transform.position.data, transform.scale))
            {
                FieldInteractor collision = trigger_map[field.data];
                on_field_exit(collision);
                trigger_map.Remove(field.data);
            }

            if (trigger_map.ContainsKey(field.data))
            {
                FieldInteractor collision = trigger_map[field.data];
                on_field_stay(collision);
            }
        }
    }

    protected sealed override void Update() { }
    protected sealed override void LateUpdate() { }

    protected sealed override void OnTriggerEnter(Collider collider) { }
    protected sealed override void OnTriggerExit(Collider collider) { } // To account for object deletion, this (or other code) must be defined.

    protected sealed override void OnCollisionEnter(Collision collision) { }
    protected sealed override void OnCollisionStay(Collision collision) { }
    protected sealed override void OnCollisionExit(Collision collision) { }

    private Dictionary<Block, BlockInteractor> collision_map;
    private Dictionary<Field, FieldInteractor> trigger_map;
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