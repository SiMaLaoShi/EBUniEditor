namespace EBA.Ebunieditor.Editor.Common
{
    public class MenuKey
    {
        public const string DEVELOPER_MODE = "EB/Developer Mode";
        public const string EMMYLUA_ENABLE = "EB/Lua/EmmyLua/Enable";
        public const string EMMYLUA_DISABLE = "EB/Lua/EmmyLua/Disable";

#if UNITY_2017_1_OR_NEWER
        //ugui
        public const string UGUI_MOVE_UP = "EB/UGUI/Move/MoveUp %i";
        public const string UGUI_MOVE_DOWN = "EB/UGUI/Move/MoveDown %k";
        public const string UGUI_MOVE_LEFT = "EB/UGUI/Move/MoveLeft %j";
        public const string UGUI_MOVE_RIGHT = "EB/UGUI/Move/MoveRight %l";
#else
        public const string UGUI_MOVE_UP = "GameObject/Move/MoveUp &UP";
        public const string UGUI_MOVE_DOWN = "GameObject/Move/MoveDown &DOWN";
        public const string UGUI_MOVE_LEFT = "GameObject/Move/MoveLeft &LEFT";
        public const string UGUI_MOVE_RIGHT = "GameObject/Move/MoveRight &RIGHT";
#endif
        
        
        
    }
}