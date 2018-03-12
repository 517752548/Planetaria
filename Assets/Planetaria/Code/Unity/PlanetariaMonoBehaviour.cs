﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Planetaria
{
    public abstract class PlanetariaMonoBehaviour : MonoBehaviour
    {
        protected abstract void OnConstruction();
        protected abstract void OnDestruction();

        protected delegate void CollisionDelegate(BlockCollision block_information);
        protected delegate void TriggerDelegate(PlanetariaCollider field_information);

        protected optional<CollisionDelegate> OnBlockEnter = null;
        protected optional<CollisionDelegate> OnBlockExit = null;
        protected optional<CollisionDelegate> OnBlockStay = null;
    
        protected optional<TriggerDelegate> OnFieldEnter = null;
        protected optional<TriggerDelegate> OnFieldExit = null;
        protected optional<TriggerDelegate> OnFieldStay = null;

        private IEnumerator wait_for_fixed_update()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                
                Debug.Log("Of/c #1");
                foreach (CollisionObserver observer in observers)
                {
                    observer.notify();
                }
                if (OnFieldStay.exists)
                {
                    foreach (CollisionObserver observer in observers)
                    {
                        foreach (PlanetariaCollider field in observer.fields())
                        {
                            OnFieldStay.data(field);
                        }
                    }
                }
                if (OnBlockStay.exists)
                {
                    Debug.Log("Of/c #2");
                    foreach (CollisionObserver observer in observers)
                    {
                        Debug.Log("Of/c #3");
                        foreach (BlockCollision collision in observer.collisions())
                        {
                            Debug.Log("Of/c #4 size is " + observer.collisions().Count);
                            OnBlockStay.data(collision);
                        }
                    }
                }
            }
        }

        protected void Awake()
        {
            transform = this.GetOrAddComponent<PlanetariaTransform>();
            foreach (PlanetariaCollider collider in this.GetComponentsInChildren<PlanetariaCollider>())
            {
                collider.register(this);
                observers.Add(collider.get_observer());
            }
            // FIXME: still need to cache (properly)
            OnConstruction();
            StartCoroutine(wait_for_fixed_update());
        }

        protected void OnDestroy()
        {
            OnDestruction();
            // FIXME: still need to un-cache (properly)
            foreach (PlanetariaCollider collider in this.GetComponentsInChildren<PlanetariaCollider>())
            {
                collider.unregister(this);
            }
        }

        public void enter_block(BlockCollision collision)
        {
            if (OnBlockEnter.exists)
            {
                OnBlockEnter.data(collision);
            }
        }

        public void exit_block(BlockCollision collision)
        {
            if (OnBlockExit.exists)
            {
                OnBlockExit.data(collision);
            }
        }

        public void enter_field(PlanetariaCollider field)
        {
            if (OnFieldEnter.exists)
            {
                OnFieldEnter.data(field);
            }
        }

        public void exit_field(PlanetariaCollider field)
        {
            if (OnFieldExit.exists)
            {
                OnFieldExit.data(field);
            }
        }

        public new PlanetariaTransform transform;
        private List<CollisionObserver> observers = new List<CollisionObserver>();
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