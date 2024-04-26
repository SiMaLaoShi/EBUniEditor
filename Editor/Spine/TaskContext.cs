using System.Collections.Generic;
using System.IO;
using EBA.Ebunieditor.Editor.Spine.Bean;

namespace EBA.Ebunieditor.Editor.Spine
{
    public class TaskContext
    {
        /// <summary>
        /// Spine的程序路径
        /// </summary>
        public const string SPINE_EXE = @"C:\Program Files\Spine\Spine.exe";
        

        /// <summary>
        /// 存放bat的路径
        /// </summary>
        public static string s_sBatRootPath = Path.Combine(System.Environment.CurrentDirectory, "bats");

        /// <summary>
        /// 解包的bat路径
        /// </summary>
        public static string s_sUnpackTaskPath = Path.Combine(s_sBatRootPath, "UnpackTask.bat");
        
        /// <summary>
        /// 导入数据的bat路径
        /// </summary>
        public static string s_sImportDataTaskPath = Path.Combine(s_sBatRootPath, "ImportDataTask.bat");
        
        /// <summary>
        /// Spine工程升级的bat路径
        /// </summary>
        public static string s_sUpdateProjectTaskPath = Path.Combine(s_sBatRootPath, "UpdateProjectTask.bat");
        
        /// <summary>
        /// 导出Gif的任务bat路径
        /// </summary>
        public static string s_sExportGifTaskPath = Path.Combine(s_sBatRootPath, "ExportGifTask.bat");
        
        /// <summary>
        /// 导出Atlas的任务bat路径
        /// </summary>
        public static string s_sExportAtlasTaskPath = Path.Combine(s_sBatRootPath, "ExportAtlasTask.bat");
        
        /// <summary>
        /// 解包工程的Spine版本
        /// </summary>
        public string OldSpineVersion = "3.7.94";
        
        /// <summary>
        /// 升级工程的Spine版本信息
        /// </summary>
        public string NewSpineVersion = "4.1.24";

        /// <summary>
        /// 需要查找的文件夹路径
        /// </summary>
        public string SearchFolderPath = @"D:\SpineUnpack\Spine";

        /// <summary>
        /// 导出的路径
        /// </summary>
        public string OutputFolderPath = @"D:\SpineUnpack\Spine_out";

        /// <summary>
        /// 纹理存放路径
        /// </summary>
        public string TextureFolderPath = "";
        
        /// <summary>
        /// spine的数据匹配模型
        /// </summary>
        public string SpineDataSearchPattern = "*.json";

        /// <summary>
        /// spine的图集数据匹配模型
        /// </summary>
        public string SpineAtlasSearchPattern = "*.atlas.txt";

        /// <summary>
        /// spine导出数据的扩展名
        /// </summary>
        public string SpineDataExtension = ".json";

        /// <summary>
        /// 导出Gif的设置配置
        /// </summary>
        public ExportGifSettingBean ExportGifSettingBean = null;

        /// <summary>
        /// 导出的配置路径
        /// </summary>
        public ExportSettingBean ExportSettingBean = null;
        

        /// <summary>
        /// 存放查找的spine的数据集合
        /// </summary>
        public List<string> lstSpineDatas = new List<string>();

        /// <summary>
        /// 存放查找的spine图集的数据集合
        /// </summary>
        public List<string> lstSpineAtlass = new List<string>();
    }
}