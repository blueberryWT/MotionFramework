﻿//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using MotionFramework.Resource;
using MotionFramework.Utility;

namespace MotionFramework.Editor
{
	public static class AssetBundleBuilderHelper
	{
		/// <summary>
		/// 获取默认的输出根路录
		/// </summary>
		public static string GetDefaultOutputRoot()
		{
			string projectPath = EditorTools.GetProjectPath();
			return $"{projectPath}/Bundles";
		}

		/// <summary>
		/// 获取构建管线的输出目录
		/// </summary>
		public static string MakePipelineOutputDirectory(string outputRoot, BuildTarget buildTarget)
		{
			return $"{outputRoot}/{buildTarget}/{ResourceSettingData.Setting.UnityManifestFileName}";
		}

		/// <summary>
		/// 制作AssetBundle的完整名称
		/// 注意：名称为全部小写并且包含后缀名
		/// </summary>
		public static string MakeBundleName(string bundleLabel, string bundleVariant)
		{
			if (string.IsNullOrEmpty(bundleVariant))
				return bundleLabel.ToLower();
			else
				return $"{bundleLabel}.{bundleVariant}".ToLower();
		}


		/// <summary>
		/// 清空流文件夹
		/// </summary>
		public static void ClearStreamingAssetsFolder()
		{
			string streamingPath = Application.dataPath + "/StreamingAssets";
			EditorTools.ClearFolder(streamingPath);
		}

		/// <summary>
		/// 删除流文件夹内无关的文件
		/// 删除.manifest文件和.meta文件
		/// </summary>
		public static void DeleteStreamingAssetsIgnoreFiles()
		{
			string streamingPath = Application.dataPath + "/StreamingAssets";
			if (Directory.Exists(streamingPath))
			{
				string[] files = Directory.GetFiles(streamingPath, "*.manifest", SearchOption.AllDirectories);
				foreach (var file in files)
				{
					FileInfo info = new FileInfo(file);
					info.Delete();
				}

				files = Directory.GetFiles(streamingPath, "*.meta", SearchOption.AllDirectories);
				foreach (var item in files)
				{
					FileInfo info = new FileInfo(item);
					info.Delete();
				}
			}
		}


		/// <summary>
		/// 获取所有补丁包版本列表
		/// 注意：列表会按照版本号从小到大排序
		/// </summary>
		private static List<int> GetPackageVersionList(BuildTarget buildTarget, string outputRoot)
		{
			List<int> versionList = new List<int>();

			string parentPath = $"{outputRoot}/{buildTarget}";
			if (Directory.Exists(parentPath) == false)
				return versionList;

			// 获取所有补丁包文件夹
			string[] allFolders = Directory.GetDirectories(parentPath);
			for (int i = 0; i < allFolders.Length; i++)
			{
				string folderName = Path.GetFileNameWithoutExtension(allFolders[i]);
				if (int.TryParse(folderName, out int version))
					versionList.Add(version);
			}

			// 从小到大排序
			versionList.Sort();
			return versionList;
		}

		/// <summary>
		/// 获取当前最大的补丁包版本号
		/// </summary>
		/// <returns>如果没有任何补丁版本，那么返回-1</returns>
		public static int GetMaxPackageVersion(BuildTarget buildTarget, string outputRoot)
		{
			List<int> versionList = GetPackageVersionList(buildTarget, outputRoot);
			if (versionList.Count == 0)
				return -1;
			return versionList[versionList.Count - 1];
		}

		/// <summary>
		/// 是否存在任何补丁包版本
		/// </summary>
		public static bool HasAnyPackageVersion(BuildTarget buildTarget, string outputRoot)
		{
			List<int> versionList = GetPackageVersionList(buildTarget, outputRoot);
			return versionList.Count > 0;
		}


		/// <summary>
		/// 从输出目录加载补丁清单文件
		/// </summary>
		public static PatchManifest LoadPatchManifestFile(string fileDirectory)
		{
			string filePath = $"{fileDirectory}/{ResourceSettingData.Setting.PatchManifestFileName}";
			if (File.Exists(filePath) == false)
			{
				throw new System.Exception($"Not found patch manifest file : {filePath}");
			}

			string jsonData = FileUtility.ReadFile(filePath);
			return PatchManifest.Deserialize(jsonData);
		}
	}
}