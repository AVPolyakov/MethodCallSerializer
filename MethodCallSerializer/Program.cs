using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MethodCallSerializer
{
    class Program
    {
        static void Main()
        {
            new DataSource1().AsDataSource(default(IDataSource1)).M1("2");
            if (DataSource.Create<IDataSource1>().M1("Test2")[0] != "2")
                throw new Exception();
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            await new DataSource1().AsDataSource(default(IDataSource1)).M2("1");
            if ((await DataSource.Create<IDataSource1>().M2("Test2"))[0] != "1")
                throw new Exception();
        }
    }

    public class DataSource1 : IDataSource1
    {
        public List<string> M1(string x) => new List<string> {x};
        public Task<List<string>> M2(string x) => Task.FromResult(new List<string> {x});
    }

    public interface IDataSource1
    {
        List<string> M1(string x);
        Task<List<string>> M2(string x);
    }
}
