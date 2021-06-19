using System.Collections.Generic;
using System.Reflection;
using System;
using Network.CommonData;

namespace Network.Server
{
    public static class ServerActionFactory
    {
        private static Dictionary<ActionTypeEnum, ServerActionBase> ActionTemplates = new Dictionary<ActionTypeEnum, ServerActionBase>();

        static ServerActionFactory()
        {
            Type actionBase = typeof(ServerActionBase);
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(actionBase))
                {
                    ServerActionBase _action = type.Assembly.CreateInstance(type.FullName) as ServerActionBase;
                    ActionTemplates.Add(_action.ActionType, _action);
                }
            }
        }

        public static ServerActionBase CreateAction(ActionTypeEnum actionType, ServiceSession session)
        {
            if (!ActionTemplates.ContainsKey(actionType)) return null;
            ServerActionBase _action = ActionTemplates[actionType].Clone() as ServerActionBase;
            _action.Session = session;
            return _action;
        }

        public static Dictionary<ActionTypeEnum, ServerActionBase> CreateAllAction(ServiceSession session)
        {
            Dictionary<ActionTypeEnum, ServerActionBase> actions = new Dictionary<ActionTypeEnum, ServerActionBase>();
            foreach (var actionType in ActionTemplates.Keys)
            {
                actions.Add(actionType, CreateAction(actionType, session));
            }
            return actions;
        }
    }
}
