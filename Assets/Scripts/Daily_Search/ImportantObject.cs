using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ImportantObject : MonoBehaviour
{
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        InvestigationManager.Register(this);
    }

    private void OnDisable()
    {
        InvestigationManager.Unregister(this);
    }

    public void Highlight(bool active)
    {
        sr.color = active ? Color.cyan : Color.white;
    }
}
