using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Monster.Components;
using Monster.StateMachine.States;
using UnityEngine;
using TriInspector;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace Monster.StateMachine
{
    /// Abstract class for state machines that handle switching between states
    [HideMonoScript]
    [RequireComponent(typeof(Movement)),
    RequireComponent(typeof(Detection))]
    public class MonsterStateMachine : MonoBehaviour
    {
        // HEADER: CONSTANTS 
        private const MonsterStates NoState = MonsterStates.None;
        
        
        // HEADER: SERIALIZED FIELDS

        [Header("Player"), SerializeField]
        private GameObject player;
        public GameObject Player => player;
        
        [Header("Patrolling")]
        
        [Tooltip("Empty Objects representing the locations you want the monster to pathfind to"), SerializeField]
        private List<Transform> patrolMarkers;
        public List<Transform> PatrolMarkers => patrolMarkers;
        [Tooltip("How long the monster should remain at a marker before moving on to the next"), SerializeField]
        private int patrolDelay;
        public int PatrolDelay => patrolDelay;

        [Header("Searching")] 
        
        [Tooltip("Time in seconds that the enemy will follow a player (will reset at timeout if player is still in sight"), SerializeField] 
        private int searchTime;
        public int SearchTime => searchTime;

        [Header("Chasing")]
        
        [Tooltip("How close the monster needs to get to the player to kill them"), SerializeField]
        private int killDistance;
        public int KillDistance => killDistance;
        
        [Header("Kill")]
        
        public UnityEvent OnKill;
        
        
        // HEADER: CONSTANT FIELDS
        
        /* * * * * * * 
         * Components *
         * * * * * * */
        public Vector3 position => transform.position;
        public Movement movement { get; private set; }
        public Detection detection { get; private set; }
        
        
        // HEADER: RUNTIME FIELDS
        
        /* * * * * *
         * States *
         * * * * * */
        /// The current state of the Monster NPC
        private MonsterState CurrentState { get; set; }

        // TODO: Make Getter/Setter once states have been made
        private MonsterStates CurrentStateID
        {
            get
            {
                return CurrentState switch
                {
                    PatrolState => MonsterStates.Patrol,
                    ChaseState => MonsterStates.Chase,
                    SearchState => MonsterStates.Search,
                    KillState => MonsterStates.Kill,
                    _ => MonsterStates.None
                };
            }
        }
        
        /* * * * * * * * * * *
         * Execution Pausing *
         * * * * * * * * * * */
        /// True if the program is waiting for a running frame to end
        private bool awaitingRun;
        /// Set true to halt execution of the current state
        private bool stop;
        /// Returns true if Run() execution is not halted due to <see cref="stop">stop</see> or <see cref="awaitingRun">awaitingRun</see>
        private bool paused => awaitingRun || stop;
        
        /* * * * * * * * *
         * Monster Values *
         * * * * * * * * */

        public Vector3 Destination { get; set; }
        
        
        // HEADER: START

        private void Awake()
        {
            movement = GetComponent<Movement>();
            detection = GetComponent<Detection>();
        }
        
        protected void Start() { _ = ChangeToState(MonsterStates.None, MonsterStates.Patrol); }
        
        
        // HEADER: RUN METHODS
        
        /// Halts Update() execution in the State Machine
        private void Await() { awaitingRun = true; }
        /// Continues Update() execution in the State Machine
        private void Continue()  {awaitingRun = false; }
        
        // Do State.Run() every update, if not paused/halted
        void FixedUpdate() { if (!paused) Run(); }

        /// Stop Update() execution until the Run() of the current state has completed
        private async void Run() {
            if (CurrentStateID == NoState) return;
            Await(); await CurrentState.Run(); Continue();
        }
        
        
        // HEADER: STATE MANAGEMENT
        // ReSharper disable Unity.PerformanceAnalysis
        // TODO: Come back to commented out code
        /// Exit old state, change to new state, enter new state
        public async UniTask ChangeToState(MonsterStates fromState, MonsterStates toState/*!, bool stopRun = false*/)
        {
            
            // Do not Change State if the calling state is no longer valid
            if (CurrentStateID != fromState /*!&& (int)fromState >= 0*/) return;

            //
            if (awaitingRun/*! && !stopRun*/) await UniTask.WaitUntil(() => !awaitingRun);
            
            // Stop the current running state
            stop = true;
            
            // Exit the previous state
            if(CurrentStateID != NoState){ await CurrentState.Exit(); CurrentState = null;}
            
            // Change to the new state
            await SetState(toState);
            
            // Enter the new state
            if(CurrentStateID != NoState)  { await CurrentState!.Enter(); }
            
            // Resume state running
            stop = false;
            
            if(Settings.Debug){print("Monster has changed from " + fromState + " to " + toState);}
        }

        /// Set the current state based on the give state enum (can be overrided to add more options in subclasses)
        protected async UniTask SetState(MonsterStates state)
        {
            switch (state) {
                case MonsterStates.Patrol: 
                    await ChangeToPatrolState(); break;
                case MonsterStates.Chase: 
                    await ChangeToChaseState(); break;
                case MonsterStates.Search: 
                    await ChangeToSearchState(); break;
                case MonsterStates.Kill:
                    await ChangeToKillState(); break;
                default: throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        /// Change to <see cref="PatrolState">Idle State</see>
        protected UniTask ChangeToPatrolState()
        {
            CurrentState = new PatrolState(this);
            return UniTask.CompletedTask;
        }

        /// Change to <see cref="ChaseState">Chase State</see>
        protected UniTask ChangeToChaseState() {
            CurrentState = new ChaseState(this);
            return UniTask.CompletedTask;
        }

        /// Change to <see cref="SearchState">Search State</see>
        protected UniTask ChangeToSearchState()
        {
            CurrentState = new SearchState(this);
            return UniTask.CompletedTask;
        }
        
        /// Change to <see cref="KillState">Kill State</see>
        protected UniTask ChangeToKillState() {
            CurrentState = new KillState(this);
            return UniTask.CompletedTask;
        }
    }
}
