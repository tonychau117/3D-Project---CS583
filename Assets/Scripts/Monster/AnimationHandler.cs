using Cysharp.Threading.Tasks;
using Monster.StateMachine.States;
using TriInspector;
using UnityEditor.Animations;
using UnityEngine;

namespace Monster
{
    [HideMonoScript]
    [RequireComponent(typeof(Animator))]   
    public class AnimationHandler : MonoBehaviour
    {
        private static readonly int Patrol = Animator.StringToHash("Patrol");
        private static readonly int Chase = Animator.StringToHash("Chase");
        //private static readonly int Taunt = Animator.StringToHash("Taunt");
        private static readonly int Kill = Animator.StringToHash("Kill");

        [SerializeField] private AnimatorController controller;
        
        private Animator anim;
        private MonsterStates currentState = MonsterStates.None;

        private bool taunting;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            anim.runtimeAnimatorController = controller;
        }
    
        private void SwitchFromState(MonsterStates state)
        {
            switch (state)
            {
                case MonsterStates.None:
                    break;
                case MonsterStates.Patrol:
                    anim.SetBool(Patrol, false);
                    break;
                case MonsterStates.Chase:
                    anim.SetBool(Chase, false);
                    //anim.SetBool(Taunt, false);
                    break;
                case MonsterStates.Search:
                    if (currentState == MonsterStates.Chase) return;
                    anim.SetBool(Chase, false);
                    break;
                default:
                    print("Cannot switch from animation state " + currentState);
                    break;
            }
        }
        // ReSharper disable Unity.PerformanceAnalysis
        public async UniTask GoToState(MonsterStates state)
        {
            if (state == currentState) return;
            var old_state = currentState;
            currentState = state;
            switch (state)
            {
                case MonsterStates.None:
                    break;
                case MonsterStates.Patrol:
                    anim.SetBool(Patrol, true);
                    break;
                case MonsterStates.Chase:
                    taunting = true;
                    //anim.SetBool(Taunt, true);
                    anim.SetBool(Chase, true);
                    await UniTask.WaitUntil(() => !taunting);
                    break;
                case MonsterStates.Search:
                    anim.SetBool(Chase, true);
                    break;
                case MonsterStates.Kill:
                    anim.SetTrigger(Kill);
                    break;
                default:
                    print("Cannot switch to animation state " + state);
                    break;
            }
            SwitchFromState(old_state);
        }

        public void FinishAnimation(string _animation)
        {
            taunting = _animation switch
            {
                "Taunt" => false,
                _ => taunting
            };
        }
    }
}
