using System;
using UnityEngine;

namespace Game.Multiplayer
{
    public class NetObject : MonoBehaviour
    {
        static uint nextId = 1;
        internal static uint GetNextId() => nextId++;
        public static void ResetNextId() => nextId = 1;
        [SerializeField] private string _assetId;
        [SerializeField, HideInInspector] bool isPrefab;

        public string assetId => _assetId;
        public Guid guidAssetId { get => (!_cacheAssetId.Equals(Guid.Empty)) ? _cacheAssetId : _cacheAssetId=new Guid(_assetId); }
        Guid _cacheAssetId;
        public uint id { get; set; } = 0;

        private void Awake()
        {
            if (id==0) id = GetNextId();
            Debug.Log("Awake Log id: " + id);
        }

        private void OnValidate()
        {
            //if (PrefabStageUtility.GetCurrentPrefabStage() == null) return;
            //if (PrefabStageUtility.GetPrefabStage(gameObject)  == null) return;
            if (_assetId==null || (isPrefab != Mirror.Utils.IsPrefab(gameObject)))
            { 
                Guid newAssetId = Guid.NewGuid();
                _assetId = newAssetId.ToString();
                Debug.Log($"Validate: old assetId: {_assetId} || current: {newAssetId}. isPrefab? {Mirror.Utils.IsPrefab(gameObject)}");
            } else
            {
                //Debug.Log($"Validate: no changes in assetId: {_assetId}. isPrefab? {Mirror.Utils.IsPrefab(gameObject)}");
            }
            isPrefab = Mirror.Utils.IsPrefab(gameObject);
        }
    }
}