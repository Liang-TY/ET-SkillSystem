// 0043：执行 ET/Skill/Compile 菜单（dotnet build 技能内容 → ET.SkillContent.dll.bytes）
// 若本片段格式不被 eval_file 支持，按 task notes 降级执行 MenuItemExecute
UnityEditor.EditorApplication.ExecuteMenuItem("ET/Skill/Compile");
return "menu executed: ET/Skill/Compile";
