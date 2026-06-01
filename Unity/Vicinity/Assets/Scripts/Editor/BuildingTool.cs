using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildingTool : EditorWindow
{
    enum Tab { Waypoint, Batiment }
    Tab tab = Tab.Waypoint;

    static readonly Vector2 HouseFootprint = new Vector2(4f, 4f);
    const float HouseFloorHeight = 1.5f;
    const float Margin = 0.1f;

    float accessDistance = 4f;
    float roadWidth = 1f;
    bool otherSide = false;
    bool placing = false;

    [MenuItem("Tools/Building Tool")]
    static void Open() => GetWindow<BuildingTool>("Building Tool");

    void OnDisable() => StopPlacing();

    void OnSelectionChange() => Repaint();

    void OnGUI()
    {
        tab = (Tab)GUILayout.Toolbar((int)tab, new[] { "Waypoint", "Bâtiment" });
        EditorGUILayout.Space();

        if (tab == Tab.Waypoint) DrawWaypointTab();
        else DrawBatimentTab();
    }

    void DrawWaypointTab()
    {
        int count = CountSelected<Waypoint>();
        EditorGUILayout.LabelField("Waypoints sélectionnés", count.ToString());
        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(count < 2);
        if (GUILayout.Button("Lier la sélection")) LinkSelection();
        if (GUILayout.Button("Délier la sélection")) UnlinkSelection();
        EditorGUI.EndDisabledGroup();
    }

    void DrawBatimentTab()
    {
        EditorGUILayout.LabelField("Maison", EditorStyles.boldLabel);
        accessDistance = EditorGUILayout.FloatField("Distance d'accès", accessDistance);
        roadWidth = EditorGUILayout.FloatField("Largeur de route", roadWidth);
        otherSide = EditorGUILayout.Toggle("Côté opposé", otherSide);
        EditorGUILayout.Space();

        if (placing)
        {
            EditorGUILayout.HelpBox(
                "Clic gauche : poser   |   Tab : changer de côté   |   Échap / clic droit : arrêter",
                MessageType.Info);
            if (GUILayout.Button("Arrêter le placement")) StopPlacing();
        }
        else
        {
            if (GUILayout.Button("Placer une maison")) StartPlacing();
        }

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(CountSelected<House>() == 0);
        if (GUILayout.Button("Améliorer les maisons sélectionnées (+1 étage)")) UpgradeSelection();
        EditorGUI.EndDisabledGroup();
    }

    void StartPlacing()
    {
        if (placing) return;
        placing = true;
        SceneView.duringSceneGui += OnSceneGUI;
        SceneView.RepaintAll();
    }

    void StopPlacing()
    {
        if (!placing) return;
        placing = false;
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.RepaintAll();
        Repaint();
    }

    void OnSceneGUI(SceneView sv)
    {
        if (!placing) return;

        Event e = Event.current;
        int id = GUIUtility.GetControlID(FocusType.Passive);
        if (e.type == EventType.Layout) HandleUtility.AddDefaultControl(id);

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Tab)
        {
            otherSide = !otherSide;
            e.Use();
            sv.Repaint();
            Repaint();
            return;
        }
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            StopPlacing();
            e.Use();
            return;
        }
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            StopPlacing();
            e.Use();
            return;
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (!ground.Raycast(ray, out float enter)) return;
        Vector3 mouse = ray.GetPoint(enter);

        if (!FindNearestLink(mouse, out Waypoint a, out Waypoint b, out Vector3 linkPoint))
        {
            if (e.type == EventType.MouseMove) sv.Repaint();
            return;
        }

        Vector3 pa = a.transform.position;
        Vector3 pb = b.transform.position;
        if (!IsBefore(pa, pb)) { var sw = a; a = b; b = sw; var sp = pa; pa = pb; pb = sp; }
        Vector3 dir = pb - pa;
        dir.y = 0f;
        dir.Normalize();
        Vector3 perp = Vector3.Cross(Vector3.up, dir).normalized * (otherSide ? -1f : 1f);
        Vector3 center = linkPoint + perp * (accessDistance + HouseFootprint.y * 0.5f);

        bool valid = IsValid(center, dir, perp);
        DrawPreview(center, dir, perp, valid);

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (valid) PlaceHouseAt(a, b, linkPoint, perp);
            e.Use();
        }
        else if (e.type == EventType.MouseMove)
        {
            sv.Repaint();
        }
    }

    bool FindNearestLink(Vector3 p, out Waypoint a, out Waypoint b, out Vector3 point)
    {
        a = null; b = null; point = Vector3.zero;
        float best = float.MaxValue;
        var wps = FindObjectsOfType<Waypoint>();

        foreach (var wp in wps)
        {
            if (wp.neighbors == null) continue;
            foreach (var n in wp.neighbors)
            {
                if (n == null || n == wp) continue;
                Vector3 c = ClosestOnSegment(wp.transform.position, n.transform.position, p);
                float d = (c - p).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    a = wp; b = n; point = c;
                }
            }
        }
        return a != null;
    }

    bool IsValid(Vector3 center, Vector3 dir, Vector3 perp)
    {
        OBB2 box = new OBB2(
            new Vector2(center.x, center.z),
            new Vector2(dir.x, dir.z),
            new Vector2(perp.x, perp.z),
            HouseFootprint.x * 0.5f + Margin,
            HouseFootprint.y * 0.5f + Margin);

        var wps = FindObjectsOfType<Waypoint>();
        foreach (var wp in wps)
        {
            Vector3 wpos = wp.transform.position;
            if (box.Contains(new Vector2(wpos.x, wpos.z))) return false;
        }
        foreach (var wp in wps)
        {
            if (wp.neighbors == null) continue;
            foreach (var n in wp.neighbors)
            {
                if (n == null || n == wp) continue;
                Vector3 p1 = wp.transform.position;
                Vector3 p2 = n.transform.position;
                Vector3 sd = p2 - p1;
                sd.y = 0f;
                float len = sd.magnitude;
                if (len < 1e-4f) continue;
                sd /= len;
                Vector3 sp = Vector3.Cross(Vector3.up, sd);
                Vector3 mid = (p1 + p2) * 0.5f;
                OBB2 road = new OBB2(
                    new Vector2(mid.x, mid.z),
                    new Vector2(sd.x, sd.z),
                    new Vector2(sp.x, sp.z),
                    len * 0.5f,
                    roadWidth * 0.5f + Margin);
                if (box.Intersects(road)) return false;
            }
        }
        foreach (var bld in FindObjectsOfType<Building>())
        {
            if (box.Intersects(BoxOf(bld.transform))) return false;
        }
        return true;
    }

    void DrawPreview(Vector3 center, Vector3 dir, Vector3 perp, bool valid)
    {
        Color line = valid ? Color.green : Color.red;
        Color fill = valid ? new Color(0f, 1f, 0f, 0.2f) : new Color(1f, 0f, 0f, 0.2f);

        Vector3 right = dir * (HouseFootprint.x * 0.5f);
        Vector3 fwd = perp * (HouseFootprint.y * 0.5f);
        Vector3 g = new Vector3(center.x, 0.02f, center.z);
        Vector3[] verts =
        {
            g - right - fwd,
            g + right - fwd,
            g + right + fwd,
            g - right + fwd
        };
        Handles.DrawSolidRectangleWithOutline(verts, fill, line);

        float h = HouseFloorHeight;
        Matrix4x4 prev = Handles.matrix;
        Handles.color = line;
        Handles.matrix = Matrix4x4.TRS(
            new Vector3(center.x, h * 0.5f, center.z),
            Quaternion.LookRotation(-perp, Vector3.up),
            new Vector3(HouseFootprint.x, h, HouseFootprint.y));
        Handles.DrawWireCube(Vector3.zero, Vector3.one);
        Handles.matrix = prev;
    }

    void PlaceHouseAt(Waypoint a, Waypoint b, Vector3 linkPoint, Vector3 perp)
    {
        int group = Undo.GetCurrentGroup();

        Undo.RecordObject(a, "Placer maison");
        Undo.RecordObject(b, "Placer maison");

        var linkNode = CreateWaypoint("Waypoint_Link", linkPoint);
        a.neighbors.Remove(b);
        b.neighbors.Remove(a);
        Connect(a, linkNode);
        Connect(b, linkNode);

        var accessNode = CreateWaypoint("Waypoint_Acces", linkPoint + perp * accessDistance);
        Connect(linkNode, accessNode);

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Maison";
        go.transform.position = linkPoint + perp * (accessDistance + HouseFootprint.y * 0.5f);
        go.transform.rotation = Quaternion.LookRotation(-perp, Vector3.up);
        Undo.RegisterCreatedObjectUndo(go, "Créer maison");

        var house = Undo.AddComponent<House>(go);
        house.footprint = HouseFootprint;
        house.floorHeight = HouseFloorHeight;
        house.accessWaypoint = accessNode;
        house.ApplyVisual();

        EditorUtility.SetDirty(a);
        EditorUtility.SetDirty(b);
        Undo.CollapseUndoOperations(group);
    }

    void LinkSelection()
    {
        if (!GetSelectedWaypoints(out var wps) || wps.Count < 2) return;

        foreach (var w in wps) Undo.RecordObject(w, "Lier waypoints");
        for (int i = 0; i < wps.Count; i++)
            for (int j = i + 1; j < wps.Count; j++)
                Connect(wps[i], wps[j]);
        foreach (var w in wps) EditorUtility.SetDirty(w);
    }

    void UnlinkSelection()
    {
        if (!GetSelectedWaypoints(out var wps)) return;

        foreach (var w in wps) Undo.RecordObject(w, "Délier waypoints");
        foreach (var a in wps)
            foreach (var b in wps)
                if (a != b) a.neighbors.Remove(b);
        foreach (var w in wps) EditorUtility.SetDirty(w);
    }

    void UpgradeSelection()
    {
        foreach (var go in Selection.gameObjects)
        {
            var h = go.GetComponent<House>();
            if (h == null) continue;
            Undo.RecordObject(h, "Améliorer maison");
            Undo.RecordObject(h.transform, "Améliorer maison");
            h.Upgrade();
            EditorUtility.SetDirty(h);
        }
    }

    bool GetSelectedWaypoints(out List<Waypoint> result)
    {
        result = new List<Waypoint>();
        var objs = Selection.gameObjects;
        if (objs == null || objs.Length == 0)
        {
            EditorUtility.DisplayDialog("Building Tool", "Aucun objet sélectionné.", "OK");
            return false;
        }
        foreach (var go in objs)
        {
            var wp = go.GetComponent<Waypoint>();
            if (wp == null)
            {
                EditorUtility.DisplayDialog("Building Tool",
                    "\"" + go.name + "\" n'a pas de script Waypoint. Opération annulée.", "OK");
                return false;
            }
            result.Add(wp);
        }
        return true;
    }

    int CountSelected<T>() where T : Component
    {
        int n = 0;
        foreach (var go in Selection.gameObjects)
            if (go.GetComponent<T>() != null) n++;
        return n;
    }

    Waypoint CreateWaypoint(string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        var wp = go.AddComponent<Waypoint>();
        Undo.RegisterCreatedObjectUndo(go, "Créer waypoint");
        return wp;
    }

    void Connect(Waypoint x, Waypoint y)
    {
        if (!x.neighbors.Contains(y)) x.neighbors.Add(y);
        if (!y.neighbors.Contains(x)) y.neighbors.Add(x);
    }

    static bool IsBefore(Vector3 p, Vector3 q)
    {
        if (p.x != q.x) return p.x < q.x;
        return p.z < q.z;
    }

    static Vector3 ClosestOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float denom = Vector3.Dot(ab, ab);
        if (denom < 1e-6f) return a;
        float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / denom);
        return a + ab * t;
    }

    static OBB2 BoxOf(Transform t)
    {
        return new OBB2(
            new Vector2(t.position.x, t.position.z),
            new Vector2(t.right.x, t.right.z),
            new Vector2(t.forward.x, t.forward.z),
            Mathf.Abs(t.lossyScale.x) * 0.5f,
            Mathf.Abs(t.lossyScale.z) * 0.5f);
    }

    struct OBB2
    {
        Vector2 c, ax, ay;
        float hx, hy;

        public OBB2(Vector2 center, Vector2 axisX, Vector2 axisY, float halfX, float halfY)
        {
            c = center;
            ax = axisX.sqrMagnitude > 1e-6f ? axisX.normalized : Vector2.right;
            ay = axisY.sqrMagnitude > 1e-6f ? axisY.normalized : Vector2.up;
            hx = halfX;
            hy = halfY;
        }

        public bool Contains(Vector2 p)
        {
            Vector2 d = p - c;
            return Mathf.Abs(Vector2.Dot(d, ax)) <= hx && Mathf.Abs(Vector2.Dot(d, ay)) <= hy;
        }

        public bool IntersectsSegment(Vector2 p, Vector2 q)
        {
            Vector2 lp = ToLocal(p);
            Vector2 lq = ToLocal(q);
            return SegAabb(lp, lq, hx, hy);
        }

        public bool Intersects(OBB2 o)
        {
            Vector2[] axes = { ax, ay, o.ax, o.ay };
            foreach (var n in axes)
            {
                float ra = hx * Mathf.Abs(Vector2.Dot(ax, n)) + hy * Mathf.Abs(Vector2.Dot(ay, n));
                float rb = o.hx * Mathf.Abs(Vector2.Dot(o.ax, n)) + o.hy * Mathf.Abs(Vector2.Dot(o.ay, n));
                float dist = Mathf.Abs(Vector2.Dot(o.c - c, n));
                if (dist > ra + rb) return false;
            }
            return true;
        }

        Vector2 ToLocal(Vector2 p)
        {
            Vector2 d = p - c;
            return new Vector2(Vector2.Dot(d, ax), Vector2.Dot(d, ay));
        }

        static bool SegAabb(Vector2 a, Vector2 b, float hx, float hy)
        {
            float tmin = 0f, tmax = 1f;
            Vector2 d = b - a;
            if (!Slab(a.x, d.x, -hx, hx, ref tmin, ref tmax)) return false;
            if (!Slab(a.y, d.y, -hy, hy, ref tmin, ref tmax)) return false;
            return true;
        }

        static bool Slab(float start, float dir, float min, float max, ref float tmin, ref float tmax)
        {
            if (Mathf.Abs(dir) < 1e-6f) return start >= min && start <= max;
            float t1 = (min - start) / dir;
            float t2 = (max - start) / dir;
            if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
            tmin = Mathf.Max(tmin, t1);
            tmax = Mathf.Min(tmax, t2);
            return tmin <= tmax;
        }
    }
}