using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Vector2Int gridPosition;
    public List<GameObject> occupants = new List<GameObject>();
    public bool isOccupied = false;

    public TextMeshPro textMesh;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetText();
    }

    public void SetOccupied(bool value)
    {
        isOccupied = value;
    }

    public void AddOccupant(GameObject obj)
    {
        if (!occupants.Contains(obj)) occupants.Add(obj);
        isOccupied = true;
    }

    public void RemoveOccupant(GameObject obj)
    {
        if (occupants.Contains(obj)) occupants.Remove(obj);
        isOccupied = occupants.Count > 0;
    }

    public bool HasEnemy(string tag)
    {
        return occupants.Any(o => o.CompareTag(tag));
    }

    public bool HasWall()
    {
        return occupants.Any(o => o.CompareTag("Wall"));
    }

    public void SetText()
    {
        if (textMesh != null)
            textMesh.text = $"({gridPosition.x}, {gridPosition.y})";
    }

    void OnMouseDown()
    {
        spriteRenderer.color = Color.red;
        Debug.Log($"Cell clicked: {gridPosition}");
    }
}
