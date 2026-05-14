using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Monster.StateMachine.States
{
    public class PatrolState : MonsterState
    {
        /* * * * * * *
         * CONSTANTS  *
         * * * * * * */
        private const Stack<Vector3> NO_PATH = null;
        private const GameObject NO_PLAYER = null;
        private const int FIRST_MARKER = 0;
        private int DEFAULT_SPEED => movement.DefaultSpeed;
        /// Patrolling is not paused, and patrolling is possible (there are markers)
        private bool ALLOW_PATROL => !pausePatrolling && patrolMarkers.Count >= 1;
        
        /* * * * * * * * * * *
         * Patrolling Fields *
         * * * * * * * * * * */
        /// List of Empty Objects representing the coordinates of patrol destinations
        private List<Transform> patrolMarkers;
        /// How long an enemy should remain at a patrol marker before proceeding to the next
        private int patrolDelay; 
        
        /* * * * * * * * * * * * * * * 
         * Patrolling Runtime Fields *
         * * * * * * * * * * * * * * */
        /// Index representing the current patrol destination in patrol markers
        private int patrolIndex;
        /// <summary> The current path from the Start Node to the Goal Node (from Top to Bottom of the stack)</summary>
        private Stack<Vector3> curPath;
        /// When true, patrolling is stopped
        private bool pausePatrolling;

        private Vector3 Destination
        {
            get => monster.Destination;
            set => monster.Destination = value;
        }

        public PatrolState(MonsterStateMachine monster) : base(monster){}
        
        // HEADER: STATE METHODS

        // Do Animations & Set Default Values
        public override UniTask Enter()
        {
            
            movement = monster.movement;
            detection = monster.detection;
            
            // INITIALIZE PATROL DATA
            patrolMarkers = monster.PatrolMarkers;
            patrolDelay = monster.PatrolDelay;
            
            _ = animator.GoToState(MonsterStates.Patrol);
            
            // SET DEFAULTS
            movement.Speed = DEFAULT_SPEED; 
            patrolIndex = FIRST_MARKER;
            
            // PATH CREATION
            if (patrolMarkers.Count <= 0) return UniTask.CompletedTask; // If no markers, don't create a path
            SetNewPath(); // Create a path to the first marker
            
            return UniTask.CompletedTask;
        }

        // Run State Functionality
        public override UniTask Run()
        {
            FindPrimaryTarget(); // Look for a target
            
            if (ALLOW_PATROL) Patrol(); // If patrolling is not paused, patrol
            
            return UniTask.CompletedTask;
        }

        // Do Exit Functionality and Exit Animations
        public override async UniTask Exit()
        {
            monster.speaker.StopLoop();
            if (Player == NO_PLAYER) return;
            monster.transform.rotation = Quaternion.LookRotation(
                movement.GetDirectionIgnoreY(monster.Player.transform.position)); // Look at target before attacking
            monster.speaker.PlayClip(AudioManager.Audios.Scream);
            await animator.GoToState(MonsterStates.Chase);
        }
        
        
        // HEADER: PATROL METHODS
        
        /// <summary> Implementation for making an Enemy follow a given path </summary>
        private void Patrol()
        {
            // If the enemy has not yet reached the target position, move towards it
            if (!NoPath() && !movement.AtLocation(Destination))
            {
                _ = animator.GoToState(MonsterStates.Patrol);
                movement.MoveTowardsLocation(Destination);
            }
            
            // If the enemy has reached the target position, but the path has not been fully traversed, get the next target position
            else if (!NoPath()){ Destination = curPath.Pop(); }

            // If the Stack is empty, make a new path
            else
            {
                monster.speaker.StopLoop();
                _ = animator.GoToState(MonsterStates.None);

                pausePatrolling = true;
                InvokeWithDelay(SetNewPath, patrolDelay);
            }
        }
        
        /// <summary> Implementation for creating a new Enemy Path, based on the Patrol Type </summary>
        private async void SetNewPath()
        {
            patrolIndex = IncrementIndex(patrolIndex, patrolMarkers, false, true); 
            
            curPath = await CreatePath(); // Create a new path in either case

            pausePatrolling = false;
            
            if (curPath == NO_PATH)
            {
                await UniTask.Delay(1000);
                return; // If no path was found, return
            }

            Destination = curPath.Pop(); // Pop the first target position from the path stack
            monster.speaker.PlayLoop(AudioManager.Audios.Walk);
        }

        
        // HEADER: Enemy Detection
        
        /// Iterate through a list of visible enemies to determine which one is the closest (enemy will be set to target this one)
        private void FindPrimaryTarget() {
            if (Player && detection.TransformInSight(Player.transform)){
                _ = monster.ChangeToState(MonsterStates.Patrol, MonsterStates.Chase);
            } 
        }
        
        
        // HEADER: HELPER METHODS
        // HDESC: Just some methods to help make the code more readable
        
        /// Returns true if a list is empty
        private static bool Empty<T>(IEnumerable<T> list) { return !list.Any(); }
        
        /// Returns true if the enemy doesn't have a path / curPath is empty 
        private bool NoPath() { return curPath == NO_PATH || Empty(curPath); }

        /// Use A* search to create a path to the enemy's next destination
        private async Task<Stack<Vector3>> CreatePath() { 
            return AStarSearch.Search (
            monster.transform, GetDestination(), epochs: 3000, useFloorPos: true
            ); 
        }

        /// <summary> Return the vector coordinates of the enemy's current target destination </summary>
        private Vector3 GetDestination() { return patrolMarkers[patrolIndex].position; }

        /// <summary> Method for incrementing an array's index </summary>     <param name="index">Array Index</param>
        /// <param name="enumerable"> The array / list of the index </param>
        /// <param name="loop"> Whether the index should loop back to 0. If not, just return -1</param>
        /// <param name="random"> Whether or not a random index should be chosen, rather than incrementing </param>
        private static int IncrementIndex<T>(int index, IEnumerable<T> enumerable, bool loop = true, bool random = false)
        {
            // If Random, return a random index (within the size of the list)
            if (random) { return Random.Range(0, enumerable.Count()); }

            // If the index has not reached the list size, increment it
            if (index < enumerable.Count() - 1) return ++index;
            
            // If Loop (and index has reached list size), return 0
            if(loop){return 0;} 
            
            // If none of these are true, return -1 
            return -1;
        }
    }
}
