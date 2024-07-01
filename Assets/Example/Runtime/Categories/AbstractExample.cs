using System.Collections.Generic;
using System.Linq;
using RosettaUI.UIToolkit;
using UnityEngine;
using UnityEngine.Serialization;

namespace RosettaUI.Example
{
    public class AbstractExample : MonoBehaviour, IElementCreator
    {
        [SerializeReference]
        public IMyInterface interfaceField = new ConcreteClassA {floatValue = 1};

        [SerializeReference] public List<IMyInterface> interfaceList = new()
        {
            new ConcreteClassA {floatValue = 1},
            new ConcreteClassB {stringValue = "two"},
            new ConcreteClassA {floatValue = 3},
            null
        };
        
        public Element CreateElement(LabelElement _)
        {
            return UI.Tabs(
                ExampleTemplate.UIFunctionTab(nameof(UI.AbstractField),
                    UI.AbstractField(() => interfaceField, true).Open(),
                    UI.List(() => interfaceList,
                        createItemElement: (binder, i) => UI.AbstractField(
                            $"Item {i}",
                            binder,
                            true
                        ),
                        createNewInstance: () => new ConcreteClassA()
                    )
                ),
                ExampleTemplate.UIFunctionTab(nameof(UI.AbstractFieldReadOnly),
                    UI.AbstractFieldReadOnly(() => interfaceField).Open(),
                    UI.ListReadOnly(() => interfaceList)
                ),
                
                ExampleTemplate.Tab("Codes",
                    ExampleTemplate.CodeElementSets("Type Changeable AbstractField",
                        (@"UI.AbstractField(
    () => _interfaceField,
    typeChangeable: true
)",
                            UI.AbstractField(
                                () => interfaceField,
                                typeChangeable: true
                            )
                        )
                    ),
                    ExampleTemplate.CodeElementSets("Type Changeable AbstractField List",
                        (@"UI.List(
    () => _interfaceList,
    createItemElement: (binder, i) => UI.AbstractField(
        $""Item {i}"",
        () => _interfaceList[i],
        typeChangeable: true
    )
)",
                            UI.List(
                                () => interfaceList,
                                createItemElement: (binder, i) => UI.AbstractField(
                                    $"Item {i}",
                                    binder,
                                    typeChangeable: true
                                )
                            )
                        )
                    )
                )
            );
        }
    }
}