using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MethodCallSerializer
{
    public static class DataSource
    {
        public static TResult AsDataSource<T, TResult>(this T it, TResult prototype) where T: TResult
        {
#if DEBUG
            return Implementation.Create<TResult>(new ImplementationImpl<T, TResult>(it));
#else
            return it;
#endif
        }

        private class ImplementationImpl<TArg, TResult> : IImplementation
        {
            private readonly TArg it;

            public ImplementationImpl(TArg it)
            {
                this.it = it;
            }

            public T Call<T>(string methodName, Args args)
            {
                var result = (T) typeof (TResult).GetMethod(methodName).Invoke(it, args.Value);
                var type = typeof (T);
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Task<>))
                    return (T) typeof (DataSource).GetMethod(nameof(SerializeTaskResult))
                        .MakeGenericMethod(type.GetGenericArguments())
                        .Invoke(null, new object[] {result, GetCachePath<TResult>(methodName)});
                else
                {
                    File.WriteAllText(GetCachePath<TResult>(methodName), JsonConvert.SerializeObject(result));
                    return result;
                }
            }

            public void CallVoid(string methodName, Args args) 
                => typeof (TResult).GetMethod(methodName).Invoke(it, args.Value);
        }

        public static async Task<T> SerializeTaskResult<T>(object o, string path)
        {
            var result = await (Task<T>) o;
            File.WriteAllText(path, JsonConvert.SerializeObject(result));
            return result;
        }

        private static string GetCachePath<TResult>(string methodName)
            => Path.Combine(cachePath, $"{typeof(TResult).Name}.{methodName}.json");

        private static readonly string cachePath = GetCachePath();

        private static string GetCachePath() 
            => Path.Combine(new FileInfo(GetPath()).Directory.Parent.FullName, 
                $@"{typeof (DataSource).Assembly.GetName().Name}\bin\Debug");

        private static string GetPath([CallerFilePath] string path = "") => path;

        public static TResult Create<TResult>() 
            => Implementation.Create<TResult>(new ImplementationImpl<TResult>());

        public class ImplementationImpl<TResult> : IImplementation
        {
            public T Call<T>(string methodName, Args args)
            {
                var type = typeof (T);
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Task<>))
                    return (T) typeof (DataSource).GetMethod(nameof(DeserializeTaskResult))
                        .MakeGenericMethod(type.GetGenericArguments())
                        .Invoke(null, new object[] {GetCachePath<TResult>(methodName)});
                else
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(GetCachePath<TResult>(methodName)));
            }

            public void CallVoid(string methodName, Args args)
            {
                throw new Exception();
            }
        }

        public static Task<T> DeserializeTaskResult<T>(string path) 
            => Task.FromResult(JsonConvert.DeserializeObject<T>(File.ReadAllText(path)));
    }
}