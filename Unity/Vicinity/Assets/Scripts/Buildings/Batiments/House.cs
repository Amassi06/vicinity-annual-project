using UnityEngine;

public class House : Building
{
    public int capacityPerFloor = 10;
    public int floors = 1;
    public float floorHeight = 1.5f;
    public Vector2 footprint = new Vector2(4f, 4f);

    public int Capacity => floors * capacityPerFloor;

    public void Upgrade()
    {
        floors++;
        ApplyVisual();
    }

    public void ApplyVisual()
    {
        float h = Mathf.Max(1, floors) * floorHeight;
        transform.localScale = new Vector3(footprint.x, h, footprint.y);
        var p = transform.position;
        p.y = h * 0.5f;
        transform.position = p;
    }

    void OnValidate()
    {
        if (floors < 1) floors = 1;
        ApplyVisual();
    }
}