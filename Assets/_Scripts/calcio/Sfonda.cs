using UnityEngine;

public class Sfonda : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("palla")){
            SockerManager.Instance?.ResetPalla();
        }

    }
}
