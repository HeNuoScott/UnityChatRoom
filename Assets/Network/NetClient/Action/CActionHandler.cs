﻿using System.Collections.Generic;
using System.Reflection;
using Network.CommonData;
using UnityEngine;
using System;

namespace Network.NetClient
{
    /// <summary>
    /// 数据处理分发
    /// </summary>
    public class CActionHandler : MonoBehaviour
    {
        public const string ACTIONTYPE = "ActionType";

        /// <summary>
        /// 处理模块委托
        /// </summary>
        public delegate void HandleModule(CActionParameter parameter);

        /// <summary>
        /// 处理类型列表
        /// </summary>
        private static List<Type> AcitonTemplates = new List<Type>();

        /// <summary>
        /// 处理对象列表
        /// </summary>
        private Dictionary<int, CActionBase> actions = new Dictionary<int, CActionBase>();

        /// <summary>
        /// 处理事件
        /// </summary>
        private Dictionary<int, HandleModule> handles = new Dictionary<int, HandleModule>();

        /// <summary>
        /// 调用队列
        /// </summary>
        private Queue<CActionParameter> invokeQueue = new Queue<CActionParameter>();

        static CActionHandler()
        {
            Type actionBase = typeof(CActionBase);
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(actionBase))
                {
                    AcitonTemplates.Add(type);
                }
            }
        }

        private void Awake()
        {
            foreach (var action in AcitonTemplates)
            {
                CActionBase _action = action.Assembly.CreateInstance(action.FullName) as CActionBase;
                actions.Add(_action.ActionType, _action);
                handles.Add(_action.ActionType, null);
            }
        }

        private void Update()
        {
            lock (invokeQueue)
            {
                while (invokeQueue.Count > 0)
                {
                    HandleInvoke(invokeQueue.Dequeue());
                }
            }
        }

        /// <summary>
        /// 调用处理
        /// </summary>
        private void HandleInvoke(CActionParameter parameter)
        {
            int handleType = parameter.GetValue<int>(ACTIONTYPE);
            handles[handleType](parameter);
        }

        /// <summary>
        /// 绑定数据处理模块
        /// </summary>
        public void AddListener(int actionType, HandleModule listener)
        {
            if (!handles.ContainsKey(actionType))
                return;

            if (handles[actionType] == null)
                handles[actionType] = new HandleModule(listener);
            else
                handles[actionType] += listener;
        }

        /// <summary>
        /// 移除绑定数据处理模块
        /// </summary>
        public void RemoveListener(int actionType, HandleModule listener)
        {
            if (!handles.ContainsKey(actionType))
                return;

            HandleModule module = handles[actionType];
            if (module != null)
            {
                foreach (var invokeModule in module.GetInvocationList())
                {
                    if (invokeModule.Target.Equals(listener.Target))
                        module -= listener;
                }
            }
        }

        /// <summary>
        /// 处理数据过程
        /// </summary>
        public void DisposeProcess(DataPackage packet)
        {
            if (!actions.ContainsKey(packet.PacketType))
                return;

            CActionBase action = actions[packet.PacketType];
            HandleModule handler = handles[action.ActionType];
            CActionParameter parameter = new CActionParameter();
            parameter[ACTIONTYPE] = action.ActionType;
            action.Packet = packet;
            if (action.ReceiveProcess(parameter) && handler != null)
                lock (invokeQueue) invokeQueue.Enqueue(parameter);
            action.Clean();
        }

        /// <summary>
        /// 发送数据过程
        /// </summary>
        public DataPackage SendProcess(int actionType, CActionParameter parameter)
        {
            if (!actions.ContainsKey(actionType))
                return null;

            CActionBase handler = actions[actionType];
            handler.Packet = new DataPackage(new DynamicBuffer(0), handler.ActionType);
            if (handler.SendProcess(parameter))
                return handler.Packet;

            handler.Clean();
            return null;
        }
    }
}