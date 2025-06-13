namespace RCRepo;

public class AssetManager
{
    private readonly AssetBundle assetBundle;

    public Dictionary<string, UnityEngine.Object> Assets { get; private set; } = [];

    public AssetManager(AssetBundle bundle)
    {
        assetBundle = bundle ?? throw new ArgumentNullException(nameof(bundle), "AssetBundle cannot be null");
    }

    public static AssetManager CreateFromBundle(AssetBundle bundle)
    {
        if (bundle == null)
        {
            throw new ArgumentNullException(nameof(bundle), "AssetBundle cannot be null");
        }
        var assetManager = new AssetManager(bundle);

        var assetNames = assetManager.assetBundle.GetAllAssetNames();
        foreach (var assetName in assetNames)
        {
            Plugin.logger.LogInfo($"Asset found: {assetName}");
            var asset = assetManager.assetBundle.LoadAsset<UnityEngine.Object>(assetName);
            if (asset == null)
            {
                Plugin.logger.LogWarning($"Failed to load asset: {assetName}");
            }
            else
            {
                var indexOfLastSlash = assetName.LastIndexOf('/');
                var newName = (assetName.Substring(indexOfLastSlash + 1, assetName.LastIndexOf('.') - indexOfLastSlash - 1) + "." + asset.GetType().Name).ToLower();
                assetManager.Assets[newName] = asset;
                Plugin.logger.LogInfo($"Loaded asset: {newName}");
            }
        }

        return assetManager;
    }

    public bool TryGetAsset<T>(string assetName, [NotNullWhen(true)] out T? asset) where T : UnityEngine.Object
    {
        var typeName = typeof(T).Name;
        var fullAssetName = (assetName + "." + typeName).ToLower();
        if (Assets.TryGetValue(fullAssetName, out var obj) && obj is T castedAsset)
        {
            asset = castedAsset;
            return true;
        }
        else
        {
            Plugin.logger.LogWarning($"Asset '{fullAssetName}' not found or is not of type {typeName}");
            asset = null;
            return false;
        }
    }
}
