using UnityEngine;

[CreateAssetMenu(fileName = "AssaultPattern", menuName = "Daily/AssaultPattern")]
public class AssaultPatternSO : ScriptableObject
{
    public string enemyName;
    public KeyCode[] keySequence; // 입력 순서
    public float timeLimit = 3f;
}