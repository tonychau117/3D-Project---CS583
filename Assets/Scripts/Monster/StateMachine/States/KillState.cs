using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Monster.StateMachine.States
{
    public class KillState : MonsterState
    {
        public KillState(MonsterStateMachine monster) : base(monster){}

        
        
        public override async UniTask Enter()
        {
            monster.transform.rotation = Quaternion.LookRotation(
                movement.GetDirectionIgnoreY(monster.Player.transform.position));
            
            _ = animator.GoToState(MonsterStates.Kill);
            monster.speaker.PlayClip(AudioManager.Audios.Grab);
            monster.OnKill.Invoke();
            await UniTask.Delay(1000);
            monster.speaker.PlayClip(AudioManager.Audios.Damage);
            await UniTask.Delay(500);
            monster.speaker.PlayClip(AudioManager.Audios.Damage);
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
