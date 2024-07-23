﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace RosettaUI.Test
{
    [Serializable]
    public class ClassForTest
    {
        public int intValue;
        public uint uintValue;
        public bool boolValue;
        public float floatValue;
        public string stringValue;
        public Color colorValue;
        public LayerMask layerMask;
        public EnumForTest enumValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Vector4 vector4Value;
        public Rect rectValue;
        public char charValue;
        public Bounds boundsValue;
        public Gradient gradient = new();
        public Quaternion quaternion;
        public Vector2Int vector2IntValue;
        public Vector3Int vector3IntValue;
        public RectInt rectIntValue;
        public BoundsInt boundsIntValue;
        // public Hash128 hash128Value;
        public RenderingLayerMask renderingLayerMask;

        public RectOffset rectOffset = new();
        
        public int[] intArray = { 1, 2, 3 };
        public List<float> floatList = new(){ 1, 2, 3 };
        public RectOffset[] classArray = { new() };
        public List<RectOffset> classList = new(){ new RectOffset() };
    }
    
    
    /// <summary>
    /// EditorのClipboardは任意クラスのパースでSerializedPropertyのみ対応している
    /// SerializedPropertyを用意するためにScriptableObjectを使用している
    /// </summary>
    [CreateAssetMenu(fileName = "ClassForTest", menuName = "Scriptable Objects/ClassForTest")]
    public class ClassForTestObject : ScriptableObject
    {
        public ClassForTest classValue;
    }
}