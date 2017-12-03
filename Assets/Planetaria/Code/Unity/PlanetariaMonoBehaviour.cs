﻿using System.Collections.Generic;
using UnityEngine;

namespace Planetaria
{
    public abstract class PlanetariaMonoBehaviour : MonoBehaviour
    {
        protected delegate void CollisionDelegate(BlockCollision block_information);
        protected delegate void TriggerDelegate(PlanetariaCollider field_information);

        protected optional<CollisionDelegate> OnBlockEnter = null;
        protected optional<CollisionDelegate> OnBlockExit = null;
        protected optional<CollisionDelegate> OnBlockStay = null;
    
        protected optional<TriggerDelegate> OnFieldEnter = null;
        protected optional<TriggerDelegate> OnFieldExit = null;
        protected optional<TriggerDelegate> OnFieldStay = null;

        private void OnCollisionStay()
        {
            if (OnFieldStay.exists)
            {
                foreach (PlanetariaCollider field in fields)
                {
                    OnFieldStay.data(field);
                }
            }
            if (OnBlockStay.exists)
            {
                if (current_collision.exists)
                {
                    OnBlockStay.data(current_collision.data);
                }
            }
        }

        protected virtual void Awake()
        {
            foreach (PlanetariaCollider channel in this.GetComponentsInChildren<PlanetariaCollider>())
            {
                channel.register(this);
            }
            // FIXME: still need to cache (properly)
        }

        protected virtual void OnDestroy()
        {
            // FIXME: still need to un-cache (properly)
            foreach (PlanetariaCollider channel in this.GetComponentsInChildren<PlanetariaCollider>())
            {
                channel.unregister(this);
            }
        }


        public void enter_block(BlockCollision collision)
        {
            if (!current_collision.exists)
            {
                if (OnBlockEnter.exists)
                {
                    OnBlockEnter.data(collision);
                }
                current_collision = collision;
            }
            else
            {
                Debug.Log("Critical Error");
            }
        }

        public void exit_block(BlockCollision collision)
        {
            if (current_collision.exists && collision == current_collision.data) // FIXME: probably have to create proper equality function
            {
                if (OnBlockExit.exists)
                {
                    OnBlockExit.data(current_collision.data);
                }
                current_collision = new optional<BlockCollision>();
            }
            else
            {
                Debug.Log("Critical Error");
            }
        }

        public void enter_field(PlanetariaCollider field)
        {
            fields.Add(field);
            if (OnFieldEnter.exists)
            {
                OnFieldEnter.data(field);
            }
        }

        public void exit_field(PlanetariaCollider field)
        {
            if (fields.Remove(field))
            {
                if (OnFieldExit.exists)
                {
                    OnFieldExit.data(field);
                }
            }
            else
            {
                Debug.Log("Critical Error");
            }
        }

        public new PlanetariaTransform transform = this.GetOrAddComponent<PlanetariaTransform>();
        private optional<BlockCollision> current_collision = new optional<BlockCollision>();
        private List<PlanetariaCollider> fields = new List<PlanetariaCollider>();
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