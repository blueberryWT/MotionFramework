﻿//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Resource
{
	internal sealed class BundledAssetProvider : BundledProvider
	{
		private AssetBundleRequest _cacheRequest;
		public override float Progress
		{
			get
			{
				if (_cacheRequest == null)
					return 0;
				return _cacheRequest.progress;
			}
		}

		public BundledAssetProvider(string assetPath, System.Type assetType)
			: base(assetPath, assetType)
		{
		}
		public override void Update()
		{
			if (IsDone)
				return;

			if (States == EAssetStates.None)
			{
				States = EAssetStates.CheckBundle;
			}

			// 1. 检测资源包
			if (States == EAssetStates.CheckBundle)
			{
				if (IsWaitForAsyncComplete)
				{
					DependBundles.WaitForAsyncComplete();
					OwnerBundle.WaitForAsyncComplete();
				}

				if (DependBundles.IsDone() == false)
					return;
				if (OwnerBundle.IsDone() == false)
					return;

				if (OwnerBundle.CacheBundle == null)
				{
					States = EAssetStates.Fail;
					InvokeCompletion();
				}
				else
				{
					States = EAssetStates.Loading;
				}
			}

			// 2. 加载资源对象
			if (States == EAssetStates.Loading)
			{
				if (IsWaitForAsyncComplete)
				{
					if (AssetType == null)
						AssetObject = OwnerBundle.CacheBundle.LoadAsset(AssetName);
					else
						AssetObject = OwnerBundle.CacheBundle.LoadAsset(AssetName, AssetType);
				}
				else
				{
					if (AssetType == null)
						_cacheRequest = OwnerBundle.CacheBundle.LoadAssetAsync(AssetName);
					else
						_cacheRequest = OwnerBundle.CacheBundle.LoadAssetAsync(AssetName, AssetType);
				}
				States = EAssetStates.Checking;
			}

			// 3. 检测加载结果
			if (States == EAssetStates.Checking)
			{
				if (_cacheRequest != null)
				{
					if (IsWaitForAsyncComplete)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						MotionLog.Warning("Suspend the main thread to load unity asset.");
						AssetObject = _cacheRequest.asset;
					}
					else
					{
						if (_cacheRequest.isDone == false)
							return;
						AssetObject = _cacheRequest.asset;
					}
				}

				States = AssetObject == null ? EAssetStates.Fail : EAssetStates.Success;
				if (States == EAssetStates.Fail)
					MotionLog.Warning($"Failed to load asset : {AssetName} from bundle : {OwnerBundle.BundleInfo.BundleName}");
				InvokeCompletion();
			}
		}
	}
}