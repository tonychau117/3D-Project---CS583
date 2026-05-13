using System;
using Tools;
using UnityEngine;

namespace Monster.Components
{
    public class Detection : EntityComponent
    {
        // HEADER: CONSTANTS

        private const int defaultRange = 20;
        
        
        // HEADER: FIELDS 

        [Tooltip("The distance this entity can detect a target from")]
        private int range = defaultRange;
        /// The distance this entity can detect a target from
        public int Range {
            get => range;
            set => range = value < 0 ? throw new Exception("Range cannot be negative!") : value; }
        
        [Tooltip("Detection range when an NPC is aware of the target"), SerializeField] 
        private int activeDetectionRange = defaultRange + 10;
        /// Detection range when an NPC is aware of the target
        public int ActiveDetectionRange {
            get => activeDetectionRange;
            set => activeDetectionRange = value < 0 ? throw new Exception("Active Range cannot be negative!") : value;
        }
            
        
        // HEADER: COMPONENTS

        private Movement movement;
        
        
        // HEADER: START

        public new void Awake() {
            base.Awake();
            movement = GetComponent<Movement>();
        }
        
        
        // HEADER: HELPER METHODS

        /// Returns true if there is a direct line between this npc and the transform
        public bool TransformInView(Transform target_transform) {
            return PhyTools.RaycastForTransform(
                position, // Start Raycast
                movement.GetCenteredDirection(target_transform.position), // Raycast Direction
                ActiveDetectionRange, // Raycast Distance
                target_transform, // The transform to detect
                Color.blue ); // Debug color
        }
        
        /// Returns true if there is a direct <b>line of sight</b> between this NPC and the transform
        public bool TransformInSight(Transform target_transform) {
            return movement.InFront(target_transform.position) && TransformInView(target_transform); }
        
    }
}
