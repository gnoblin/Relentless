﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.CloudBuild;
using Debug = UnityEngine.Debug;

namespace LoomNetwork.CZB
{
    public class BuildMetaInfoGenerator : IPreprocessBuild
    {
        private const string BUILD_META_INFO_PATH = "Assets/Resources/" + BuildMetaInfo.ResourcesPath + ".asset";

        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            PreBuildInternal();
        }

        [InitializeOnLoadMethod]
        public static void CreateIfMissing()
        {
            GetBuildMetaInfo();
        }

        public static void PreBuildInternal()
        {
            BuildMetaInfo buildMetaInfo = GetBuildMetaInfo();
            buildMetaInfo.BuildDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ssZ");
            buildMetaInfo.BuildDayOfYear = DateTime.UtcNow.DayOfYear;

#if !UNITY_CLOUD_BUILD
            string output;
            int exitCode;
            try
            {
                ExecuteCommand("git", "rev-parse --abbrev-ref HEAD", out output, out exitCode, timeout: 20000);
                if (exitCode != 0)
                {
                    throw new Exception("exitCode != 0");
                }

                buildMetaInfo.GitBranchName = output;
            } catch (Exception e)
            {
                buildMetaInfo.GitBranchName = "[error]";
                Debug.LogException(e);
            }

            try
            {
                ExecuteCommand("git", "log --pretty=format:%h -n 1", out output, out exitCode, timeout: 20000);
                if (exitCode != 0)
                {
                    throw new Exception("exitCode != 0");
                }

                buildMetaInfo.GitCommitHash = output;
            } catch (Exception e)
            {
                buildMetaInfo.GitCommitHash = "[error]";
                Debug.LogException(e);
            }

#endif

            EditorUtility.SetDirty(buildMetaInfo);
        }

        public static void PreCloudBuildExport(BuildManifestObject manifest)
        {
#if UNITY_CLOUD_BUILD
            Debug.Log("Cloud Build manifest:\r\n" + manifest.ToJson());
#endif

            BuildMetaInfo buildMetaInfo = GetBuildMetaInfo();
            buildMetaInfo.GitBranchName = manifest.GetValue<string>("scmBranch");
            buildMetaInfo.GitCommitHash = manifest.GetValue<string>("scmCommitId");
            buildMetaInfo.CloudBuildBuildNumber = Convert.ToInt32(manifest.GetValue<string>("buildNumber"));
            buildMetaInfo.CloudBuildTargetName = manifest.GetValue<string>("cloudBuildTargetName");

            const int gitShortHashLength = 8;
            buildMetaInfo.GitCommitHash = buildMetaInfo.GitCommitHash.Substring(0, buildMetaInfo.GitCommitHash.Length > gitShortHashLength?gitShortHashLength:buildMetaInfo.GitCommitHash.Length);

            EditorUtility.SetDirty(buildMetaInfo);
        }

        private static BuildMetaInfo GetBuildMetaInfo()
        {
            BuildMetaInfo instance = BuildMetaInfo.Instance;
            if (instance != null)
            {
                return instance;
            }

            instance = ScriptableObject.CreateInstance<BuildMetaInfo>();
            AssetDatabase.CreateAsset(instance, BUILD_META_INFO_PATH);

            return instance;
        }

        private static void ExecuteCommand(string fileName, string arguments, out string output, out int exitCode, string standardInput = null, Encoding standardInputEncoding = null, int timeout = 5000)
        {
            Process program = new Process
                              {
                                  StartInfo =
                                  {
                                      FileName = fileName,
                                      Arguments = arguments,
                                      UseShellExecute = false,
                                      RedirectStandardOutput = true,
                                      RedirectStandardInput = standardInput != null,
                                      CreateNoWindow = true,
                                      StandardOutputEncoding = Encoding.UTF8
                                  }
                              };
            program.Start();
            if (standardInput != null)
            {
                StreamWriter streamWriter = new StreamWriter(program.StandardInput.BaseStream, standardInputEncoding ?? new UTF8Encoding(false));
                streamWriter.Write(standardInput);
                streamWriter.Close();
            }

            string result = program.StandardOutput.ReadToEnd().Trim();
            bool exited = program.WaitForExit(timeout);
            if (!exited)
            {
                if (!program.HasExited)
                {
                    program.Kill();
                }

                throw new TimeoutException($"'{fileName} {arguments}' did not finish in time, output: \r\n{result}");
            }

            output = result;
            exitCode = program.ExitCode;
        }
    }
}
