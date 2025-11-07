using UnityEngine;

/// <summary>
/// Sets environment variables for NDI before anything else runs.
/// Place this script in your first-loaded scene.
/// </summary>
public class NDIBootstrap : MonoBehaviour
{
    [Header("NDI Discovery Configuration")]
    [Tooltip("IP of the machine running NDI Discovery Server (e.g., your PC). Leave empty to disable.")]
    public string discoveryServerIp = "192.168.1.10";

    [Tooltip("Enable verbose NDI debug logging.")]
    public bool enableDebugLogging = false;

    [Tooltip("Optional: force IPv4 or IPv6 (leave empty for default).")]
    public string ndiNetworkAdapter = ""; // e.g. "ipv4" or "ipv6"

    [Tooltip("Optional: specify NDI recv queue depth (default: 4).")]
    public int recvQueueDepth = 4;

    void Awake()
    {
        ApplyConfiguration();
        DontDestroyOnLoad(this.gameObject);
    }

    void ApplyConfiguration()
    {
        if (!string.IsNullOrEmpty(discoveryServerIp))
        {
            System.Environment.SetEnvironmentVariable("NDI_DISCOVERY_SERVER", discoveryServerIp);
            Debug.Log($"[NDI] Discovery Server set to {discoveryServerIp}");
        }

        if (enableDebugLogging)
        {
            System.Environment.SetEnvironmentVariable("NDI_DEBUG", "true");
            Debug.Log("[NDI] Debug logging enabled");
        }

        if (!string.IsNullOrEmpty(ndiNetworkAdapter))
        {
            System.Environment.SetEnvironmentVariable("NDI_NETWORK_INTERFACES", ndiNetworkAdapter);
            Debug.Log($"[NDI] Network interface forced to {ndiNetworkAdapter}");
        }

        System.Environment.SetEnvironmentVariable("NDI_RECV_QUEUE_DEPTH", recvQueueDepth.ToString());
        Debug.Log($"[NDI] Receive queue depth set to {recvQueueDepth}");
    }
}
