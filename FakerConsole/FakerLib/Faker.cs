using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;
using System.Reflection;
using System.Linq;

namespace FakerLib
{
    public class Faker
    {
        private readonly int RecursionLevel = 3;
        private Dictionary<Type, IDtoGenerator> generators = new Dictionary<Type, IDtoGenerator>();
        private Dictionary<Type, int> createdTypes = new Dictionary<Type, int>();
        private FakerConfig _config;

        public Faker(FakerConfig config = null)
        {
            GeneratorsLoader gl = new GeneratorsLoader();
            generators = gl.GetGenerators();
            generators.Add(typeof(int), new IntGenerator());
            generators.Add(typeof(long), new LongGenerator());
            generators.Add(typeof(double), new DoubleGenerator());
            generators.Add(typeof(Uri), new UriGenerator());
            generators.Add(typeof(char), new CharGenerator());
            generators.Add(typeof(string), new StringGenerator());
            _config = config;
        }


        public T Create<T>()
        {
            object obj = default(T);
            Type objectType = typeof(T);
            if (IsDto<T>()) return (T)generators[objectType].Generate();
            if (IsGenericList<T>())
            {
                var t = typeof(T).GetGenericArguments().Single();
                MethodInfo method = typeof(ListGenerator).GetMethod(nameof(ListGenerator.GenerateGeneric));
                MethodInfo generic = method.MakeGenericMethod(t);
                ListGenerator listGenerator = new ListGenerator(this);
                return (T)generic.Invoke(listGenerator, null);
            }


            AddCreatedType(objectType);
            if (createdTypes[objectType] > RecursionLevel)
            {
                RemoveCreatedType(objectType);
                return default(T);
            }

            List<ConstructorInfo> constructors = GetSortedConstructors<T>();
            if (constructors.Count == 0)
            {
                obj = (T)Activator.CreateInstance(objectType);
                FillObject<T>(ref obj, GetFieldsToFill<T>().ToArray());
                RemoveCreatedType(objectType);
                return (T)obj;
            }
            foreach (ConstructorInfo constructor in constructors)
            {
                object[] parameters = FillConstructorParameters(constructor);
                try
                {
                    obj = (T)constructor.Invoke(parameters);
                    FillObject<T>(ref obj, GetFieldsToFill<T>().ToArray());
                    RemoveCreatedType(objectType);
                    return (T)obj;
                }
                catch
                {
                    continue;
                }
            }
            RemoveCreatedType(objectType);
            return (T)obj;
        }

        private List<ConstructorInfo> GetSortedConstructors<T>()
        {
            ConstructorInfo[] result = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var sortedResult = from constructor in result
                               orderby constructor.GetParameters().Length descending
                               select constructor;

            return sortedResult.ToList();
        }

        private object[] FillConstructorParameters(ConstructorInfo constructor)
        {
            IList<object> result = new List<object>();
            var parameters = constructor.GetParameters();
            foreach (ParameterInfo parameter in parameters)
            {
                result.Add(InvokeCreation(parameter.ParameterType));
            }
            return result.ToArray();
        }

        private void FillObject<T>(ref object obj, MemberInfo[] members)
        {
            foreach (MemberInfo member in members)
            {
                var key = (typeof(T), member);
                if (_config!=null && _config.configOptions.ContainsKey(key))
                {
                    var generator = (IDtoGenerator)Activator.CreateInstance(_config.configOptions[key]);
                    MemberTypes memberType = member.MemberType;
                    if (memberType == MemberTypes.Field) ((FieldInfo)member).SetValue(obj, generator.Generate());
                    if (memberType == MemberTypes.Property) ((PropertyInfo)member).SetValue(obj, generator.Generate());
                }
                else if (!IsFieldFilled(obj, member))
                {
                    MemberTypes memberType = member.MemberType;
                    if (memberType == MemberTypes.Field) ((FieldInfo)member).SetValue(obj, InvokeCreation(((FieldInfo)member).FieldType));
                    if (memberType == MemberTypes.Property) ((PropertyInfo)member).SetValue(obj, InvokeCreation(((PropertyInfo)member).PropertyType));
                }
            }
        }

        private List<MemberInfo> GetFieldsToFill<T>()
        {
            List<MemberInfo> members = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance).ToList<MemberInfo>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var publicSetterProps = from property in properties
                                    where property.SetMethod != null && property.SetMethod.IsPublic
                                    select property;
            members.AddRange(publicSetterProps.ToList<MemberInfo>());
            return members;
        }

        private bool IsDto<T>()
        {
            if (generators.ContainsKey(typeof(T))) return true;
            return false;
        }

        private bool IsGenericList<T>()
        {
            if (typeof(T).Name == typeof(List<>).Name) return true;
            return false;
        }

        private bool IsFieldFilled(object creatingObject, MemberInfo member)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                Type fieldType = ((FieldInfo)member).FieldType;
                return !Equals(((FieldInfo)member).GetValue(creatingObject), DefaultValue(fieldType));
            }
            if (member.MemberType == MemberTypes.Property)
            {
                Type propertyType = ((PropertyInfo)member).PropertyType;
                return !Equals(((PropertyInfo)member).GetValue(creatingObject), DefaultValue(propertyType));
            }
            return true;
        }

        private object DefaultValue(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }

        private object InvokeCreation(Type t)
        {
            MethodInfo method = typeof(Faker).GetMethod(nameof(Faker.Create));
            MethodInfo genericmethod = method.MakeGenericMethod(t);
            return genericmethod.Invoke(this, null);
        }

        private void AddCreatedType(Type objectType)
        {
            if (createdTypes.ContainsKey(objectType)) createdTypes[objectType]++;
            else createdTypes[objectType] = 1;
        }

        private void RemoveCreatedType(Type objectType)
        {
            createdTypes[objectType]--;
        }

    }
}
