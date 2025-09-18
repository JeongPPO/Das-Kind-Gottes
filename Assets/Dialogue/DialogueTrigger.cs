using UnityEngine;
using Yarn.Unity;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueRunner dialogueRunner;   // Inspector에서 연결하거나 GetComponent로 찾기
    public string startNode = "Introduction";      // 시작할 노드 이름

    void Start()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.StartDialogue(startNode);
        }
        else
        {
            Debug.LogError("DialogueRunner not assigned!");
        }
    }

    // 트리거 방식으로 하고 싶다면 이런 식도 가능
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueRunner.StartDialogue(startNode);
        }
    }
}
