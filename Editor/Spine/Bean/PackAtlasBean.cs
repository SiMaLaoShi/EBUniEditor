using System.Collections.Generic;

namespace EBA.Ebunieditor.Editor.Spine.Bean
{
    public class PackAtlasBean
    {
        public bool stripWhitespaceX { get; set; } = true;

        public bool stripWhitespaceY { get; set; } = true;

        public bool rotation { get; set; } = true;

        public bool alias { get; set; } = true;

        public bool ignoreBlankImages { get; set; } = false;

        public int alphaThreshold { get; set; } = 3;

        public int minWidth { get; set; } = 16;

        public int minHeight { get; set; } = 16;

        public int maxWidth { get; set; } = 2048;

        public int maxHeight { get; set; } = 2048;

        public bool pot { get; set; } = false;

        public bool multipleOfFour { get; set; } = false;

        public bool square { get; set; } = false;

        public string outputFormat { get; set; } = "png";

        public double jpegQuality { get; set; } = 0.9;

        public bool premultiplyAlpha { get; set; } = true;

        public bool bleed { get; set; } = false;

        public List<int> scale { get; set; } = new List<int>() {1};

        public List<string> scaleSuffix { get; set; } = new List<string>() {""};

        public List<string> scaleResampling { get; set; } = new List<string>() {"bicubic"};

        public int paddingX { get; set; } = 2;

        public int paddingY { get; set; } = 2;

        public bool edgePadding { get; set; } = true;

        public bool duplicatePadding { get; set; } = false;

        public string filterMin { get; set; } = "Linear";

        public string filterMag { get; set; } = "Linear";

        public string wrapX { get; set; } = "ClampToEdge";

        public string wrapY { get; set; } = "ClampToEdge";

        public string format { get; set; } = "RGBA8888";

        public string atlasExtension { get; set; } = ".atlas.txt";

        public bool combineSubdirectories { get; set; } = false;

        public bool flattenPaths { get; set; } = false;

        public bool useIndexes { get; set; } = false;

        public bool debug { get; set; } = false;

        public bool fast { get; set; } = false;

        public bool limitMemory { get; set; } = true;

        public bool currentProject { get; set; } = true;

        public string packing { get; set; } = "rectangles";

        public bool prettyPrint { get; set; } = false;

        public bool legacyOutput { get; set; } = false;

        public string webp { get; set; } = null;

        public int bleedIterations { get; set; } = 3;

        public bool ignore { get; set; } = false;

        public string separator { get; set; } = "_";

        public bool silent { get; set; } = true;
    }
}