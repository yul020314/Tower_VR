using UnityEngine;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;
    
    [SerializeField] 
    private List<Transform> waypoints = new List<Transform>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public Transform GetWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Count)
        {
            return waypoints[index];
        }
        return null;
    }
    
    // 在场景中添加路径点
    public void AddWaypoint(Transform waypoint)
    {
        waypoints.Add(waypoint);
    }
}