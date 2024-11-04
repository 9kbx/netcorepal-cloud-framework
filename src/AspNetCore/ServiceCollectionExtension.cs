using System.Reflection;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.AspNetCore;
using NetCorePal.Extensions.AspNetCore.Validation;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRequestCancellationToken(this IServiceCollection services)
    {
        services.AddSingleton<IRequestCancellationToken, HttpContextAccessorRequestAbortedHandler>();
        return services;
    }

    public static IServiceCollection AddKnownExceptionErrorModelInterceptor(this IServiceCollection services)
    {
        services.AddTransient<IValidatorInterceptor, KnownExceptionErrorModelInterceptor>();
        return services;
    }

    public static MediatRServiceConfiguration AddKnownExceptionValidationBehavior(
        this MediatRServiceConfiguration cfg)
    {
        cfg.AddOpenBehavior(typeof(KnownExceptionValidationBehavior<,>));
        return cfg;
    }



    /// <summary>
    /// ������ʵ��IQuery�ӿڵ���ע��Ϊ��ѯ�࣬��ӵ�������
    /// </summary>
    /// <param name="services"></param>
    /// <param name="Assemblies"></param>
    /// <returns></returns>

    public static IServiceCollection AddAllQueries(this IServiceCollection services, params Assembly[] Assemblies)
    {
        foreach (var assembly in Assemblies)
        {
            //��assembly�л�ȡ����ʵ��IQuery�ӿڵ���
            var queryTypes = assembly.GetTypes().Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Any(i => i == typeof(IQuery)));
            foreach (var queryType in queryTypes)
            {
                //ע��Ϊ�Լ�
                services.AddTransient(queryType, queryType);
            }
        }
        return services;
    }
}