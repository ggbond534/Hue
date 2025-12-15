using UnityEngine;

public class PressEDestroyMapTrigger : MonoBehaviour
{
    [Header("要删除的物体")]
    [SerializeField] private GameObject mapToDestroy;

    [Header("玩家头上提示")]
    [SerializeField] private GameObject eHintOnPlayer;

    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        if (eHintOnPlayer)
            eHintOnPlayer.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (eHintOnPlayer)
            eHintOnPlayer.SetActive(false);
    }

    private void Update()
    {
        if (!playerInside) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (mapToDestroy)
                Destroy(mapToDestroy);

            if (eHintOnPlayer)
                eHintOnPlayer.SetActive(false);

            Destroy(gameObject);
        }
    }
}