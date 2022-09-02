using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public float speed = 5f;
    private Transform rotator_;

    private void Start() {
        rotator_ = GetComponent<Transform>();
    }

    private void Update()
    {
        rotator_.Rotate(0, speed * Time.deltaTime, 0);
    }
}
