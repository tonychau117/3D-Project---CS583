using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Tools;
using Unity.VisualScripting;
using UnityEngine;

namespace Monster.StateMachine.States
{
    public class SearchState : MonsterState
    {
        /// The current path from the Start Node to the Goal Node (from Top to Bottom of the stack)
        private Stack<Vector3> curPath;
        
        /// The second location to search for a lost target 
        private Vector3 secondDestination;
        /// The delay between marking the first and second search locations
        private int SearchTime => monster.SearchTime;
        
        /// Whether the entity is searching the second search location
        private bool findNewPath;

        private bool timeout;
        
        
        // HEADER: CONSTRUCTOR

        public SearchState(MonsterStateMachine monster) : base(monster) {}
        
        
        // HEADER: STATE METHODS
        // ReSharper disable Unity.PerformanceAnalysis
        public override UniTask Enter()
        {
            // Create a path to the last known location of the target
            curPath = AStarSearch.Search(monster.transform, Player.transform.position);
            if(curPath.IsUnityNull()){ _ = monster.ChangeToState(MonsterStates.Search, MonsterStates.Patrol); 
                return UniTask.CompletedTask;}
            
            // Start moving the enemy through the path
            if (curPath != null) monster.Destination = curPath.Pop();
            
            movement.Speed = movement.DefaultSpeed + 3;
            
            InvokeWithDelay(
            () => {
                HandleTimeout();
            }, SearchTime);
            return UniTask.CompletedTask;
        }

        public override async UniTask Run() {
            // If the enemy is found, return to a Chase State
            if (detection.TransformInSight(Player.transform)){
                _ = monster.ChangeToState(MonsterStates.Search, MonsterStates.Chase); return;
            }

            if (timeout)
            {
                // TODO: FIX ANIMATIONS
                //enemy.SetAnimationBool("Chase", false);
                await UniTask.Delay(3000); // Delay before returning to idle state
                _ = monster.ChangeToState(MonsterStates.Search, MonsterStates.Patrol);
                return;
            }
            
            // If Search execution is paused, return, otherwise do Search for Target
            await Search();
        }

        public override UniTask Exit() {return UniTask.CompletedTask;}
        
        
        // HEADER: SEARCH METHODS
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// Implementation for making an Enemy follow a given path 
        private async Task Search()
        {
            // If the enemy has not yet reached the target position, move towards it
            if (!movement.AtLocation(monster.Destination))
            {
                // TODO: FIX ANIMATIONS
                //enemy.SetAnimationBool("Chase", true);
                movement.MoveTowardsLocation(monster.Destination);
            }
            
            // If the enemy has reached the target position, but the path has not been fully traversed, get the next target position
            else if (!NoPath()){ monster.Destination = curPath.Pop(); }
            
            // If the Stack is empty, 
            else
            {
                curPath = AStarSearch.Search(monster.transform, Player.transform.position);
                
                // If a valid path is not found, set the NPC back to idle
                if(NoPath()){ _ = monster.ChangeToState(MonsterStates.Search, MonsterStates.Patrol); return;}
                
                // Otherwise, move down the path
                monster.Destination = curPath.Pop();
            }
        }

        private void HandleTimeout()
        {
            if (detection.TransformInSight(Player.transform))
            {
                InvokeWithDelay(
                    () => { HandleTimeout(); }, SearchTime);
            }
            else
            {
                _ = monster.ChangeToState(MonsterStates.Search, MonsterStates.Patrol);
            }
        }
        
        // HEADER: HELPER METHODS
        
        /// <summary> Returns true if the enemy doesn't have a path / curPath is empty </summary>
        private bool NoPath() { return curPath.IsUnityNull() || curPath.Count == 0; }
    }
}
