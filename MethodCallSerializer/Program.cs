using System;
using System.Collections.Generic;

namespace MethodCallSerializer
{
    class Program
    {
        static void Main()
        {
            new DataSource1().AsDataSource(default(IDataSource1)).M1("Test1");
            if (DataSource.Create<IDataSource1>().M1("Test2")[0] != "Test1")
                throw new Exception();
        }
    }

    public class DataSource1 : IDataSource1
    {
        public List<string> M1(string x) => new List<string> {x};
    }

    public interface IDataSource1
    {
        List<string> M1(string x);
    }
}
