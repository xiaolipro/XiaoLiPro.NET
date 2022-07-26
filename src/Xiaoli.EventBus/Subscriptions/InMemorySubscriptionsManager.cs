﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaoLi.EventBus.Events;

namespace XiaoLi.EventBus.Subscriptions
{
    /// <summary>
    /// 基于内存的订阅管理器
    /// </summary>
    public class InMemorySubscriptionsManager : ISubscriptionsManager
    {
        // 内存字典
        // 数据格式：{事件名称:[订阅信息]}
        private readonly Dictionary<string, List<SubscriptionInfo>> _subscriptions;

        // 事件类型列表
        private readonly List<Type> _eventTypes;
        public InMemorySubscriptionsManager()
        {
            _subscriptions = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
        }

        #region implements
        public bool IsEmpty => _subscriptions.Count == 0;

        public event EventHandler<string> OnEventRemoved;
        public void AddDynamicSubscription<THandler>(string eventName) 
            where THandler : IDynamicIntegrationEventHandler
        {
            var handlerType = typeof(THandler);
            var subscription = SubscriptionInfo.Dynamic(eventName,handlerType);
            DoAddSubscriptionInfo(subscription);
        }

        public void AddSubscription<TEvent, THandler>() 
            where TEvent : IntegrationEvent 
            where THandler : IIntegrationEventHandler<TEvent>
        {
            string eventName = GetEventName<TEvent>();
            var subscription = SubscriptionInfo.Typed(eventName, typeof(THandler));
            DoAddSubscriptionInfo(subscription);
        }

        public void RemoveDynamicSubscription<THandler>(string eventName) 
            where THandler : IDynamicIntegrationEventHandler
        {
            var subscription = DoFindSubscription(eventName, typeof(THandler));
            DoRemoveSubscriptionInfo(subscription);
        }

        public void RemoveSubscription<TEvent, THandler>() 
            where TEvent : IntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>
        {
            string eventName = GetEventName<TEvent>();
            var subscription = DoFindSubscription(eventName, typeof(THandler));
            DoRemoveSubscriptionInfo(subscription);
        }

        public void Clear() => _subscriptions.Clear();

        public IEnumerable<SubscriptionInfo> GetSubscriptionInfos(string eventName) => _subscriptions[eventName];
        
        public IEnumerable<SubscriptionInfo> GetSubscriptionInfos<TEvent>() where TEvent : IntegrationEvent
            => GetSubscriptionInfos(GetEventName<TEvent>());

        public bool HasSubscriptions<TEvent>() where TEvent : IntegrationEvent
            => HasSubscriptions(GetEventName<TEvent>());

        public bool HasSubscriptions(string eventName)
            => _subscriptions.ContainsKey(eventName);

        public string GetEventName<TEvent>() where TEvent : IntegrationEvent 
            => typeof(TEvent).Name;

        public Type GetEventTypeByName(string eventName) 
            => _eventTypes.SingleOrDefault(type => type.Name.Equals(eventName, StringComparison.OrdinalIgnoreCase));

        #endregion

        #region private methods

        /// <summary>
        /// 添加订阅信息
        /// </summary>
        /// <param name="subscriptionInfo"></param>
        /// <exception cref="ArgumentException"></exception>
        void DoAddSubscriptionInfo(SubscriptionInfo subscriptionInfo)
        {
            string eventName = subscriptionInfo.EventName;
            var handlerType = subscriptionInfo.HandlerType;
            if (!HasSubscriptions(eventName))
            {
                _subscriptions.Add(eventName, new List<SubscriptionInfo>());
            }

            if (_subscriptions[eventName].Any(x => x.HandlerType == handlerType))
            {
                throw new ArgumentException($"Handler type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            _subscriptions[eventName].Add(subscriptionInfo);

            // 维护事件类型列表
            if (!_eventTypes.Contains(handlerType))
            {
                _eventTypes.Add(handlerType);
            }
        }

        /// <summary>
        /// 移除订阅信息
        /// </summary>
        /// <param name="subscriptionInfo"></param>
        private void DoRemoveSubscriptionInfo(SubscriptionInfo subscriptionInfo)
        {
            if (subscriptionInfo == null) return;
            string eventName = subscriptionInfo.EventName;

            _subscriptions[eventName].Remove(subscriptionInfo);


            // 事件没有任何订阅信息时，清除事件
            if (!_subscriptions[eventName].Any())
            {
                RemoveEvent(eventName);
            }
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="eventName"></param>
        private void RemoveEvent(string eventName)
        {
            // 从订阅集中移除
            _subscriptions.Remove(eventName);
            RaiseOnEventRemoved(eventName);
            // 从事件类型列表中移除
            var eventType = GetEventTypeByName(eventName);
            if (eventType != null)
            {
                _eventTypes.Remove(eventType);
            }
        }

        /// <summary>
        /// 查询订阅信息
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handlerType"></param>
        /// <returns></returns>
        private SubscriptionInfo DoFindSubscription(string eventName, Type handlerType)
        {
            if (!HasSubscriptions(eventName)) return default;

            return _subscriptions[eventName].SingleOrDefault(x => x.HandlerType == handlerType);
        }

        /// <summary>
        /// 引发移除事件
        /// </summary>
        /// <param name="eventName"></param>
        private void RaiseOnEventRemoved(string eventName)
        {
            OnEventRemoved?.Invoke(this, eventName);
        }
        #endregion
    }
}
