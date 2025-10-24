using UnityEngine;
using UnityEngine.AI;

public class GenerateNavmesh : MonoBehaviour
{
    [Tooltip("NavMeshSurface to build at runtime")]
    public NavMeshSurface surface;

    void Awake()
    {
        // Auto-find if not assigned
        if (surface == null)
            surface = GetComponent<NavMeshSurface>() ?? FindObjectOfType<NavMeshSurface>();
    }

    public void GenerateNavMesh()
    {
        if (surface == null)
        {
            Debug.LogError("[GenerateNavmesh] No NavMeshSurface assigned or found in scene.");
            return;
        }
        surface.BuildNavMesh();
        Debug.Log("[GenerateNavmesh] NavMesh built.");
    }
}
