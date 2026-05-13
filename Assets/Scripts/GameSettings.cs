using TriInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings"), HideMonoScript]
public class GameSettings : ScriptableObject
{
    public bool Debug;
}
