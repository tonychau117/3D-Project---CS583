using UnityEngine;
using System;
//using Components;
using Cysharp.Threading.Tasks;
using Monster.Components;

namespace Monster.StateMachine.States
{
    /* * * * *
     * Enums *
     * * * * */
    /// Enum representing which state the NPC is in <br></br>
    /// [ <see cref="MonsterStates.None">None</see>,
    /// <see cref="MonsterStates.Any">Any</see>,
    /// <see cref="MonsterStates.Patrol">Patrol</see>,
    /// <see cref="MonsterStates.Chase">Chase</see>,
    /// <see cref="MonsterStates.Search">Search</see>,
    /// <see cref="MonsterStates.Kill">Kill</see>,
    public enum MonsterStates
    {
        /// The monster is not currently in any state / state has not been initialized
        None = -1,
        /// The monster is in any given state
        Any = 0,
        /// The monster is walking around the map, searching for the player
        Patrol = 1,
        /// The monster is running and pursuing the player
        Chase = 2,
        /// The monster has lost sight of the player, and is looking for them
        Search = 3,
        /// The monster is in range of the player, and will kill them
        Kill = 4
    }
    
    /// Abstract class for creating the states an NPC can be in
    public abstract class MonsterState 
    {
        // HEADER: State Machine
        protected readonly MonsterStateMachine monster;
        
        /* * * * * * * * *
         * NPC Components *
         * * * * * * * * */
        protected GameObject Player => monster.Player;
        protected Movement movement;
        protected Detection detection;
        protected AnimationHandler animator;

        
        // HEADER: CONSTRUCTOR

        protected MonsterState(MonsterStateMachine this_monster)
        {
            monster = this_monster;
            movement = monster.movement;
            detection = monster.detection;
            animator = monster.animator;
        }
        
        // HEADER: STATE MANAGEMENT
        
        /// Functionality to execute when entering this state
        public abstract UniTask Enter();
        /// Functionality to run while in this state
        public abstract UniTask Run();
        /// Functionality to run when leaving this state
        public abstract UniTask Exit();
        

        // HEADER: PAUSE FUNCTIONS
        // ReSharper disable Unity.PerformanceAnalysis
        /// Run the given function after a given delay
        public async UniTask InvokeWithDelay(Action func, int delay){
            await UniTask.Delay(delay);
            func.Invoke();
        }

        /// Run the given function repeatedly until a condition is met 
        public async UniTask InvokeWithWaitUntil(Action func, Func<bool> conditional){
            await UniTask.WaitUntil(conditional);
            func.Invoke();
        }
    }
}

