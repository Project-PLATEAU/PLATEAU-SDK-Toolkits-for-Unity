using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlateauToolkit.Sandbox.Editor
{
    [Icon("UnityEditor.InspectorWindow")]
    [Overlay(typeof(SceneView), "plateau-sandbox-placement-tool", "PLATEAU 配置ツール", "PlateauPlacementInspector")]
    sealed class PlateauSandboxPlacementToolOverlay : Overlay, ITransientOverlay
    {
        public bool visible => ToolManager.activeToolType == typeof(PlateauSandboxPlacementTool);

        IDisposable m_OnBrushShapeRandomSeedChangedSubscription;

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();

            m_OnBrushShapeRandomSeedChangedSubscription?.Dispose();
            m_OnBrushShapeRandomSeedChangedSubscription = null;
        }

        public override VisualElement CreatePanelContent()
        {
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PlateauSandboxPaths.PlacementToolOverlayUxml);
            TemplateContainer root = uxml.CloneTree();

            VisualElement brushPlacementGroup = root.Q<VisualElement>("brush-placement-group");
            DropdownField locationDropDown = root.Q<DropdownField>("placement-mode");
            DropdownField upVectorDropDown = root.Q<DropdownField>("placement-up-vector");
            Slider rotationSlider = root.Q<Slider>("brush-placement-rotation");
            Slider radiusSlider = root.Q<Slider>("brush-placement-radius");
            SliderInt countSlider = root.Q<SliderInt>("brush-placement-count");
            Slider spacingSlider = root.Q<Slider>("brush-placement-spacing");
            Toggle isSeedFixed = root.Q<Toggle>("brush-placement-fixed-seed");
            TextField seedField = root.Q<TextField>("brush-placement-seed");
            Button randomizeSeedButton = root.Q<Button>("brush-placement-randomize-seed");
            DropdownField modeDropDown = root.Q<DropdownField>("placement-tool");

            void UpdateMode()
            {
                brushPlacementGroup.style.display =
                    PlateauSandboxContext.GetCurrent().PlacementSettings.Mode == PlacementMode.Brush ?
                    DisplayStyle.Flex : DisplayStyle.None;
            }

            locationDropDown.RegisterValueChangedCallback(_ =>
            {
                var context = PlateauSandboxContext.GetCurrent();
                context.PlacementSettings.Location = (PlacementLocation)locationDropDown.index;
            });
            upVectorDropDown.RegisterValueChangedCallback(_ =>
            {
                var context = PlateauSandboxContext.GetCurrent();
                context.PlacementSettings.UpVector = (PlacementUpVector)upVectorDropDown.index;
            });
            rotationSlider.RegisterValueChangedCallback(evt =>
            {
                PlateauSandboxContext.GetCurrent().PlacementSettings.Brush.ForwardYAxis = evt.newValue;
            });
            radiusSlider.RegisterValueChangedCallback(evt =>
            {
                PlateauSandboxContext.GetCurrent().PlacementSettings.Brush.Radius = evt.newValue;
            });
            countSlider.RegisterValueChangedCallback(evt =>
            {
                PlateauSandboxContext.GetCurrent().PlacementSettings.Brush.InstantiationCount = evt.newValue;
            });
            spacingSlider.RegisterValueChangedCallback(evt =>
            {
                PlateauSandboxContext.GetCurrent().PlacementSettings.Brush.Spacing = evt.newValue;
            });
            isSeedFixed.RegisterValueChangedCallback(evt =>
            {
                PlateauSandboxContext.GetCurrent().PlacementSettings.Brush.IsShapeRandomSeedFixed = evt.newValue;
            });
            seedField.RegisterValueChangedCallback(evt =>
            {
                if (string.IsNullOrEmpty(evt.newValue))
                {
                    PlateauSandboxContext.GetCurrent().PlacementSettings.Brush.ShapeRandomSeed = 0;
                }
                else if (int.TryParse(evt.newValue, out int value))
                {
                    PlateauSandboxContext.GetCurrent().PlacementSettings.Brush.ShapeRandomSeed = value;
                }
                else
                {
                    seedField.value = evt.previousValue;
                }
            });
            randomizeSeedButton.RegisterCallback<ClickEvent>(_ =>
            {
                PlateauSandboxContext.GetCurrent().PlacementSettings.Brush.RandomizeShapeSeed();
            });
            modeDropDown.RegisterValueChangedCallback(_ =>
            {
                var context = PlateauSandboxContext.GetCurrent();
                switch (modeDropDown.index)
                {
                    case 0:
                        context.PlacementSettings.Mode = PlacementMode.Click;
                        break;
                    case 1:
                    {
                        context.PlacementSettings.Mode = PlacementMode.Brush;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(modeDropDown.name);
                }

                UpdateMode();
            });

            var context = PlateauSandboxContext.GetCurrent();

            context.PlacementSettings.Brush.OnShapeRandomSeedChanged += OnShapeRandomSeedChanged;
            m_OnBrushShapeRandomSeedChangedSubscription = new CallbackDisposable(() =>
                context.PlacementSettings.Brush.OnShapeRandomSeedChanged -= OnShapeRandomSeedChanged);

            void OnShapeRandomSeedChanged(int seed)
            {
                seedField.value = context.PlacementSettings.Brush.ShapeRandomSeed.ToString();
            }

            // Sync the initial state
            locationDropDown.index = (int)context.PlacementSettings.Location;
            upVectorDropDown.index = (int)context.PlacementSettings.UpVector;
            modeDropDown.index = (int)context.PlacementSettings.Mode;
            upVectorDropDown.index = (int)context.PlacementSettings.UpVector;
            radiusSlider.value = context.PlacementSettings.Brush.Radius;
            countSlider.value = context.PlacementSettings.Brush.InstantiationCount;
            spacingSlider.value = context.PlacementSettings.Brush.Spacing;
            isSeedFixed.value = context.PlacementSettings.Brush.IsShapeRandomSeedFixed;
            seedField.value = context.PlacementSettings.Brush.ShapeRandomSeed.ToString();
            rotationSlider.value = context.PlacementSettings.Brush.ForwardYAxis;
            UpdateMode();

            return root;
        }
    }
}