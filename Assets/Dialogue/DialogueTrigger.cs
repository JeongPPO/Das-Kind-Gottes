using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public string startNode = "StartofEverything";

    IEnumerator Start()
    {
        // DialogueRunner가 초기화될 시간을 1프레임 줍니다.
        yield return null; 

        if (dialogueRunner != null && !string.IsNullOrEmpty(startNode))
        {
            if (dialogueRunner.IsDialogueRunning)
            {
                dialogueRunner.Stop();
            }
            dialogueRunner.StartDialogue(startNode);
        }
    }
}
