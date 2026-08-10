using System;

namespace Celeste.Mod.XaphanHelper.Data
{
    public class JacketPriorityData
    {
        public string Name;
        public Func<Level, bool> Test;
        public string Id;

        public JacketPriorityData(string name, Func<Level, bool> test, string id)
        {
            Name = name;
            Test = test;
            Id = id;
        }
    }
}
