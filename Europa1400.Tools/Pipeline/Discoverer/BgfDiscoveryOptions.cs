namespace Europa1400.Tools.Pipeline.Discoverer
{
    public class BgfDiscoveryOptions
    {
        private BgfDiscoveryOptions()
        {
        }

        public bool GroupTextures { get; private set; }
        public bool GroupAnimations { get; private set; }

        public static BgfDiscoveryOptions Create()
        {
            return new BgfDiscoveryOptions();
        }

        public BgfDiscoveryOptions WithGroupedTextures()
        {
            GroupTextures = true;
            return this;
        }

        public BgfDiscoveryOptions WithGroupedAnimations()
        {
            GroupAnimations = true;
            return this;
        }
    }
}