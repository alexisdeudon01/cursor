using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Rendering;

/// <summary>
/// Custom Entities bootstrap that strips graphics-dependent systems when running
/// without compute shader support (headless/nographics). This prevents the noisy
/// “No SRP present, no compute shader support” log spam from Entities Graphics.
/// </summary>
public class HeadlessEntitiesBootstrap : ICustomBootstrap
{
    private const string RenderingNamespacePrefix = "Unity.Rendering";

    public bool Initialize(string defaultWorldName)
    {
        var world = new World(defaultWorldName);
        World.DefaultGameObjectInjectionWorld = world;

        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);

        bool isHeadless = Application.isBatchMode || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
        bool hasCompute = SystemInfo.supportsComputeShaders;
        bool hasSrp = GraphicsSettings.currentRenderPipeline != null;

        // Strip graphics systems when headless or when SRP/compute support is missing.
        if (isHeadless || !hasCompute || !hasSrp)
        {
            // Convert IReadOnlyList to List to use RemoveAll
            var systemsList = systems.ToList();
            systemsList.RemoveAll(t =>
            {
                var ns = t.Namespace ?? string.Empty;
                return ns.StartsWith(RenderingNamespacePrefix, StringComparison.Ordinal);
            });
            systems = systemsList;
            Debug.Log("[HeadlessEntitiesBootstrap] Entities Graphics systems stripped (headless/no SRP/no compute).");
        }

        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);

        // Update player loop - AppendWorldToCurrentPlayerLoop only takes the world parameter
        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);

        return true;
    }
}
