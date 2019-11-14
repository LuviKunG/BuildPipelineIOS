using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LuviKunG.BuildPipeline.iOS
{
    public sealed class BuildPipelineIOSWindow : EditorWindow
    {
        private const string HELPBOX_NAME_FORMATTING_INFO = @"How to format the file name.

{name} = App Name.
{package} = Android Package Name.
{version} = App Version.
{bundle} = App Bundle.
{date} = Date time. (format)";
        private static readonly GUIContent CONTENT_LIST_URL_SCHEMES_HEADER = new GUIContent("URLSchemes");

        private BuildPipelineIOSSettings settings;
        private ReorderableList listMain;
        private List<string> urlSchemeList;
        private bool isDirty = false;

        public static BuildPipelineIOSWindow OpenWindow()
        {
            var window = GetWindow<BuildPipelineIOSWindow>(true, "WebGL Build Pipeline Setting", true);
            window.Show();
            return window;
        }

        private void OnEnable()
        {
            isDirty = false;
            settings = BuildPipelineIOSSettings.Instance;
            urlSchemeList = new List<string>();
            listMain = new ReorderableList(urlSchemeList, typeof(string), true, true, true, true);
            listMain.drawHeaderCallback = DrawHeader;
            listMain.drawElementCallback = DrawElement;
            listMain.elementHeightCallback = ElementHeight;
            listMain.onAddCallback = OnAdd;
            listMain.onRemoveCallback = OnRemove;
            listMain.onReorderCallback = OnReorder;
            //local function.
            void DrawHeader(Rect rect)
            {
                EditorGUI.LabelField(rect, CONTENT_LIST_URL_SCHEMES_HEADER);
            }
            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                urlSchemeList[index] = EditorGUI.TextField(rect, GUIContent.none, urlSchemeList[index]);
            }
            float ElementHeight(int index)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            void OnAdd(ReorderableList list)
            {
                urlSchemeList.Add(string.Empty);
                isDirty = true;
            }
            void OnRemove(ReorderableList list)
            {
                if (list.index < 0)
                    return;
                urlSchemeList.RemoveAt(list.index);
                isDirty = true;
            }
            void OnReorder(ReorderableList list)
            {
                isDirty = true;
            }
        }

        private void OnGUI()
        {
            GUI.enabled = !UnityEditor.BuildPipeline.isBuildingPlayer;
            EditorGUILayout.LabelField("Folder name formatting", EditorStyles.boldLabel);
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                settings.nameFormat = EditorGUILayout.TextField(settings.nameFormat);
                if (changeScope.changed)
                    settings.Save();
            }
            EditorGUILayout.HelpBox(HELPBOX_NAME_FORMATTING_INFO, MessageType.Info, true);
            EditorGUILayout.LabelField("Formatted name", settings.GetFolderName(), EditorStyles.helpBox);
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                settings.dateTimeFormat = EditorGUILayout.TextField("Date time format", settings.dateTimeFormat);
                settings.createNewFolder = EditorGUILayout.Toggle("Create new folder", settings.createNewFolder);
                settings.setURLSchemes = EditorGUILayout.Toggle("Set URLSchemes", settings.setURLSchemes);
                if (settings.setURLSchemes)
                {
                    if (settings.urlSchemes == null)
                        settings.urlSchemes = new string[0];
                    urlSchemeList.Clear();
                    urlSchemeList.AddRange(settings.urlSchemes);
                    listMain.DoLayoutList();
                }
                settings.buildOptions = (BuildOptions)EditorGUILayout.EnumFlagsField("Build options", settings.buildOptions);
                if (changeScope.changed || isDirty)
                {
                    isDirty = false;
                    if (settings.setURLSchemes)
                        settings.urlSchemes = urlSchemeList.ToArray();
                    settings.Save();
                }
            }
            using (var verticalScope = new EditorGUILayout.VerticalScope())
            {
                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Build location", string.IsNullOrWhiteSpace(settings.buildPath) ? "<No location>" : settings.buildPath);
                    if (GUILayout.Button("Change...", GUILayout.Width(96.0f)))
                    {
                        var path = OpenBuildSavePanel(settings.buildPath);
                        if (!string.IsNullOrEmpty(path))
                            settings.buildPath = path;
                    }
                }
                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    bool cacheEnable = GUI.enabled;
                    GUI.enabled = !string.IsNullOrWhiteSpace(settings.buildPath);
                    if (GUILayout.Button("Open Build Location", GUILayout.MaxWidth(256.0f)))
                    {
                        Application.OpenURL(settings.buildPath);
                    }
                    GUI.enabled = cacheEnable;
                    GUILayout.FlexibleSpace();
                }
            }
        }

        private string OpenBuildSavePanel(string path)
        {
            string newPath = EditorUtility.SaveFolderPanel("Choose Location of Build Game", path, null);
            if (string.IsNullOrEmpty(newPath))
                return null;
            settings.buildPath = newPath;
            settings.Save();
            return newPath;
        }
    }
}