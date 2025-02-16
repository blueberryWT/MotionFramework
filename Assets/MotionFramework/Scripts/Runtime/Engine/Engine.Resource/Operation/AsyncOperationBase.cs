﻿//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Resource
{
	public abstract class AsyncOperationBase : IEnumerator
	{
		/// <summary>
		/// 状态
		/// </summary>
		public EOperationStatus Status { get; protected set; } = EOperationStatus.None;

		/// <summary>
		/// 错误信息
		/// </summary>
		public string Error { get; protected set; } = string.Empty;

		/// <summary>
		/// 是否已经完成
		/// </summary>
		public bool IsDone
		{
			get
			{
				return Status == EOperationStatus.Failed || Status == EOperationStatus.Succeed;
			}
		}

		/// <summary>
		/// 用户请求的回调
		/// </summary>
		private Action<AsyncOperationBase> _callback;

		/// <summary>
		/// 完成事件
		/// </summary>
		public event Action<AsyncOperationBase> Completed
		{
			add
			{
				if (IsDone)
					value.Invoke(this);
				else
					_callback += value;
			}
			remove
			{
				_callback -= value;
			}
		}

		internal abstract void Start();
		internal abstract void Update();
		internal void Finish()
		{
			_callback?.Invoke(this);
		}

		#region 异步相关
		public bool MoveNext()
		{
			return !IsDone;
		}
		public void Reset()
		{
		}
		public object Current => null;
		#endregion
	}
}