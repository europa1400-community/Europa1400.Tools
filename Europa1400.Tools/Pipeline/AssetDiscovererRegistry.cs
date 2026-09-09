using System;
using System.Collections.Generic;
using Europa1400.Tools.Pipeline.Assets;
using Europa1400.Tools.Pipeline.Discoverer;

namespace Europa1400.Tools.Pipeline
{
    public static class AssetDiscovererRegistry
    {
        private static readonly Dictionary<Type, object> Discoverers = new Dictionary<Type, object>();

        static AssetDiscovererRegistry()
        {
            Register(new AgebAssetDiscoverer());
            Register(new AobjAssetDiscoverer());
            Register(new SbfAssetDiscoverer());
            Register(new BgfAssetDiscoverer());
            Register(new GfxAssetDiscoverer());
            Register(new GlobAssetDiscoverer<TxsAsset>("*.txs", "TXS", (path, relative) => new TxsAsset(path, relative)));
            Register(new BafAssetDiscoverer());
            Register(new GlobAssetDiscoverer<BafIniAsset>("*.bafini", "BAFINI",
                (path, relative) => new BafIniAsset(path, relative)));
            Register(new GlobAssetDiscoverer<OgrAsset>("*.ogr", "OGR", (path, relative) => new OgrAsset(path, relative)));
            Register(new GlobAssetDiscoverer<Ed3Asset>("*.ed3", "ED3", (path, relative) => new Ed3Asset(path, relative)));
            Register(new GlobAssetDiscoverer<ResAsset>("*.res", "RES", (path, relative) => new ResAsset(path, relative)));
            Register(new GlobAssetDiscoverer<FormAsset>("*.form", "FORM",
                (path, relative) => new FormAsset(path, relative)));
            Register(new GlobAssetDiscoverer<ScrAsset>("*.scr", "SCR", (path, relative) => new ScrAsset(path, relative)));
            Register(new GlobAssetDiscoverer<SavAsset>("*.sav", "SAV", (path, relative) => new SavAsset(path, relative)));
            Register(new DummyAssetDiscoverer());
        }

        public static void Register<TAsset>(IAssetDiscoverer<TAsset> discoverer) where TAsset : GameAsset
        {
            Discoverers[typeof(TAsset)] = discoverer;
        }

        public static IAssetDiscoverer<TAsset> Get<TAsset>() where TAsset : GameAsset
        {
            if (Discoverers.TryGetValue(typeof(TAsset), out var d))
                return (IAssetDiscoverer<TAsset>)d;

            throw new InvalidOperationException($"No discoverer registered for asset type {typeof(TAsset).Name}");
        }
    }
}