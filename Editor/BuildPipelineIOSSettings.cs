using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LuviKunG.BuildPipeline.iOS
{
    public sealed class BuildPipelineIOSSettings
    {
        private const string ALIAS = "unity.editor.luvikung.buildpipeline.ios.";
        private static readonly string PREFS_SETTINGS_BUILD_PATH = ALIAS + "buildpath";
        private static readonly string PREFS_SETTINGS_NAME_FORMAT = ALIAS + "nameformat";
        private static readonly string PREFS_SETTINGS_DATE_TIME_FORMAT = ALIAS + "datetimeformat";
        private static readonly string PREFS_SETTINGS_CREATE_NEW_FOLDER = ALIAS + "createNewFolder";
        private static readonly string PREFS_SETTINGS_SET_URL_SCHEMES = ALIAS + "setURLSchemes";
        private static readonly string PREFS_SETTINGS_URL_SCHEMES = ALIAS + "urlSchemes";
        private static readonly string PREFS_SETTINGS_BUILD_OPTIONS = ALIAS + "buildOptions";

        public string buildPath;
        public string nameFormat;
        public string dateTimeFormat;
        public bool createNewFolder;
        public bool setURLSchemes;
        public string[] urlSchemes;
        public BuildOptions buildOptions;

        private static BuildPipelineIOSSettings instance;
        public static BuildPipelineIOSSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = new BuildPipelineIOSSettings();
                return instance;
            }
        }

        public BuildPipelineIOSSettings()
        {
            Load();
        }
        public void Load()
        {
            // Define 'BUILD_PIPELINE_IOS_UNITY_DEFAULT' if you want to set a build location as same as default of Unity Editor
            // But it's buggy because the native doesn't generate the new folder to put a build.
#if BUILD_PIPELINE_IOS_UNITY_DEFAULT
            buildPath = PlayerPrefs.GetString(PREFS_SETTINGS_BUILD_PATH, EditorUserBuildSettings.GetBuildLocation(BuildTarget.iOS));
#else
            buildPath = PlayerPrefs.GetString(PREFS_SETTINGS_BUILD_PATH, string.Empty);
#endif
            nameFormat = PlayerPrefs.GetString(PREFS_SETTINGS_NAME_FORMAT, "{package}_{date}");
            dateTimeFormat = PlayerPrefs.GetString(PREFS_SETTINGS_DATE_TIME_FORMAT, "yyyyMMddHHmmss");
            createNewFolder = PlayerPrefs.GetString(PREFS_SETTINGS_CREATE_NEW_FOLDER, bool.TrueString) == bool.TrueString;
            setURLSchemes = PlayerPrefs.GetString(PREFS_SETTINGS_SET_URL_SCHEMES, bool.FalseString) == bool.TrueString;
            if (PlayerPrefs.HasKey(PREFS_SETTINGS_URL_SCHEMES))
            {
                string str = PlayerPrefs.GetString(PREFS_SETTINGS_URL_SCHEMES, string.Empty);
                if (str != null && str.Length > 0)
                    urlSchemes = str.Split('|');
                else
                    urlSchemes = new string[0];
            }
            else
                urlSchemes = new string[0];
            buildOptions = (BuildOptions)PlayerPrefs.GetInt(PREFS_SETTINGS_BUILD_OPTIONS, 0);
        }

        public void Save()
        {
            PlayerPrefs.SetString(PREFS_SETTINGS_BUILD_PATH, buildPath);
            PlayerPrefs.SetString(PREFS_SETTINGS_NAME_FORMAT, nameFormat);
            PlayerPrefs.SetString(PREFS_SETTINGS_DATE_TIME_FORMAT, dateTimeFormat);
            PlayerPrefs.SetString(PREFS_SETTINGS_CREATE_NEW_FOLDER, createNewFolder ? bool.TrueString : bool.FalseString);
            PlayerPrefs.SetString(PREFS_SETTINGS_SET_URL_SCHEMES, createNewFolder ? bool.TrueString : bool.FalseString);
            if (urlSchemes != null && urlSchemes.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < urlSchemes.Length - 1; i++)
                {
                    sb.Append(urlSchemes[i]);
                    sb.Append('|');
                }
                sb.Append(urlSchemes[urlSchemes.Length - 1]);
                PlayerPrefs.SetString(PREFS_SETTINGS_URL_SCHEMES, sb.ToString());
            }
            else if (PlayerPrefs.HasKey(PREFS_SETTINGS_URL_SCHEMES))
                PlayerPrefs.DeleteKey(PREFS_SETTINGS_URL_SCHEMES);
            PlayerPrefs.SetInt(PREFS_SETTINGS_BUILD_OPTIONS, (int)buildOptions);
        }

        public string GetFolderName()
        {
            StringBuilder s = new StringBuilder(nameFormat);
            s.Replace("{name}", Application.productName);
            s.Replace("{package}", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS));
            s.Replace("{version}", Application.version);
            s.Replace("{bundle}", PlayerSettings.Android.bundleVersionCode.ToString());
            s.Replace("{date}", DateTime.Now.ToString(dateTimeFormat));
            return s.ToString();
        }
    }
}