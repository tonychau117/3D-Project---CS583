using System;
using TriInspector;
using UnityEngine;

namespace Monster.Components
{
    /// Abstract class for managing the movement of an entity (Base Value: Speed)
    [HideMonoScript]
    public class Movement : EntityComponent
    {
        
        // HEADER: FIELDS 

        [Tooltip("How fast this entity moves"), SerializeField] 
        private int speed;
        public int Speed {
            get => speed;
            set => speed = value < 0 ? throw new Exception("Speed cannot be negative!") : value;
        }
        public int DefaultSpeed { get; private set; }
        
        [Tooltip("Adds to this entity's position so that it matches the center of this entity"), SerializeField]
        private Vector3 centering_factor;
        
        /// The forward direction of this entity
        private Vector3 forward => transform.forward;
        
        
        // HEADER: START

        public new void Awake() {
            base.Awake();
            DefaultSpeed = speed;
        }
        
        
        // HEADER: EXTRA MODIFIERS
        
        /// Set speed to 0, stopping the entity
        public void Stop(){speed = 0;}
    
        /// Raise the speed by the given value
        public void RaiseSpeed(int value){speed += value;}
    
        /// Lower the speed by the given value
        public void LowerSpeed(int value){speed -= value;}
        
        
        // HEADER: POSITION LOGIC

        /// Set a given vectors y-value to the y-value of the entity
        private Vector3 GetGroundedPosition(Vector3 pos) { return new Vector3(pos.x, position.y, pos.z); }

        /// Returns true if an NPC is within a certain distance from a coordinate (Ignores height)
        public bool WithinLocation(float distance, Vector3 location) {
            return Vector3.Distance(position, GetGroundedPosition(location)) < distance; }
        
        /// Returns true if an NPC is within 1 unit from a coordinate (Ignores Height)
        public bool AtLocation(Vector3 location) { return WithinLocation(1, location); }
        
        
        // HEADER: MOVEMENT LOGIC
        
        /// Move the NPC gradually towards a given location
        public void MoveTowardsLocation(Vector3 location)
        {
            // Move gradually towards the new position
            var new_pos = Vector3.MoveTowards(
                position, GetGroundedPosition(location), speed * Time.fixedDeltaTime);
            
            // TODO: May want to implement gradual turning at some point if possible?
            // Conversely, turn instantly towards the direction of movement
            var direction = GetDirectionIgnoreY(new_pos);
            var lookDir = direction == Vector3.zero ? transform.rotation : Quaternion.LookRotation(direction);
            
            rb.Move(new_pos, lookDir);
        }
        
        
        // HEADER: DIRECTION LOGIC
        
        /// Return the direction of this entity to the given coordinate
        public Vector3 GetDirection(Vector3 coordinate) { return Vector3.Normalize(coordinate - position); }

        public Vector3 GetDirectionIgnoreY(Vector3 coordinate) {
            return Vector3.Normalize(GetGroundedPosition(coordinate) - position);
        }

        public Vector3 GetCenteredDirection(Vector3 coordinate)
        {
            var center_position = position + centering_factor;
            return Vector3.Normalize(coordinate - center_position);
        }

        /// Return whether the given coordinate is in front of the NPC
        public bool InFront(Vector3 coordinate) { return Vector3.Dot(forward, GetDirection(coordinate)) >= 0; }
    }
}
