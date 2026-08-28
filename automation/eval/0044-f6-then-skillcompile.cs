// 0044：先 F6（ET/Loader/Compile 重建热更 DLL）再 ET/Skill/Compile（内容 DLL）
UnityEditor.EditorApplication.ExecuteMenuItem("ET/Loader/Compile");
UnityEditor.EditorApplication.ExecuteMenuItem("ET/Skill/Compile");
return "menu executed: ET/Loader/Compile + ET/Skill/Compile";
