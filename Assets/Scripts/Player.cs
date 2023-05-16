using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour {

    NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    NetworkVariable<int> ColorIndex = new NetworkVariable<int>();

    public override void OnNetworkSpawn() {
        ColorIndex.OnValueChanged += OnColorChanged;
        Position.OnValueChanged += OnPositionChanged;

        if (IsOwner) Initialize();
        else gameObject.GetComponent<SpriteRenderer>().color = GameManager.Instance.colors[ColorIndex.Value];
    }

    public override void OnNetworkDespawn() {
        ColorIndex.OnValueChanged -= OnColorChanged;
        Position.OnValueChanged -= OnPositionChanged;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space) && IsOwner) SubmitRandomColorRequestServerRpc();
    }

    void Initialize() {
        SubmitRandomPositionRequestServerRpc(); // Para colocar inicialmente cada Player en un lugar distinto
        SubmitRandomColorRequestServerRpc();
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


    // Callbacks OnValueChanged
    private void OnColorChanged(int previous, int current) {
        gameObject.GetComponent<SpriteRenderer>().color = GameManager.Instance.colors[ColorIndex.Value];
    }

    private void OnPositionChanged(Vector3 previous, Vector3 current) {
        Debug.Log($"Testing change: {previous} | {current}");
        transform.position = Position.Value;
    }

}
