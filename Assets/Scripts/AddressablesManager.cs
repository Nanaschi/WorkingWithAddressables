using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;


[Serializable]
public class AssetReferenceAudioClip : AssetReferenceT<AudioClip> //TODO here as a Generic you should pass required file types 
{
    public AssetReferenceAudioClip(string guid) : base(guid)
    {
    }
}

public class AddressablesManager : MonoBehaviour
{
    [SerializeField] private AssetReference _musicAssetReference;
    [SerializeField] private AssetReferenceTexture2D _logoAssetReference; //this is done because this field exists (opposite to AssetReferenceAudioClip)
    [SerializeField] private AssetReference _playerArmatureAssetReference;
    
    

    private GameObject playerController; //we do a global field to create a reference that we can free OnDestroy (or probably in IDisposable)

    [SerializeField] private RawImage rawImageLogo;
    [SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;

    private void Start()
    {
        
        //It would be done automatically but this gives you more control over resource control
        Addressables.InitializeAsync().Completed += AddressablesManager_Completed; 
    }

    private void AddressablesManager_Completed(AsyncOperationHandle<IResourceLocator> obj)
    {
        //it means when the player is instantiated we can keep it as a reference in the variable go
        _playerArmatureAssetReference.InstantiateAsync().Completed += (go) =>
        {
            playerController = go.Result; //making the AsyncOperationHandler as a GameObject
            _cinemachineVirtualCamera.Follow = playerController.transform.Find("PlayerCameraRoot");
        };
//Just LoadAsset is obsolete cause the async one can load things multiple times without unloading
        _musicAssetReference.LoadAssetAsync<AudioClip>().Completed += (clip) =>
        {
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip.Result; //Again we use the Result to get the AudioClip paased as the parameter to the LoadAssetAsync instead of AsyncOperationHandle
            audioSource.playOnAwake = false; 
            audioSource.loop = true; 
            audioSource.Play();
        };
        _logoAssetReference.LoadAssetAsync<Texture2D>(); //We are not taking the RawImage here cause Texture2D is what we are associating it with

    }

    private void Update()
    {
        if (_logoAssetReference.Asset != null && rawImageLogo.texture == null)
        {
            rawImageLogo.texture = _logoAssetReference.Asset as Texture2D;
            var currentColor = rawImageLogo.color;
            currentColor.a = 1.0f;
            rawImageLogo.color = currentColor;
        }
        
        
        //There are two ways of loading the Addressable
        //1) it is whether to generate a Method with a lambda parameter of the LoadAssetAsync<>. In this Lambda method you also include the logic of the addressed thing???
        //2) In the update you check whether the needed Reference is loaded or not + any other condition to check 
    }

    private void OnDestroy()
    {
        _playerArmatureAssetReference.ReleaseInstance(playerController); //This will release it Reference 
        _logoAssetReference.ReleaseAsset(); //Release the Asset
    }
}
