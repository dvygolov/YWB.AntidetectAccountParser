namespace YWB.IndigoAccountParser.Model
{
    public class IndigoProfileSettings
    {
        public Container container { get; set; }
        public string groupId { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
        public int browserType { get; set; }
        public int browserTypeVersion { get; set; }
        public string proxyHost { get; set; }
        public int proxyPort { get; set; }
        public string proxyUser { get; set; }
        public string proxyPass { get; set; }
        public bool proxyIpValidation { get; set; }
        public int proxyType { get; set; }
        public string keyData { get; set; }
        public bool disablePlugins { get; set; }
        public bool maskWebRtc { get; set; }
        public int webRtcType { get; set; }
        public bool webrtcPubIpFillOnStart { get; set; }
        public bool webrtcLocalIpInReal { get; set; }
        public bool disableFlashPlugin { get; set; }
        public bool loadCustomExtensions { get; set; }
        public bool storeExtensions { get; set; }
        public bool storeLs { get; set; }
        public bool storePasswords { get; set; }
        public bool storeHistory { get; set; }
        public bool storeBookmarks { get; set; }
        public bool storeServiceWorkerCache { get; set; }
        public string customExtensionFileNames { get; set; }
        public bool useCanvasNoise { get; set; }
        public bool useFingerprintsShadow { get; set; }
        public bool shared { get; set; }
        public bool forbidConcurrentExecution { get; set; }
        public bool useAudioNoise { get; set; }
        public bool useWebglNoise { get; set; }
        public bool webglCnv { get; set; }
        public bool webglMeta { get; set; }
        public bool maskFonts { get; set; }
        public string[] fonts { get; set; }
        public bool maskFontGlyphs { get; set; }
        public bool useGeoSpoofing { get; set; }
        public int geoPermitType { get; set; }
        public bool geoFillOnStart { get; set; }
        public bool maskMediaDevices { get; set; }
        public int mediaDevicesAudioInputs { get; set; }
        public int mediaDevicesAudioOutputs { get; set; }
        public int mediaDevicesVideoInputs { get; set; }
        public string osType { get; set; }
        public bool tzFillOnStart { get; set; }
        public bool googleServices { get; set; }
        public bool localPortsProtection { get; set; }
        public string sid { get; set; }
        public bool offlineProfile { get; set; }
    }

    public class Container
    {
        public string audioNoiseHash { get; set; }
        public string fontsHash { get; set; }
        public string webglNoiseHash { get; set; }
        public string webGlVendor { get; set; }
        public string webGlRenderer { get; set; }
        public Webglparam[] webGlParams { get; set; }
        public Webglparam[] webGl2Params { get; set; }
        public int scrWidth { get; set; }
        public int scrHeight { get; set; }
        public IndigoNavigator navigator { get; set; }
        public string navUserAgent { get; set; }
        public string webRtcLocalIps { get; set; }
    }

    public class Webglparam
    {
        public string name { get; set; }
        public int code { get; set; }
        public string ext { get; set; }
        public object value { get; set; }
    }

    public class IndigoNavigator
    {
        public string platform { get; set; }
        public int hardwareConcurrency { get; set; }
        public int maxTouchPoints { get; set; }
        public string langHdr { get; set; }
    }
}
