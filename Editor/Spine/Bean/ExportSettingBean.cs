namespace EBA.Ebunieditor.Editor.Spine.Bean
{
    public class ExportSettingBean
    {
        public string @class { get; set; } = "export-json";
        
        public string extension { get; set; } = ".json";
        
        public string format { get; set; } = "JSON";

        public bool prettyPrint { get; set; } = false;

        public bool nonessential { get; set; } = true;

        public bool cleanUp { get; set; } = true;

        public PackAtlasBean packAtlas { get; set; } = null;


        public string packSource { get; set; } = "attachments";
        
        public string packTarget { get; set; } = "perskeleton";

        public bool warnings { get; set; } = true;
        
        public string version { get; set; } = null;
        
        public string output { get; set; } = "";

        public bool forceAll { get; set; } = false;

        public string input { get; set; } = "";

        public bool open { get; set; } = false;
    }
}