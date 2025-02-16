﻿//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Resource
{
	public class OperationUpdater
	{
		private static readonly List<AsyncOperationBase> _operations = new List<AsyncOperationBase>(100);

		public static void ProcessOperaiton(AsyncOperationBase operationBase)
		{
			_operations.Add(operationBase);
			operationBase.Start();
		}

		public static void Update()
		{
			for (int i = _operations.Count - 1; i >= 0; i--)
			{
				_operations[i].Update();
				if (_operations[i].IsDone)
				{
					_operations[i].Finish();
					_operations.RemoveAt(i);
				}
			}
		}
	}
}