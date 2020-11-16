using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyContainerLib.Tests
{
    public class ServiceImplGeneric<TRepository> : IService<TRepository>
        where TRepository : IRepository
    {
        public ServiceImplGeneric(TRepository repository)
        {
            
        }
    
    }

    public interface ISingletonCheck { }
    public class SingletonCheckImpl : ISingletonCheck
    {

    }

    public interface INotSingletonCheck { }
    public class NotSingletonCheckImpl : INotSingletonCheck
    {

    }

    public interface IRepository { }
    public class RepositoryImpl : IRepository
    {
        public RepositoryImpl() { throw new Exception(); } // может иметь свои зависимости, опустим для простоты
    }

    public interface IService { }
    public class ServiceImpl : IService
    {
        public ServiceImpl(IRepository repository) // ServiceImpl зависит от IRepository
        {

        }
    }

    public class SecondServiceImpl : IService { }

    public class ThirdService : IService
    {
        public ThirdService(IEnumerable<IRepository> repo)
        {

        }
    }

    public class MySqlRepository : IRepository 
    {
        public MySqlRepository()
        {
            throw new Exception();
        }
    }

    public interface IService<TRepository> where TRepository : IRepository
    {
    }

    public class ServiceImpl<TRepository> : IService<TRepository>
        where TRepository : IRepository
    {
        public ServiceImpl(TRepository repository)
        {

        }
    }
}
