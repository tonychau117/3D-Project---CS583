using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vertx.Debugging;

namespace Tools
{
    public static class PhyTools
    {
    
        /* Every method here will draw the cast if Debug is On in the Game Settings (Assets -> Resources -> GameSettings -> Debug)
         (Unless specified otherwise in the method parameters)*/
    
        // HEADER: RAY CASTING
        
        /// Returns the transform hit by the Raycast 
        private static Transform Raycast(Vector3 position, Vector3 direction, float distance, 
            Color debugColor = default, bool debug = true)
        {
            // TODO: Come back to later and make sure this works
            if (Settings.Debug && debug)
            {
                D.raw(new Ray(position, direction), debugColor, 0.5f);
                //Debug.DrawRay(position, direction * distance, debugColor);
            }
            Physics.Raycast(position, direction, out var hit, distance);
            return hit.transform;
        }
    
        /// Returns true if the hit gameObject has any of the provided tags
        public static bool RaycastForTag(Vector3 position, Vector3 direction, float distance, List<string> tags, 
            Color debugColor = default, bool debug = true)
        {
            var hit = Raycast(position, direction, distance, debugColor, debug);
            var transform = hit ? hit.transform : null;
            return transform && tags.Any(transform.CompareTag); // Returns true if the hit object contains any of the tags
        }

        /// Returns true if there is an unobstructed ray between the given position and transform
        public static bool RaycastForTransform(Vector3 position, Vector3 direction, float distance, Transform entity, 
            Color debugColor = default, bool debug = true) {
            var hit = Raycast(position, direction, distance, debugColor, debug);
            return hit && hit.transform == entity;
        }

        
        // HEADER: BOX CASTING
        
        /// Returns a list of Transforms that are caught in the Boxcast <br></br>
        /// (Does not detect colliders that the box spawns inside)
        public static List<Transform> BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, 
            float maxDistance, Color debugColor = default, bool debug = true)
        {
            var hits = new RaycastHit[3];
            if (Settings.Debug && debug)
            {
                D.raw(new Shape.BoxCastAll(center, halfExtents, direction, hits, orientation, maxDistance), debugColor, 0.5f);
                //DrawPhysics.BoxCastNonAlloc(center, halfExtents, direction, hits, orientation, maxDistance)
            }
            else { Physics.BoxCastNonAlloc(center, halfExtents, direction, hits, orientation, maxDistance); }
        
            var transforms = hits.Select(hit => hit.transform).ToList();
            return transforms;
        }

        /// Returns a list of Colliders that are caught in the Overlap Box <br></br>
        /// (DOES detect colliders that the box spawns inside)
        public static Collider[] OverlapBox(Vector3 center, Vector3 halfExtents, 
            Color debugColor = default, bool debug = true)
        {
            var hits = new Collider[3];
            
            if (Settings.Debug && debug)
            {
                D.raw(new Shape.Box(center, halfExtents), debugColor, 0.5f);
                //DrawPhysics.BoxCastNonAlloc(center, halfExtents, direction, hits, orientation, maxDistance)
            }
            Physics.OverlapBoxNonAlloc(center, halfExtents, hits);
            
            return hits;
        }
    }
}
