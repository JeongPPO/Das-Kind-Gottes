using UnityEngine;

[CreateAssetMenu(fileName = "InfiltrationConfig", menuName = "Infiltration/Config")]
public class InfiltrationConfigSO : ScriptableObject
{
    [Header("Grid")]
    public Vector2Int gridOrigin = new Vector2Int(0, 0);
    public int gridWidth = 34;
    public int gridHeight = 22;
    public float tileSize = 1f;

    [Header("Movement")]
    public float baseMoveCooldown = 0.12f;  // 타일당 이동 쿨다운
    public float stealthMoveCooldownMultiplier = 1.5f; // 은신 시(참고용)

    [Header("Input thresholds")]
    public float longPressASeconds = 0.35f;      // 필살기(참고용)
    public float longPressSpaceSeconds = 0.35f;  // 힐 길게누르기
    public float parryWindowSeconds = 0.18f;

    [Header("Attacker")]
    public float attackerBasicDamage = 0.5f;     // 하트 0.5개

    [Header("Supporter")]
    public int dashTiles = 2;                    // 2칸 대시
    public float dashCooldown = 0.6f;

    [Header("Healer")]
    public float healAmountHearts = 1f;          // 하트 1개 회복
    public float healCooldown = 10f;
    public float parryCooldown = 10f;

    [Header("Hazard")]
    public float hazardDamageOnEnter = 0.25f;    // 진입 즉시 0.25 하트
    public float hazardDamagePerTick = 0.25f;    // 초당 0.25 하트
    public float hazardTickInterval = 1.0f;

    [Header("Lives")]
    public int lives = 3;
}