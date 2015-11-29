#if (MicrosoftUnitTest)

// In VisualStudio, we need some definitions to build the sourcecode that is also
// attributed for the NUnit test runner.
namespace NUnit.Framework
{
    public class TestFixture : System.Attribute
    {
    }

    public class SetUp : System.Attribute
    {
        // Called before each Test method
    }

    public class Test : System.Attribute
    {
    }

    public class TearDown : System.Attribute
    {
        // Called after each Test method
    }
}


#else

// In Monodevelop, we need some definitions to build the sourcecode that is also attributed for
// the VisualStudio test runner.
// To install NUnit test runner for Monodevelop, just download package 'monodevelop-nunit'. 
namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    public class TestClass : System.Attribute
    {
    }
    
    public class TestInitialize : System.Attribute
    {
        // Called before each TestMethod
    }

    public class TestMethod : System.Attribute
    {
    }

    public class TestCleanup : System.Attribute
    {
        // Called after each TestMethod
    }
}

#endif
