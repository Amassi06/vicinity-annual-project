using System.Collections.Generic;
using UnityEngine;

// Parcourt tous les Waypoint de la scène et génère un rectangle plat
// entre chaque waypoint et chacun de ses voisins.
public class WaypointManager : MonoBehaviour
{
    [Header("Apparence des routes")]
    public float roadWidth = 1f;
    public float roadThickness = 0.05f;
    public Color roadColor = new Color(0.20f, 0.20f, 0.22f);

    private Material roadMat;

    void Start()
    {
        Generate();
    }
public void Generate()
{
    if (roadMat == null) roadMat = MakeMat(roadColor);

    for (int i = transform.childCount - 1; i >= 0; i--)
        Destroy(transform.GetChild(i).gameObject);

    var waypoints = FindObjectsOfType<Waypoint>();

    foreach (var wp in waypoints)
    {
        if (wp.neighbors == null) continue;
        foreach (var n in wp.neighbors)
        {
            if (n != null && n != wp && !n.neighbors.Contains(wp))
                n.neighbors.Add(wp);
        }
    }

    var done = new HashSet<long>();
    foreach (var wp in waypoints)
    {
        if (wp.neighbors == null) continue;
        foreach (var n in wp.neighbors)
        {
            if (n == null || n == wp) continue;

            int a = wp.GetInstanceID();
            int b = n.GetInstanceID();
            long key = a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
            if (!done.Add(key)) continue;

            CreateSegment(wp.transform.position, n.transform.position);
        }
    }
}

    void CreateSegment(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float length = dir.magnitude;
        if (length < 0.001f) return;

        var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var col = seg.GetComponent<Collider>();
        if (col != null) Destroy(col);

        seg.transform.SetParent(transform);
        seg.transform.position = (from + to) * 0.5f;
        seg.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        seg.transform.localScale = new Vector3(roadWidth, roadThickness, length);

        var r = seg.GetComponent<Renderer>();
        r.sharedMaterial = roadMat;
        seg.name = "Route";
    }

    Material MakeMat(Color col)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        if (s == null) s = Shader.Find("Sprites/Default");
        var m = new Material(s);
        m.color = col;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
        return m;
    }
}