namespace EBA.Ebunieditor.Editor.Spine.Bean
{
    public class ExportGifSettingBean
    {
        public string @class { get; set; } = "export-gif";
        public string exportType { get; set; } = "animation";
        public string skeletonType { get; set; } = "single";
        public string skeleton { get; set; } = "";
        public string animationType { get; set; } = "all";
        public string animation { get; set; } = null;
        public string skinType { get; set; } = "current";
        public bool skinNone { get; set; } = false;
        public string skin { get; set; } = null;
        public bool maxBounds { get; set; } = false;
        public bool renderImages { get; set; } = true;
        public bool renderBones { get; set; } = false;
        public bool renderOthers { get; set; } = false;
        public int scale { get; set; } = 100;
        public int fitWidth { get; set; } = 0;
        public int fitHeight { get; set; } = 0;
        public bool enlarge { get; set; } = false;
        public string background { get; set; } = null;
        public int fps { get; set; } = 20;
        public bool lastFrame { get; set; } = false;
        public int cropWidth { get; set; } = 0;
        public int cropHeight { get; set; } = 0;
        public int rangeStart { get; set; } = -1;
        public int rangeEnd { get; set; } = -1;
        public string outputType { get; set; } = "singleFile";
        public int animationRepeat { get; set; } = 1;
        public int animationPause { get; set; } = 0;
        public int colors { get; set; } = 256;
        public int colorDither { get; set; } = 50;
        public int alphaThreshold { get; set; } = 0;
        public int alphaDither { get; set; } = 40;
        public int quality { get; set; } = 100;
        public bool transparency { get; set; } = true;
        public int repeat { get; set; } = 0;
        public bool pad { get; set; } = false;
        public int msaa { get; set; } = 0;
        public int smoothing { get; set; } = 0;
        public string alphaDitherType { get; set; } = "diffusion";
        public string alphaDitherOption { get; set; } = "expand";
        public bool renderSelection { get; set; } = false;
        public int cropX { get; set; } = 0;
        public int cropY { get; set; } = 0;
        public int speed { get; set; } = 4;
        public string output { get; set; } = "";
        public string input { get; set; } = "";
        public bool open { get; set; } = false;
    }
}