﻿//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Resource
{
	public sealed class OfflinePlayModeImpl : IBundleServices
	{
		internal PatchManifest AppPatchManifest;
		
		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync()
		{
			var operation = new OfflinePlayModeInitializationOperation(this);
			OperationUpdater.ProcessOperaiton(operation);
			return operation;
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public int GetResourceVersion()
		{
			if (AppPatchManifest == null)
				return 0;			
			return AppPatchManifest.ResourceVersion;
		}

		/// <summary>
		/// 获取内置资源标记列表
		/// </summary>
		public string[] GetManifestBuildinTags()
		{
			if (AppPatchManifest == null)
				return new string[0];
			return AppPatchManifest.GetBuildinTags();
		}

		#region IBundleServices接口
		AssetBundleInfo IBundleServices.GetAssetBundleInfo(string bundleName)
		{
			if (string.IsNullOrEmpty(bundleName))
				return new AssetBundleInfo(string.Empty, string.Empty);

			if (AppPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				string localPath = AssetPathHelper.MakeStreamingLoadPath(patchBundle.Hash);
				AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, localPath, patchBundle.Version, patchBundle.IsEncrypted, patchBundle.IsRawFile);
				return bundleInfo;
			}
			else
			{
				MotionLog.Warning($"Not found bundle in patch manifest : {bundleName}");
				AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, string.Empty);
				return bundleInfo;
			}
		}
		bool IBundleServices.CacheDownloadFile(string bundleName)
		{
			throw new NotImplementedException();
		}
		string IBundleServices.GetAssetBundleName(string assetPath)
		{
			return AppPatchManifest.GetAssetBundleName(assetPath);
		}
		string[] IBundleServices.GetAllDependencies(string assetPath)
		{
			return AppPatchManifest.GetAllDependencies(assetPath);
		}
		#endregion
	}
}