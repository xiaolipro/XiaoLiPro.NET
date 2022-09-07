﻿using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XiaoLi.NET.Grpc.Interceptors;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SkyApm.Diagnostics.Grpc.Client;
using SkyApm.Diagnostics.Grpc.Server;
using XiaoLi.NET.Grpc.LoadBalancingFactories;
using XiaoLi.NET.LoadBalancing;

namespace XiaoLi.NET.Grpc
{
    public static class GrpcServiceCollectionExtensions
    {
        /// <summary>
        /// 添加Grpc负载均衡客户端
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="services"></param>
        /// <param name="address"></param>
        public static IHttpClientBuilder AddGrpcLoadBalancingClient<TClient>(
            this IServiceCollection services, string address)
            where TClient : class
        {
            services.TryAddSingleton(typeof(ClientLogInterceptor));
            services.TryAddSingleton(typeof(ClientExceptionInterceptor));

            var sp = services.BuildServiceProvider();
            return services
                .AddGrpcClient<TClient>(options =>
                {
                    options.Address = new Uri($"{sp.GetRequiredService<IResolver>().Name}://" + address);
                })
                .ConfigureChannel(options =>
                {
                    options.Credentials = ChannelCredentials.Insecure;
                    options.ServiceConfig = new ServiceConfig
                        { LoadBalancingConfigs = { new LoadBalancingConfig(sp.GetRequiredService<IBalancer>().Name) } };
                    options.ServiceProvider = sp;
                })
                .AddInterceptor<ClientExceptionInterceptor>()
                .AddInterceptor<ClientLogInterceptor>()
                .AddInterceptor<ClientDiagnosticInterceptor>();
        }

        /// <summary>
        /// 添加Grpc服务端
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcServer(this IServiceCollection services)
        {
            return services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
                options.Interceptors.Add<ServerLogInterceptor>();
                options.Interceptors.Add<ServerExceptionInterceptor>();
                options.Interceptors.Add<ServerDiagnosticInterceptor>();
            }).Services;
        }

        /// <summary>
        /// 添加Grpc客户端负载均衡器
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TResolver"></typeparam>
        /// <typeparam name="TBalancer"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddGrpcClientLoadBalancer<TResolver, TBalancer>(
            this IServiceCollection services)
            where TResolver : class, IResolver
            where TBalancer : class, IBalancer
        {
            services.Replace(new ServiceDescriptor(typeof(IBalancer), typeof(TBalancer),
                ServiceLifetime.Singleton));
            services.Replace(new ServiceDescriptor(typeof(IResolver), typeof(TResolver),
                ServiceLifetime.Singleton));

            services.TryAddSingleton<IBackoffPolicyFactory,CustomBackoffPolicyFactory>();
            services.TryAddSingleton<ResolverFactory, CustomResolverFactory>();
            services.TryAddSingleton<LoadBalancerFactory, CustomBalancerFactory>();

            return services;
        }
    }
}