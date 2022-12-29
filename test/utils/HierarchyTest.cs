using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace cynofield.mods.utils
{
    public class HierarchyTest
    {
        class TestHierarchy : IHierarchy
        {
            private List<object> list = new List<object>();
            public TestHierarchy()
            {
                list.Add("1");
                list.Add(new TestHierarchy2());
                list.Add("3");
            }

            IHierarchy IHierarchy.Parent => throw new NotImplementedException();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return list.GetEnumerator();
            }

            override public string ToString() => "0";
        }

        class TestHierarchy2 : IHierarchy
        {
            private List<object> list = new List<object>();

            public TestHierarchy2()
            {
                list.Add("21");
                list.Add("22");
            }

            IHierarchy IHierarchy.Parent => throw new NotImplementedException();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return list.GetEnumerator();
            }

            override public string ToString() => "2";
        }

        [Fact]
        public void Test1()
        {
            string result = "";
            new TestHierarchy()
            .Traverse(
                (o) => o as IHierarchy,
                (o) => result += o.ToString() + " "
            );

            Assert.Equal("3 22 21 2 1 0 ", result);
        }
    }
}
