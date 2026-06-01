using System.Collections.Generic;
using UnityEngine;
 
// Un point de route posé à la main dans la scène.
// On relie ses voisins par glisser-déposer dans l'inspecteur (champ "neighbors").
public class Waypoint : MonoBehaviour
{
    public List<Waypoint> neighbors = new List<Waypoint>();
 
    // Affiche le point et ses connexions dans la vue Scène (aide au placement manuel).
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.25f);
 
        Gizmos.color = Color.cyan;
        if (neighbors == null) return;
        foreach (var n in neighbors)
        {
            if (n != null)
                Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }
}