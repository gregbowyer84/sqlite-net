using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

using System.Diagnostics;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SetUp = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#else
using NUnit.Framework;
#endif


namespace SQLite.Tests
{
    [TestFixture]
    public class ModelBuilderTests {
        public class VO
        {
            public int ID { get; set; }
            public bool Flag { get; set; }
            public String Text { get; set; }

            public override string ToString()
            {
                return string.Format("VO:: ID:{0} Flag:{1} Text:{2}", ID, Flag, Text);
            }
        }
        public class DbAcs : SQLiteConnection
        {
            public DbAcs(String path)
                : base(path)
            {
            }

            public void buildTable()
            {
                CreateTable<VO>();
            }

            public int CountWithFlag(Boolean flag)
            {
                var cmd = CreateCommand("SELECT COUNT(*) FROM VO Where Flag = ?", flag);
                return cmd.ExecuteScalar<int>();
            }
        }

	    [TestFixtureSetUp]
	    public void Setup ()
	    {
		    ModelBuilder.Current.Entity<VO> ()
			    .AddAttribute (x => x.ID, new PrimaryKeyAttribute ())
			    .AddAttribute (x => x.ID, new AutoIncrementAttribute());
	    }

        [Test]
        public void TestPrimaryKey()
        {
	        Assert.IsTrue (typeof(VO).GetProperties ().Any (x => x.GetMetaDataAttributes<PrimaryKeyAttribute> ().Any ()));
        }
    }
}
