using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnPointId;

    public string SpawnPointId
    {
        get { return spawnPointId; }
    }
}