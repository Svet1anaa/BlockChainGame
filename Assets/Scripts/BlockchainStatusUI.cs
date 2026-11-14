
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlockchainStatusUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button retryButton;

    private void Update()
    {
        BlockchainManager manager = BlockchainManager.Instance;

        if (manager == null)
        {
            if (statusText != null)
            {
                statusText.text = "Blockchain: Not connected";
                statusText.color = Color.gray;
            }

            if (retryButton != null)
                retryButton.gameObject.SetActive(false);

            return;
        }

        if (statusText != null)
        {
            statusText.text = manager.StatusMessage;

            switch (manager.CurrentStatus)
            {
                case BlockchainManager.ConnectionStatus.Connecting:
                    statusText.color = Color.yellow;
                    break;

                case BlockchainManager.ConnectionStatus.Ready:
                    statusText.color = Color.green;
                    break;

                case BlockchainManager.ConnectionStatus.Error:
                    statusText.color = Color.red;
                    break;

                default:
                    statusText.color = Color.gray;
                    break;
            }
        }

        if (retryButton != null)
        {
            bool showRetry =
                manager.CurrentStatus ==
                BlockchainManager.ConnectionStatus.Error;

            if (retryButton.gameObject.activeSelf != showRetry)
            {
                retryButton.gameObject.SetActive(showRetry);
            }
        }
    }
}
