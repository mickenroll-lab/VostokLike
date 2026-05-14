using UnityEngine;

public class Campfire : MonoBehaviour
{
    public void Interact()
    {
        if (SleepManager.Instance != null)
            SleepManager.Instance.Sleep();
    }
}
