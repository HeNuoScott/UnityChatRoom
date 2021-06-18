using System.Collections.Generic;
using System.Reflection;
using System;


namespace Network.NetServer
{
    public static class SActionFactory
    {
        private static Dictionary<int, SActionBase> ActionTemplates = new Dictionary<int, SActionBase>();

        static SActionFactory()
        {
            Type actionBase = typeof(SActionBase);
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(actionBase))
                {
                    SActionBase _action = type.Assembly.CreateInstance(type.FullName) as SActionBase;
                    ActionTemplates.Add(_action.ActionType, _action);
                }
            }
        }

        public static SActionBase CreateAction(int actionType, SessionClient session)
        {
            if (!ActionTemplates.ContainsKey(actionType))
                return null;

            SActionBase _action = ActionTemplates[actionType].Clone() as SActionBase;
            _action.Session = session;
            return _action;
        }

        public static Dictionary<int, SActionBase> CreateAllAction(SessionClient session)
        {
            Dictionary<int, SActionBase> actions = new Dictionary<int, SActionBase>();
            foreach (var actionType in ActionTemplates.Keys)
            {
                actions.Add(actionType, CreateAction(actionType, session));
            }
            return actions;
        }
    }
}
