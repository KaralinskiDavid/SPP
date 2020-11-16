using Microsoft.VisualStudio.TestTools.UnitTesting;
using DependencyContainerLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DependencyContainerLib.Tests
{

    [TestClass()]
    public class ContainerTests
    {
        Configuration config;

        [TestInitialize]
        public void TestInit()
        {
            config = new Configuration();
        }

        [TestMethod()]
        public void InterfaceOrAbstractPassingCheck()
        {

            Assert.ThrowsException<ArgumentException>(() => config.Register<IEnumerable<int>, IList<int>>());
        }

        [TestMethod()]
        public void SimpleDependencyTest()
        {
            config.Register<IEnumerable<int>, List<int>>();
            Container container = new Container(config);

            var actualObject = container.Resolve<IEnumerable<int>>();

            Type expectedType = typeof(List<int>);
            Assert.IsInstanceOfType(actualObject, expectedType);
        }

        [TestMethod()]
        public void RecursiveDependencyTest()
        {
            config.Register<IService, ServiceImpl>();
            config.Register<IRepository, RepositoryImpl>();
            Container container = new Container(config);

            Assert.ThrowsException<TargetInvocationException>(() => container.Resolve<IService>());
        }

        [TestMethod()]
        public void SingletonTest()
        {
            config.Register<ISingletonCheck, SingletonCheckImpl>(true);
            config.Register<INotSingletonCheck, NotSingletonCheckImpl>();
            Container container = new Container(config);

            SingletonCheckImpl firstSingletonInvocation = container.Resolve<ISingletonCheck>();
            SingletonCheckImpl secondSingletonInvocation = container.Resolve<ISingletonCheck>();

            NotSingletonCheckImpl firstNotSingletonInvocation = container.Resolve<INotSingletonCheck>();
            NotSingletonCheckImpl secondNotSingletonInvocation = container.Resolve<INotSingletonCheck>();

            Assert.AreEqual(firstSingletonInvocation.GetHashCode(), secondSingletonInvocation.GetHashCode());
            Assert.AreNotEqual(firstNotSingletonInvocation.GetHashCode(), secondNotSingletonInvocation.GetHashCode());
        }

        //[TestMethod()]
        //public void MultiThreadEnvironment()
        //{
        //    config.Register<ISingletonCheck, SingletonCheckImpl>(true);
        //    Container container = new Container(config);
        //    List<SingletonCheckImpl> list = new List<SingletonCheckImpl>();

        //    for(var i=0;i<10;i++)
        //    {
        //        Task.Run(() => list.Add(container.Resolve<ISingletonCheck>()));
        //    }
        //    Task.WaitAll();

        //    List<SingletonCheckImpl> expected = new List<SingletonCheckImpl> { list[0], list[0], list[0], list[0], list[0], list[0], list[0], list[0], list[0], list[0] };
        //    CollectionAssert.AreEquivalent(expected, list);

        //}

        [TestMethod()]
        public void MultipleImplementationsCheck()
        {
            config.Register<IService, ServiceImpl>();
            config.Register<IService, SecondServiceImpl>();

            Container container = new Container(config);
            IEnumerable<IService> implementations = container.Resolve<IEnumerable<IService>>();

            int implementationsCount = 0;

            foreach(var implementation in implementations)
            {
                implementationsCount++;
            }
            Assert.AreEqual(2, implementationsCount);
        }

        [TestMethod()]
        public void MultipleImplementationInCostructorCheck()
        {
            config.Register<IRepository, RepositoryImpl>();
            config.Register<IService, ThirdService>();
            Container container = new Container(config);

            Assert.ThrowsException<TargetInvocationException>(() => container.Resolve<IService>());
        }

        [TestMethod]
        public void NonExistingDependency()
        {
            config.Register<IService, ServiceImpl>();
            Container container = new Container(config);

            ServiceImpl serviceImpl = container.Resolve<IService>();

            Assert.IsNotNull(serviceImpl);
        }

        [TestMethod()]
        public void GenericsCheck()
        {
            config.Register<IRepository, MySqlRepository>();
            config.Register<IService<IRepository>, ServiceImpl<IRepository>>();
            Container container = new Container(config);

            Assert.ThrowsException<TargetInvocationException>(() => container.Resolve<IService<IRepository>>());
        }

        [TestMethod()]
        public void SelfRegistration()
        {
            config.Register<List<int>, List<int>>();
            Container container = new Container(config);

            List<int> implementation = container.Resolve<List<int>>();

            Assert.IsInstanceOfType(implementation, typeof(List<int>));
        }

        [TestMethod()]
        public void OpenGenericsCheck()
        {
            config.Register<IRepository, MySqlRepository>();
            config.Register(typeof(IService<>), typeof(ServiceImpl<>));
            Container container = new Container(config);

            Assert.ThrowsException<TargetInvocationException>(() => container.Resolve<IService<IRepository>>());
        }

    }

}