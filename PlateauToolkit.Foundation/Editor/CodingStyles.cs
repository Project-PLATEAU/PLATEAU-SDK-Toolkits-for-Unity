#region Code Inspection Surpression
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable Unity.RedundantSerializeFieldAttribute
// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedParameter.Local
// ReSharper disable EmptyForStatement
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0219 // Variable is assigned but its value is never used
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0060 // Remove unused parameter
#endregion
using UnityEngine;

namespace PlateauToolkit.Editor.CodingStyles
{
    // Correct
    class InternalClass
    {
    }

    // Wrong
    internal class InternalClassWrong
    {
    }

    class AccessibilityModifierStyles : MonoBehaviour
    {
        // Correct
        int m_PrivateField;
        static int s_PrivateStaticField;

        int Property { get; set; }
        static int StaticProperty { get; set; }

        void PrivateMethod()
        {
        }

        static void PrivateStaticMethod()
        {
        }

        class PrivateClass
        {
        }
        interface IPrivateInterface
        {
        }

        // Wrong
        private int m_PrivateField2;
        private static int s_PrivateStaticField2;

        private int Property2 { get; set; }
        private static int StaticProperty2 { get; set; }

        private void PrivateMethod2()
        {
        }

        private static void PrivateStaticMethod2()
        {
        }

        private class PrivateClass2
        {
        }

        private interface IPrivateInterface2
        {
        }
    }

    class NamingConventions
    {
        // Correct
        int m_MyField;

        // Wrong
        int _myField;
        int myField;
        int MyField;

        // Correct
        static int s_MyStaticField;

        // Wrong
        static int m_MyStaticField;
        static int MyStaticField;
        static int staticField;

        // Correct
        static readonly int k_MyStaticReadonlyField;

        // Wrong
        static readonly int s_MyStaticFieldWrong;
        static readonly int MyStaticFieldWrong2;
        static readonly int k_MyStaticField_Wrong;

        // Correct
        const int k_MyConstant = 1;

        // Wrong
        const int MY_CONSTANT = 2;
        const int MyConstant = 3;

        // Correct
        [SerializeField] int m_MySerializeField;

        // Wrong
        [SerializeField] int mySerializeField;
        [SerializeField] int MySerializeField;

        // Correct
        int MyProperty { get; set; }

        // Wrong
        int myProperty { get; set; }
        int m_MyProperty { get; set; }

        // Correct
        void MyMethod() { }

        // Wrong
        void myMethod() { }
        void _MyMethod() { }

        // Correct
        interface IMyInterface { }

        // Wrong
        interface MyInterface { }

        // Correct
        enum MyEnum { }

        // Wrong
        enum EMyEnum { }

        // Correct
        class MyGenericClass<TParam> { }
        class MyGenericClass2<T> { }

        // Wrong
        class MyGenericClass3<Param> { }

        void LocalVariables()
        {
            // Correct
            int myVar = 10;

            // Wrong
            int m_MyVar = 20;
            int MyVar = 30;
        }

        void Arguments(
            // Correct
            int myArgument,
            // Wrong
            int m_MyArgument,
            int MyArgument)
        {
        }
    }

    class BracesStyles
    {
        void Method()
        {
            // Correct
            if (true)
            {
            }

            // Wrong
            if (true) {
            }

            // Correct
            for (int i = 0; i < 20; i++)
            {
            }

            // Wrong
            for (int i = 0; i < 10; i++) {
            }
        }
    }
}