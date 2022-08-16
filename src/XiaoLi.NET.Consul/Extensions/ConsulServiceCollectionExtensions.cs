﻿using System;
using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using XiaoLi.NET.Consul.LoadBalancing;
using XiaoLi.NET.LoadBalancing;

namespace XiaoLi.NET.Consul.Extensions
{
    public static class ConsulServiceCollectionExtensions
    {
        /// <summary>
        /// 添加Consul
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddConsul(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConsulClientOptions>(configuration.GetSection("ConsulClient"));
            services.Configure<ConsulRegisterOptions>(configuration.GetSection("ConsulRegister"));

            var consulClientOptions = configuration.GetSection("ConsulClient").Get<ConsulClientOptions>();
            if (consulClientOptions == null) throw new ArgumentNullException(nameof(ConsulClientOptions));
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                consulConfig.Address = consulClientOptions.Address;
                consulConfig.Datacenter = consulClientOptions.Datacenter;
            }));
            services.AddSingleton<IHostedService, ConsulHostedService>();
        }


        /// <summary>
        /// 添加Consul负载均衡调度器
        /// </summary>
        public static void AddConsulDispatcher<TBalancer>(this IServiceCollection services)
            where TBalancer : class, IBalancer
        {
            services.TryAddSingleton<IBalancer, TBalancer>();
            services.TryAddSingleton<IResolver, ConsulResolver>();
            services.TryAddSingleton<IDispatcher, ConsulDispatcher>();
        }
    }
}