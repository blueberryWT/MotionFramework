﻿//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MotionFramework.Network;

namespace MotionFramework.Resource
{
	/// <summary>
	/// 初始化操作
	/// </summary>
	public abstract class InitializationOperation : AsyncOperationBase
	{
	}

	/// <summary>
	/// 编辑器下模拟运行的初始化操作
	/// </summary>
	internal class EditorModeInitializationOperation : InitializationOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 离线模式的初始化操作
	/// </summary>
	internal class OfflinePlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			Idle,
			LoadAppManifest,
			CheckAppManifest,
			Done,
		}

		private OfflinePlayModeImpl _impl;
		private ESteps _steps = ESteps.Idle;
		private WebGetRequest _downloader;
		private string _downloadURL;

		internal OfflinePlayModeInitializationOperation(OfflinePlayModeImpl impl)
		{
			_impl = impl;
		}
		internal override void Start()
		{
			_steps = ESteps.LoadAppManifest;
		}
		internal override void Update()
		{
			if (_steps == ESteps.Idle)
				return;

			if (_steps == ESteps.LoadAppManifest)
			{
				string filePath = AssetPathHelper.MakeStreamingLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
				_downloadURL = AssetPathHelper.ConvertToWWWPath(filePath);
				_downloader = new WebGetRequest(_downloadURL);
				_downloader.SendRequest();
				_steps = ESteps.CheckAppManifest;
			}

			if (_steps == ESteps.CheckAppManifest)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					Error = _downloader.GetError();
					Status = EOperationStatus.Failed;
					_downloader.Dispose();
					_steps = ESteps.Done;
					throw new System.Exception($"Fatal error : Failed load application patch manifest file : {_downloadURL}");
				}

				// 解析APP里的补丁清单
				_impl.AppPatchManifest = PatchManifest.Deserialize(_downloader.GetText());
				_downloader.Dispose();
				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;
			}
		}
	}

	/// <summary>
	/// 网络模式的初始化操作
	/// </summary>
	internal class HostPlayModeInitializationOperation : InitializationOperation
	{
		private enum ESteps
		{
			Idle,
			InitCache,
			LoadAppManifest,
			CheckAppManifest,
			LoadSandboxManifest,
			Done,
		}

		private HostPlayModeImpl _impl;
		private ESteps _steps = ESteps.Idle;
		private WebGetRequest _downloader;
		private string _downloadURL;

		internal HostPlayModeInitializationOperation(HostPlayModeImpl impl)
		{
			_impl = impl;
		}
		internal override void Start()
		{
			_steps = ESteps.InitCache;
		}
		internal override void Update()
		{
			if (_steps == ESteps.Idle)
				return;

			if (_steps == ESteps.InitCache)
			{
				// 如果缓存文件不存在
				if (PatchHelper.CheckSandboxCacheFileExist() == false)
				{
					_impl.Cache = new PatchCache();
					_impl.Cache.InitAppVersion(Application.version);
				}
				else
				{
					// 加载缓存
					_impl.Cache = PatchCache.LoadCache();

					// 修复缓存
					_impl.Cache.RepairCache();

					// 每次启动时比对APP版本号是否一致	
					if (_impl.Cache.CacheAppVersion != Application.version)
					{
						MotionLog.Warning($"Cache is dirty ! Cache app version is {_impl.Cache.CacheAppVersion}, Current app version is {Application.version}");

						// 注意：在覆盖安装的时候，会保留APP沙盒目录，可以选择清空缓存目录
						if (_impl.ClearCacheWhenDirty)
						{
							_impl.Cache.ClearCache();
						}

						// 注意：一定要删除清单文件
						PatchHelper.DeleteSandboxPatchManifestFile();
						_impl.Cache.InitAppVersion(Application.version);
					}
				}
				_steps = ESteps.LoadAppManifest;
			}

			if (_steps == ESteps.LoadAppManifest)
			{
				// 加载APP内的补丁清单
				MotionLog.Log($"Load application patch manifest.");
				string filePath = AssetPathHelper.MakeStreamingLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
				_downloadURL = AssetPathHelper.ConvertToWWWPath(filePath);
				_downloader = new WebGetRequest(_downloadURL);
				_downloader.SendRequest();
				_steps = ESteps.CheckAppManifest;
			}

			if (_steps == ESteps.CheckAppManifest)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					Error = _downloader.GetError();
					Status = EOperationStatus.Failed;		
					_downloader.Dispose();
					_steps = ESteps.Done;
					throw new System.Exception($"Fatal error : Failed load application patch manifest file : {_downloadURL}");
				}

				// 解析补丁清单
				string jsonData = _downloader.GetText();
				_impl.AppPatchManifest = PatchManifest.Deserialize(jsonData);
				_impl.LocalPatchManifest = _impl.AppPatchManifest;
				_downloader.Dispose();
				_steps = ESteps.LoadSandboxManifest;
			}

			if (_steps == ESteps.LoadSandboxManifest)
			{
				// 加载沙盒内的补丁清单	
				if (PatchHelper.CheckSandboxPatchManifestFileExist())
				{
					MotionLog.Log($"Load sandbox patch manifest.");
					string filePath = AssetPathHelper.MakePersistentLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
					string jsonData = File.ReadAllText(filePath);
					_impl.LocalPatchManifest = PatchManifest.Deserialize(jsonData);
				}

				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;
			}
		}
	}
}