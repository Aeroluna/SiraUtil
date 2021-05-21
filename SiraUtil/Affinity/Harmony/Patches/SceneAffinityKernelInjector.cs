﻿using HarmonyLib;
using SiraUtil.Zenject;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Zenject;

namespace SiraUtil.Affinity.Harmony.Patches
{
    [HarmonyPatch(typeof(SceneContext), "InstallBindings")]
    internal class SceneAffinityKernelInjector
    {
        private static readonly MethodInfo _nonLazyMethod = typeof(NonLazyBinder).GetMethod(nameof(NonLazyBinder.NonLazy));
        private static readonly MethodInfo _kernelInjectorMethod = SymbolExtensions.GetMethodInfo(() => KernelInjector(null!));
        private static readonly FieldInfo _containerField = typeof(SceneContext).GetField("_container", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Inject(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(_nonLazyMethod))
                {
                    // We are currently at the NonLazy method.
                    int index = i;
                    // Next up is a pop code. Let's jump to it
                    index++;
                    // Now we want to jump to the next instruction (ldarg.0)
                    index++;
                    // Because we want to insert new instructions between that.
                    codes.InsertRange(index, new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0), // Load the instance of the SceneContext onto the stack
                        new CodeInstruction(OpCodes.Ldfld, _containerField), // Push the container from the SceneContext onto the stack
                        new CodeInstruction(OpCodes.Callvirt, _kernelInjectorMethod), // Insert our new instructions
                        new CodeInstruction(OpCodes.Pop) // Clears the stack
                    });
                    break;
                }
            }

            return codes;
        }

        private static IfNotBoundBinder KernelInjector(DiContainer container)
        {
            if (!container.HasBinding<AffinityManager>())
            {
                ProjectContext.Instance.Container.Bind<AffinityManager>().ToSelf().AsSingle().CopyIntoAllSubContainers();
                container.Bind<AffinityManager>().ToSelf().AsSingle();
            }
            return container.BindInterfacesAndSelfTo<AffinityKernel>().AsSingle();
        }
    }
}