using UnityEngine;

public class TestTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{name} overlapped with {other.name}");
    }
}