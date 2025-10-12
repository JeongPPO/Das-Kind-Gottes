using System.Collections.Generic;

public static class InvestigationManager
{
    private static readonly List<ImportantObject> objects = new();

    public static void Register(ImportantObject obj)
    {
        if (!objects.Contains(obj))
            objects.Add(obj);
    }

    public static void Unregister(ImportantObject obj)
    {
        objects.Remove(obj);
    }

    public static void SetHighlightAll(bool active)
    {
        foreach (var obj in objects)
            obj.Highlight(active);
    }
}