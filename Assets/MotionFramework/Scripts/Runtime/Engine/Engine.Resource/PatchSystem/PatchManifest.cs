﻿//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Utility;
using MotionFramework.IO;

namespace MotionFramework.Resource
{
	/// <summary>
	/// 补丁清单文件
	/// </summary>
	[Serializable]
	public class PatchManifest
	{
		/// <summary>
		/// 资源版本号
		/// </summary>
		public int ResourceVersion;

		/// <summary>
		/// 内置资源的标记列表
		/// </summary>
		public string BuildinTags;

		/// <summary>
		/// 资源列表（主动收集的资源列表）
		/// </summary>
		public List<PatchAsset> AssetList = new List<PatchAsset>();

		/// <summary>
		/// 资源包列表
		/// </summary>
		public List<PatchBundle> BundleList = new List<PatchBundle>();


		/// <summary>
		/// 资源包集合（提供BundleName获取PatchBundle）
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchBundle> Bundles = new Dictionary<string, PatchBundle>();

		/// <summary>
		/// 资源映射集合（提供AssetPath获取PatchAsset）
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchAsset> Assets = new Dictionary<string, PatchAsset>();


		/// <summary>
		/// 获取内置资源标记列表
		/// </summary>
		public string[] GetBuildinTags()
		{
			return StringConvert.StringToStringList(BuildinTags, ';').ToArray();
		}

		/// <summary>
		/// 获取资源依赖列表
		/// </summary>
		public string[] GetAllDependencies(string assetPath)
		{
			if (Assets.TryGetValue(assetPath, out PatchAsset patchAsset))
			{
				List<string> result = new List<string>(patchAsset.DependIDs.Length);
				foreach (var dependID in patchAsset.DependIDs)
				{
					if (dependID >= 0 && dependID < BundleList.Count)
					{
						var dependPatchBundle = BundleList[dependID];
						result.Add(dependPatchBundle.BundleName);
					}
					else
					{
						throw new Exception($"Invalid depend id : {dependID} Asset path : {assetPath}");
					}
				}
				return result.ToArray();
			}
			else
			{
				MotionLog.Warning($"Not found asset path in patch manifest : {assetPath}");
				return new string[] { };
			}
		}

		/// <summary>
		/// 获取资源包名称
		/// </summary>
		public string GetAssetBundleName(string assetPath)
		{
			if (Assets.TryGetValue(assetPath, out PatchAsset patchAsset))
			{
				int bundleID = patchAsset.BundleID;
				if (bundleID >= 0 && bundleID < BundleList.Count)
				{
					var patchBundle = BundleList[bundleID];
					return patchBundle.BundleName;
				}
				else
				{
					throw new Exception($"Invalid depend id : {bundleID} Asset path : {assetPath}");
				}
			}
			else
			{
				MotionLog.Warning($"Not found asset path in patch manifest : {assetPath}");
				return string.Empty;
			}
		}


		/// <summary>
		/// 序列化
		/// </summary>
		public static void Serialize(string savePath, PatchManifest patchManifest)
		{
			string json = JsonUtility.ToJson(patchManifest);
			FileUtility.CreateFile(savePath, json);
		}

		/// <summary>
		/// 反序列化
		/// </summary>
		public static PatchManifest Deserialize(string jsonData)
		{
			PatchManifest patchManifest = JsonUtility.FromJson<PatchManifest>(jsonData);

			// BundleList
			foreach (var patchBundle in patchManifest.BundleList)
			{
				patchBundle.ParseFlagsValue();
				patchManifest.Bundles.Add(patchBundle.BundleName, patchBundle);
			}

			// AssetList
			foreach (var patchAsset in patchManifest.AssetList)
			{
				string assetPath = patchAsset.AssetPath;

				// 添加原始路径
				// 注意：我们不允许原始路径存在重名
				if (patchManifest.Assets.ContainsKey(assetPath))
					throw new Exception($"Asset path have existed : {assetPath}");
				else
					patchManifest.Assets.Add(assetPath, patchAsset);

				// 添加去掉后缀名的路径
				if (Path.HasExtension(assetPath))
				{
					string assetPathWithoutExtension = assetPath.RemoveExtension();
					if (patchManifest.Assets.ContainsKey(assetPathWithoutExtension))
						MotionLog.Warning($"Asset path have existed : {assetPathWithoutExtension}");
					else
						patchManifest.Assets.Add(assetPathWithoutExtension, patchAsset);
				}
			}

			return patchManifest;
		}
	}
}