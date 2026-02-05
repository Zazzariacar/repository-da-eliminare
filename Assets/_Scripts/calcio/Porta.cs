using System.Collections;
using UnityEngine;

public class Porta : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)

    {
        if (other.gameObject.CompareTag("palla"))
        {
            Debug.Log("Porta: Qualcuno è entrato nel trigger della porta.");
            SockerManager.Instance.Gol(1);

            StartCoroutine(PallaResetWait());
        }
    }
    IEnumerator PallaResetWait()
        {
        yield return new WaitForSeconds(1f);
        SockerManager.Instance.ResetPalla();

    }
}
