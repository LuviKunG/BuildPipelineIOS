using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace LuviKunG.BuildPipeline.iOS
{
    public static class BuildPipelineIOSPostProcess
    {
        private static BuildPipelineIOSSettings settings;

        [PostProcessBuild(0)]
        public static void OnPostProcessBuild(BuildTarget target, string targetPath)
        {
            settings = BuildPipelineIOSSettings.Instance;
            if (target == BuildTarget.iOS)
            {
                try
                {
                    string plistPath = targetPath + "/Info.plist";
                    PlistDocument plist = new PlistDocument();
                    plist.ReadFromString(File.ReadAllText(plistPath));
                    // Get root
                    PlistElementDict rootDict = plist.root;
                    // background location useage key (new in iOS 8)
                    rootDict.SetString("NSLocationAlwaysUsageDescription", "Uses background location");
                    // background modes
                    PlistElementArray bgModes = rootDict.CreateArray("UIBackgroundModes");
                    bgModes.AddString("location");
                    bgModes.AddString("fetch");
                    if (settings.setURLSchemes)
                    {
                        if (settings.urlSchemes != null && settings.urlSchemes.Length > 0)
                        {
                            PlistElementArray urlSchemes = rootDict.CreateArray("CFBundleURLSchemes");
                            for (int i = 0; i < settings.urlSchemes.Length; i++)
                                urlSchemes.AddString(settings.urlSchemes[i]);
                        }
                        else
                            Debug.LogWarning("Cannot set URL Schemes because it's null or empty.");
                    }
                    // Write to file
                    File.WriteAllText(plistPath, plist.WriteToString());
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}