using System.Collections.Generic;
using System.Reflection;
using Network.CommonData;
using System;

namespace Network.Client
{
    /// <summary>
    /// 数据处理分发
    /// </summary>
    public class ClientActionHandler
    {
        /// <summary>
        /// 处理模块委托
        /// </summary>
        public delegate void HandleModule(ActionParameter parameter);

        /// <summary>
        /// 处理类型列表
        /// </summary>
        private static List<Type> AcitonTemplates = new List<Type>();

        /// <summary>
        /// 处理对象列表
        /// </summary>
        private Dictionary<ActionTypeEnum, ClientActionBase> actions = new Dictionary<ActionTypeEnum, ClientActionBase>();

        /// <summary>
        /// 处理事件
        /// </summary>
        private Dictionary<ActionTypeEnum, HandleModule> handles = new Dictionary<ActionTypeEnum, HandleModule>();

        /// <summary>
        /// 调用队列
        /// </summary>
        private Queue<ActionParameter> invokeQueue = new Queue<ActionParameter>();

        public ClientActionHandler()
        {
            Type actionBase = typeof(ClientActionBase);
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(actionBase))
                {
                    AcitonTemplates.Add(type);
                }
            }
            foreach (var action in AcitonTemplates)
            {
                ClientActionBase _action = action.Assembly.CreateInstance(action.FullName) as ClientActionBase;
                actions.Add(_action.ActionType, _action);
                handles.Add(_action.ActionType, null);
            }
        }

        public void Update()
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
        private void HandleInvoke(ActionParameter parameter)
        {
            ActionTypeEnum handleType = (ActionTypeEnum)parameter.GetValue<int>(NetConfig.ACTIONTYPE);
            handles[handleType](parameter);
        }

        /// <summary>
        /// 绑定数据处理模块
        /// </summary>
        public void AddListener(ActionTypeEnum actionType, HandleModule listener)
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
        public void RemoveListener(ActionTypeEnum actionType, HandleModule listener)
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
            if (!actions.ContainsKey(packet.PacketType)) return;

            ClientActionBase action = actions[packet.PacketType];
            HandleModule handler = handles[action.ActionType];
            ActionParameter parameter = new ActionParameter();
            parameter[NetConfig.ACTIONTYPE] = action.ActionType;
            action.Packet = packet;
            if (action.ReceiveProcess(parameter) && handler != null)
                lock (invokeQueue) invokeQueue.Enqueue(parameter);
            action.Clean();
        }

        /// <summary>
        /// 发送数据过程
        /// </summary>
        public DataPackage SendProcess(ActionTypeEnum actionType, ActionParameter parameter)
        {
            if (!actions.ContainsKey(actionType)) return null;

            ClientActionBase handler = actions[actionType];
            handler.Packet = new DataPackage(new DynamicBuffer(0), handler.ActionType);
            if (handler.SendProcess(parameter)) return handler.Packet;
            handler.Clean();
            return null;
        }
    }
}