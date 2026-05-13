using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Monster.StateMachine.States
{
    public class ChaseState : MonsterState
    {
       // HEADER: CONSTRUCTOR
        public ChaseState(MonsterStateMachine monster) : base(monster) {}

        
        // HEADER: STATE METHODS
        
        // Set Default Values
        public override UniTask Enter() {
            
            // Initialize Values
            movement.Speed = movement.DefaultSpeed + 3;
            
            return UniTask.CompletedTask;
        }

        // Follow the target, check if they are still in sight and if they are in range for an attack
        public override async UniTask Run()
        {
            if(!monster.Player)
            {
                _ = monster.ChangeToState(MonsterStates.Chase, MonsterStates.Patrol);
                return;
            }
            
            FollowTarget(); // NPC will follow their target
            
            if(!CheckIfInSight()) { IfLost(); } // If the NPC loses sight of the target
            
            // If NPC is in range of their target, do an attack
            if (!CheckIfInRange()) return;
            
            _ = monster.ChangeToState(MonsterStates.Chase, MonsterStates.Kill); 
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }

        
        // HEADER: CHASE METHODS
        
        /// Move the enemy towards the target's location
        protected virtual void FollowTarget() {  movement.MoveTowardsLocation(monster.Player.transform.position); }

        /// Check to see if the target is close enough to attack the target
        protected virtual bool CheckIfInRange() {
            return movement.WithinLocation(monster.KillDistance ,monster.Player.transform.position); }
        
        protected virtual bool CheckIfInSight()
        { return detection.TransformInView(monster.Player.transform); }

        /// What the NPC should do if they lose a target
        protected virtual void IfLost()
        {
            _ = monster.ChangeToState(MonsterStates.Chase, MonsterStates.Patrol);
        }
    }
}
