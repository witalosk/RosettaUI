using System;
using UnityEngine;

namespace RosettaUI.Example
{
    public enum MyEnum
    {
        One,
        Two,
        Three
    }

    [Serializable]
    public class SimpleClass
    {
        public float floatValue;
        public string stringValue;
        private int _privateValue; // will be ignored
    }

    public interface IMyInterface { }
    
    [Serializable]
    public class ConcreteClassA : IMyInterface
    {
        public float floatValue;
    }
    
    [Serializable]
    public class ConcreteClassB : IMyInterface
    {
        public string stringValue;
    }
}