#if UNITY_EDITOR
using CreateWithVR.Assignment;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace CreateWithVR.Assignment.Editor
{
    public static class AssignmentSetupEditor
    {
        const string ScenePath = "Assets/Scenes/VRRoom_Assignment.unity";
        const string ControllerXrOriginPath = "Assets/VRTemplateAssets/Prefabs/Setup/Complete XR Origin Set Up Variant.prefab";
        const string LeftHandModelPath = "Assets/Samples/XR Hands/1.7.3/HandVisualizer/Models/LeftHand.fbx";
        const string RightHandModelPath = "Assets/Samples/XR Hands/1.7.3/HandVisualizer/Models/RightHand.fbx";
        const string HandMaterialPath = "Assets/Samples/XR Interaction Toolkit/3.4.1/Hands Interaction Demo/Materials/Unity_Hand_Medium.mat";
        const string AmbientLoopPath = "Assets/VRTemplateAssets/Audio/Button_14_hover.wav";
        const string AmbientOneShotPath = "Assets/Samples/XR Interaction Toolkit/3.4.1/Hands Interaction Demo/DemoAssets/Audio/ButtonHover.wav";

        [MenuItem("VR Assignment/Setup Complete Assignment")]
        public static void SetupCompleteAssignment()
        {
            if (!EnsureSceneIsOpen())
                return;

            ReplaceWithControllerXrOrigin();
            AddClockToWallClockLocationInternal();
            SetupControllerHandModelsInternal();
            ActivateInactiveObjects();
            SetupInteractableTooltips();
            SetupAmbientAudio();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("VR Assignment setup complete. Press Play to test the clock, hands, tooltips, and audio.");
        }

        // Entry point for Unity batch mode: -executeMethod CreateWithVR.Assignment.Editor.AssignmentSetupEditor.RunBatchSetup
        public static void RunBatchSetup()
        {
            EditorSceneManager.OpenScene(ScenePath);
            SetupCompleteAssignment();
            EditorApplication.Exit(0);
        }

        [MenuItem("VR Assignment/Create Analog Wall Clock")]
        static void CreateAnalogWallClockMenu()
        {
            var parent = Selection.activeTransform;
            CreateAnalogWallClock(parent);
            if (parent != null)
                EditorSceneManager.MarkSceneDirty(parent.gameObject.scene);
        }

        [MenuItem("VR Assignment/Setup Controller Hand Models")]
        static void SetupControllerHandModelsMenu()
        {
            SetupControllerHandModelsInternal();
            EditorSceneManager.SaveOpenScenes();
        }

        [MenuItem("VR Assignment/Add Clock To WallClocklocation")]
        static void AddClockToWallClockLocationMenu()
        {
            AddClockToWallClockLocationInternal();
            EditorSceneManager.SaveOpenScenes();
        }

        static bool EnsureSceneIsOpen()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path == ScenePath)
                return true;

            if (Application.isBatchMode)
            {
                EditorSceneManager.OpenScene(ScenePath);
                return true;
            }

            if (EditorUtility.DisplayDialog(
                    "VR Assignment",
                    "Open VRRoom_Assignment scene and run full setup?",
                    "Open & Setup",
                    "Cancel"))
            {
                EditorSceneManager.OpenScene(ScenePath);
                return true;
            }

            return false;
        }

        static void ReplaceWithControllerXrOrigin()
        {
            var xrOrigin = Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogWarning("No XR Origin found — adding controller XR Origin prefab at default spawn.");
                SpawnControllerXrOrigin(Vector3.zero, Quaternion.identity);
                return;
            }

            var root = xrOrigin.transform.root.gameObject;
            var position = root.transform.position;
            var rotation = root.transform.rotation;

            if (root.name.Contains("Complete XR Origin Set Up Variant") &&
                !root.name.Contains("Hands"))
            {
                Debug.Log("Controller-based XR Origin already in scene.");
                return;
            }

            Object.DestroyImmediate(root);
            SpawnControllerXrOrigin(position, rotation);
        }

        static void SpawnControllerXrOrigin(Vector3 position, Quaternion rotation)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ControllerXrOriginPath);
            if (prefab == null)
            {
                Debug.LogError($"Missing prefab: {ControllerXrOriginPath}");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            Undo.RegisterCreatedObjectUndo(instance, "Add Controller XR Origin");
        }

        static void AddClockToWallClockLocationInternal()
        {
            var location = GameObject.Find("WallClocklocation");
            if (location == null)
            {
                Debug.LogError("WallClocklocation not found in scene.");
                return;
            }

            var existing = location.GetComponentInChildren<AnalogWallClock>(true);
            if (existing != null)
            {
                var clockObject = existing.gameObject;
                if (clockObject.name != "AnalogWallClock" && existing.transform.parent != null)
                    clockObject = existing.transform.parent.gameObject;
                Object.DestroyImmediate(clockObject);
            }

            var clockRoot = CreateAnalogWallClock(location.transform);
            clockRoot.transform.localPosition = Vector3.zero;
            clockRoot.transform.localRotation = Quaternion.identity;
            location.SetActive(true);
        }

        static GameObject CreateAnalogWallClock(Transform parent)
        {
            var clockRoot = new GameObject("AnalogWallClock");
            Undo.RegisterCreatedObjectUndo(clockRoot, "Create Analog Wall Clock");
            clockRoot.transform.SetParent(parent, false);

            var face = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            face.name = "ClockFace";
            face.transform.SetParent(clockRoot.transform, false);
            face.transform.localScale = new Vector3(0.45f, 0.02f, 0.45f);
            face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Object.DestroyImmediate(face.GetComponent<Collider>());

            var frame = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frame.name = "ClockFrame";
            frame.transform.SetParent(clockRoot.transform, false);
            frame.transform.localScale = new Vector3(0.5f, 0.015f, 0.5f);
            frame.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Object.DestroyImmediate(frame.GetComponent<Collider>());

            var hourHand = CreateClockHand(clockRoot.transform, "HourHand", new Vector3(0.04f, 0.01f, 0.01f), new Vector3(0f, 0.08f, 0.04f));
            var minuteHand = CreateClockHand(clockRoot.transform, "MinuteHand", new Vector3(0.03f, 0.01f, 0.008f), new Vector3(0f, 0.11f, 0.03f));
            var secondHand = CreateClockHand(clockRoot.transform, "SecondHand", new Vector3(0.015f, 0.01f, 0.005f), new Vector3(0f, 0.13f, 0.015f), new Color(0.85f, 0.2f, 0.2f));

            var clock = clockRoot.AddComponent<AnalogWallClock>();
            var serializedClock = new SerializedObject(clock);
            serializedClock.FindProperty("m_HourHand").objectReferenceValue = hourHand.transform;
            serializedClock.FindProperty("m_MinuteHand").objectReferenceValue = minuteHand.transform;
            serializedClock.FindProperty("m_SecondHand").objectReferenceValue = secondHand.transform;
            serializedClock.FindProperty("m_RotationAxis").vector3Value = Vector3.forward;
            serializedClock.FindProperty("m_ZeroOffsetDegrees").floatValue = 90f;
            serializedClock.ApplyModifiedPropertiesWithoutUndo();

            return clockRoot;
        }

        static void SetupControllerHandModelsInternal()
        {
            var xrOrigin = Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("No XR Origin found for hand setup.");
                return;
            }

            DisableHandTrackingVisuals(xrOrigin.transform);

            var leftController = FindChildRecursive(xrOrigin.transform, "Left Controller");
            var rightController = FindChildRecursive(xrOrigin.transform, "Right Controller");

            if (leftController == null || rightController == null)
            {
                Debug.LogError("Could not find Left/Right Controller under XR Origin.");
                return;
            }

            DisableDefaultControllerMeshes(leftController);
            DisableDefaultControllerMeshes(rightController);

            AttachHandModel(leftController, LeftHandModelPath, ControllerHandAnimator.HandSide.Left, new Vector3(0f, 0f, -0.05f), new Vector3(-90f, 90f, 0f));
            AttachHandModel(rightController, RightHandModelPath, ControllerHandAnimator.HandSide.Right, new Vector3(0f, 0f, -0.05f), new Vector3(-90f, -90f, 0f));
        }

        static void ActivateInactiveObjects()
        {
            SetActiveIfFound("WallClocklocation", true);
            SetActiveIfFound("KeySocket", true);
            SetActiveIfFound("MugSocket", true);
            SetActiveIfFound("Post Process Volume", true);
        }

        static void SetupInteractableTooltips()
        {
            AddTooltipToInteractable("Key", "Grab the key and place it in the key socket.");
            AddTooltipToInteractable("Book", "Grab the book and return it to the shelf socket.");
            AddTooltipToInteractable("Mug", "Grab the mug and place it on the mug socket.");
        }

        static void SetupAmbientAudio()
        {
            var environment = GameObject.Find("Environment");
            if (environment == null)
                environment = new GameObject("Environment");

            var existing = environment.transform.Find("AmbientAudio");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            var audioGo = new GameObject("AmbientAudio");
            Undo.RegisterCreatedObjectUndo(audioGo, "Create Ambient Audio");
            audioGo.transform.SetParent(environment.transform, false);
            audioGo.transform.localPosition = new Vector3(0f, 2f, 0f);

            var source = audioGo.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.loop = true;
            source.volume = 0.25f;
            source.minDistance = 1f;
            source.maxDistance = 15f;

            var zone = audioGo.AddComponent<AmbientAudioZone>();
            var loopClip = AssetDatabase.LoadAssetAtPath<AudioClip>(AmbientLoopPath);
            var oneShotClip = AssetDatabase.LoadAssetAtPath<AudioClip>(AmbientOneShotPath);

            var serializedZone = new SerializedObject(zone);
            serializedZone.FindProperty("m_AmbientLoop").objectReferenceValue = loopClip;
            serializedZone.FindProperty("m_RandomOneShots").arraySize = oneShotClip != null ? 1 : 0;
            if (oneShotClip != null)
                serializedZone.FindProperty("m_RandomOneShots").GetArrayElementAtIndex(0).objectReferenceValue = oneShotClip;
            serializedZone.FindProperty("m_Volume").floatValue = 0.25f;
            serializedZone.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddTooltipToInteractable(string objectName, string message)
        {
            var target = GameObject.Find(objectName);
            if (target == null || target.GetComponent<XRGrabInteractable>() == null)
                return;

            if (target.GetComponent<InteractableTooltip>() != null)
                return;

            var tooltipRoot = new GameObject("Tooltip");
            tooltipRoot.transform.SetParent(target.transform, false);
            tooltipRoot.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            tooltipRoot.transform.localRotation = Quaternion.identity;

            var canvas = tooltipRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var rectTransform = tooltipRoot.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(280f, 60f);
            rectTransform.localScale = Vector3.one * 0.0015f;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(tooltipRoot.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = message;
            text.fontSize = 22f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            tooltipRoot.SetActive(false);

            var tooltip = target.AddComponent<InteractableTooltip>();
            var serializedTooltip = new SerializedObject(tooltip);
            serializedTooltip.FindProperty("m_TooltipText").objectReferenceValue = text;
            serializedTooltip.FindProperty("m_TooltipRoot").objectReferenceValue = tooltipRoot;
            serializedTooltip.FindProperty("m_Message").stringValue = message;
            serializedTooltip.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetActiveIfFound(string objectName, bool active)
        {
            var obj = GameObject.Find(objectName);
            if (obj != null)
                obj.SetActive(active);
        }

        static GameObject CreateClockHand(Transform parent, string name, Vector3 scale, Vector3 localPosition, Color? color = null)
        {
            var hand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hand.name = name;
            hand.transform.SetParent(parent, false);
            hand.transform.localScale = scale;
            hand.transform.localPosition = localPosition;
            Object.DestroyImmediate(hand.GetComponent<Collider>());

            if (color.HasValue)
            {
                var renderer = hand.GetComponent<Renderer>();
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = color.Value
                };
            }

            return hand;
        }

        static void DisableHandTrackingVisuals(Transform xrOriginRoot)
        {
            foreach (var renderer in xrOriginRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                renderer.enabled = false;

            foreach (var behaviour in xrOriginRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null)
                    continue;

                var typeName = behaviour.GetType().Name;
                if (typeName is "HandVisualizer" or "XRHandMeshController" or "XRHandSkeletonDriver")
                    behaviour.enabled = false;
            }
        }

        static void DisableDefaultControllerMeshes(Transform controller)
        {
            foreach (var renderer in controller.GetComponentsInChildren<Renderer>(true))
            {
                var name = renderer.gameObject.name;
                if (name.Contains("Controller") || name.Contains("Universal"))
                    renderer.enabled = false;
            }
        }

        static void AttachHandModel(Transform controller, string modelPath, ControllerHandAnimator.HandSide side, Vector3 localPosition, Vector3 localEulerAngles)
        {
            var handObjectName = side == ControllerHandAnimator.HandSide.Left ? "LeftHandModel" : "RightHandModel";
            var existing = controller.Find(handObjectName);
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelPrefab == null)
            {
                Debug.LogError($"Could not load hand model at {modelPath}");
                return;
            }

            var handInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, controller);
            handInstance.name = handObjectName;
            handInstance.transform.localPosition = localPosition;
            handInstance.transform.localRotation = Quaternion.Euler(localEulerAngles);
            handInstance.transform.localScale = Vector3.one * 1.1f;

            var handMaterial = AssetDatabase.LoadAssetAtPath<Material>(HandMaterialPath);
            if (handMaterial != null)
            {
                foreach (var renderer in handInstance.GetComponentsInChildren<Renderer>(true))
                {
                    var materials = renderer.sharedMaterials;
                    for (var i = 0; i < materials.Length; i++)
                        materials[i] = handMaterial;
                    renderer.sharedMaterials = materials;
                }
            }

            var animator = handInstance.GetComponent<ControllerHandAnimator>();
            if (animator == null)
                animator = handInstance.AddComponent<ControllerHandAnimator>();

            var serializedAnimator = new SerializedObject(animator);
            serializedAnimator.FindProperty("m_HandSide").enumValueIndex = (int)side;
            serializedAnimator.FindProperty("m_HandRoot").objectReferenceValue = handInstance.transform;
            serializedAnimator.FindProperty("m_CurlAxis").vector3Value = Vector3.right;
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
        }

        static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;

            for (var i = 0; i < parent.childCount; i++)
            {
                var result = FindChildRecursive(parent.GetChild(i), name);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
#endif
