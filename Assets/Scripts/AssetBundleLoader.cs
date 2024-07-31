using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetBundleLoader : MonoBehaviour
{
    public string baseDirectory;  // Base directory where Asset Bundles are stored
    public ConfigLoader configLoader;
    public Dictionary<string, GameObject> loadedModels = new Dictionary<string, GameObject>();
    private List<AssetBundle> loadedAssetBundles = new List<AssetBundle>(); // Track loaded AssetBundles

    IEnumerator Start()
    {
        string[] tagDirectories = Directory.GetDirectories(baseDirectory);

        foreach (string tagDirectory in tagDirectories)
        {
            string[] bundleFiles = Directory.GetFiles(tagDirectory);

            foreach (string bundleFile in bundleFiles)
            {
                if (Path.GetExtension(bundleFile) == "" && !bundleFile.EndsWith(".manifest"))
                {
                    AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundleFile);
                    yield return bundleRequest;

                    AssetBundle bundle = bundleRequest.assetBundle;

                    if (bundle != null)
                    {
                        // Load all assets from the bundle
                        AssetBundleRequest assetRequest = bundle.LoadAllAssetsAsync();
                        yield return assetRequest;

                        if (assetRequest.allAssets == null)
                        {
                            Debug.LogWarning($"No assets found in AssetBundle '{bundleFile}'.");
                            continue;
                        }

                        foreach (Object asset in assetRequest.allAssets)
                        {
                            if (asset is GameObject model)
                            {
                                // Store the loaded model for later use
                                loadedModels[model.name] = model;
                                Debug.Log($"Successfully loaded model: {model.name}");
                            }
                            else
                            {
                                Debug.Log($"Asset '{asset.name}' is not a GameObject, skipping.");
                            }
                        }

                        loadedAssetBundles.Add(bundle); // Keep a reference to the loaded bundle
                    }
                    else
                    {
                        Debug.LogError($"Failed to load Asset Bundle from {bundleFile}!");
                    }
                }
            }
        }
        configLoader.LoadConfig();
    }

    private void OnDestroy()
    {
        // Unload all loaded AssetBundles when done
        foreach (var bundle in loadedAssetBundles)
        {
            bundle.Unload(false); // Unload the bundle but keep loaded assets
        }
        loadedAssetBundles.Clear();
    }
}
