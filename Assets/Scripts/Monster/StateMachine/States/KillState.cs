using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Monster.StateMachine.States
{
    public class KillState : MonsterState
    {
        public KillState(MonsterStateMachine monster) : base(monster){}

        
        
        public override UniTask Enter()
        {
            monster.transform.rotation = Quaternion.LookRotation(
                movement.GetDirectionIgnoreY(monster.Player.transform.position));
            // TODO: FIX ANIMATIONS
            // Do animation
            monster.OnKill.Invoke();
            return UniTask.CompletedTask;
        }

        public override UniTask Run()
        {
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            return UniTask.CompletedTask;
        }
    }
}
