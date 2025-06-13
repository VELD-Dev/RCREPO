namespace RCRepo;

[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BepInEx.BaseUnityPlugin
{
    [NotNull] internal static ManualLogSource logger { get; private set; }
    [NotNull] public static Plugin Singleton { get; private set; }
    [NotNull] public static string AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
    [NotNull] public static AssetManager AM { get; private set; }

    private void Awake()
    {
        Singleton = this;
        logger = Logger;
#if DEBUG
        gameObject.hideFlags = HideFlags.HideAndDontSave;
#endif

        logger.LogInfo($"Loading mod {PluginInfo.Name} v{PluginInfo.Version}");

        var bundlePath = Path.Combine(AssemblyPath, "ratchetvaluables");
        BundleLoader.LoadBundle(bundlePath, BundleLoadCallback, true);
        logger.LogInfo($"Loaded Bundle '{bundlePath}'");
    }

#if DEBUG
    private void Update()
    {
        if(InputManager.instance.KeyDown(InputKey.Interact))
        {
            if(AM.TryGetAsset<ValuableContent>("PocketWatchValuable", out var pocketWatchValuable))
            {
                UnityEngine.Object.Instantiate(pocketWatchValuable.Prefab, PlayerController.instance.transform.position, Quaternion.identity);
            }
            if(AM.TryGetAsset<ValuableContent>("OmniWrench3000Valuable", out var omniWrenchValuable))
            {
                UnityEngine.Object.Instantiate(omniWrenchValuable.Prefab, PlayerController.instance.transform.position + Vector3.up, Quaternion.identity);
            }
        }
    }
#endif

    IEnumerator BundleLoadCallback(AssetBundle bundle)
    {
        AM = AssetManager.CreateFromBundle(bundle);

        yield break;
    }
}
