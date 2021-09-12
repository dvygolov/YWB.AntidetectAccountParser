namespace YWB.AntidetectAccountParser.Model.Indigo
{
    public class IndigoPlanSettings
    {
        public string Uid { get; set; }
        public string Email { get; set; }
        public bool Expired { get; set; }
        public int ProfilesCount { get; set; }
        public bool ProxyTunnel { get; set; }
        public bool CanvasShadow { get; set; }
        public bool ZeroFingerPrints { get; set; }
        public bool DisablePlugins { get; set; }
        public bool AutomatitonApi { get; set; }
        public bool RandomProfile { get; set; }
        public bool LoadCustomExtensions { get; set; }
        public bool SessionSharing { get; set; }
        public int Weight { get; set; }
        public bool SharingToSameWeight { get; set; }
        public bool SharingToLessWeight { get; set; }
        public bool SharingPermissionsFromBiggerWeight { get; set; }
        public string Name { get; set; }
        public bool TagRestrictedAccess { get; set; }
        public bool Mimic { get; set; }
        public bool Stealthfox { get; set; }
        public bool Firefox { get; set; }
        public bool Chromium { get; set; }
        public bool StoreLs { get; set; }
        public bool StoreExtensions { get; set; }
        public bool StartUrl { get; set; }
        public bool DisableBrowserNotifications { get; set; }
        public bool ProxyPlugins { get; set; }
        public bool ForceUpdate { get; set; }
        public bool ShowSubscribeOnboarding { get; set; }
        public bool ShowDemo { get; set; }
        public bool CreateProfileDisabled { get; set; }
        public bool GenerateFpDisabled { get; set; }
        public bool NaturalCanvas { get; set; }
        public string CollaborationOwnerId { get; set; }
        public bool CollaborationMember { get; set; }
        public int MaxMembers { get; set; }
        public bool HelpChat { get; set; }
        public int AccpmpState { get; set; }
        public int ClbMigrationState { get; set; }
    }
}