using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour {

    NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    NetworkVariable<int> ColorIndex = new NetworkVariable<int>();

    public override void OnNetworkSpawn() {
        // Para colocar inicialmente cada Player en un lugar distinto
        if (IsOwner) Initialize();
    }

    void Update() {
        transform.position = Position.Value;
        gameObject.GetComponent<SpriteRenderer>().color = GameManager.Instance.colors[ColorIndex.Value];
    }

    void Initialize() {
        if (NetworkManager.Singleton.IsServer) {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;

            SetRandomColor();
            gameObject.GetComponent<SpriteRenderer>().color = GameManager.Instance.colors[ColorIndex.Value];
            

        } else {
            SubmitRandomPositionRequestServerRpc();
            SubmitRandomColorRequestServerRpc();
        }
    }


    [ServerRpc]
    void SubmitRandomPositionRequestServerRpc(ServerRpcParams rpcParams = default) {
        Position.Value = GetRandomPositionOnPlane();
    }

    [ServerRpc]
    void SubmitRandomColorRequestServerRpc(ServerRpcParams rpcParams = default) {
        SetRandomColor();
    }

    private static Vector3 GetRandomPositionOnPlane() {
        return new Vector2(Random.Range(-7f, 7f), Random.Range(-3f, 3f));
    }

    private void SetRandomColor() {
        int randomMatIndex = Random.Range(0, GameManager.Instance.colors.Count);
        ColorIndex.Value = randomMatIndex;
    }

}
