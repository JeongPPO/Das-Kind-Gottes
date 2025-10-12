using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;

public class CharacterSpriteManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpriteGroup
    {
        public string enemyName;
        public List<GameObject> sprites;
    }

    public List<EnemySpriteGroup> enemySpriteGroups = new List<EnemySpriteGroup>();

    public void ShowCharacterSpriteAnim(string spriteName, string animTrigger)
    {
        string currentEnemyName = EnemyManager.Instance.CurrentEnemy?.enemyName;
        if (string.IsNullOrEmpty(currentEnemyName))
        {
            Debug.LogWarning("현재 Enemy의 이름을 찾을 수 없습니다.");
            return;
        }

        foreach (var group in enemySpriteGroups)
        {
            if (group.enemyName == currentEnemyName)
            {
                foreach (var obj in group.sprites)
                {
                    if (obj.name == spriteName)
                    {
                        obj.SetActive(true);
                        var animator = obj.GetComponent<Animator>();
                        if (animator != null)
                            animator.SetTrigger(animTrigger);
                    }
                    else
                    {
                        obj.SetActive(false);
                    }
                }
            }
            else
            {
                foreach (var obj in group.sprites)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}