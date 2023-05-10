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

        if (Input.GetKeyDown(KeyCode.Space)) GenerateNewRandomColor();

        transform.position = Position.Value;
        gameObject.GetComponent<SpriteRenderer>().color = GameManager.Instance.colors[ColorIndex.Value];
    }

    void Initialize() {
        if (NetworkManager.Singleton.IsServer) {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
            
            ColorIndex.Value = Random.Range(0, GameManager.Instance.colors.Count);
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
        ColorIndex.Value = SetUnrepeatedRandomColor();      
    }

    private static Vector3 GetRandomPositionOnPlane() {
        return new Vector2(Random.Range(-7f, 7f), Random.Range(-3f, 3f));
    }

    private int SetUnrepeatedRandomColor() {
        bool isRepeated = false;
        int randomIndexColor;

        do {
            randomIndexColor = Random.Range(0, GameManager.Instance.colors.Count);
            foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds) {
                isRepeated = false;
                if (randomIndexColor == NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<Player>().ColorIndex.Value) {
                    isRepeated = true;
                    break;
                }
            }    
        } while (isRepeated);

        return randomIndexColor;
    }

    private void GenerateNewRandomColor() {
        if (!IsOwner) return;

        if (NetworkManager.Singleton.IsServer) ColorIndex.Value = SetUnrepeatedRandomColor();
        else SubmitRandomColorRequestServerRpc();
    }

}
