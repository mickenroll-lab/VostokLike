using UnityEngine;

public class Campfire : MonoBehaviour
{
    public float mentalRecoveryRange = 3f;
    public float mentalRecoveryRate = 0.5f; // 1/2秒

    private PlayerState playerState;

    void Update()
    {
        if (playerState == null)
            playerState = FindObjectOfType<PlayerState>();
        if (playerState == null) return;

        float dist = Vector3.Distance(transform.position, playerState.transform.position);
        if (dist <= mentalRecoveryRange)
            playerState.mental = Mathf.Min(playerState.mental + mentalRecoveryRate * Time.deltaTime, playerState.mentalMax);
    }

    public void Interact()
    {
        if (SleepManager.Instance != null)
            SleepManager.Instance.Sleep();
    }
}
