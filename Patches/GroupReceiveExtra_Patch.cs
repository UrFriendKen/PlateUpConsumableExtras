using HarmonyLib;
using Kitchen;
using System;
using System.Reflection;
using Unity.Entities;
using ConsumableExtras.Components;
using System.Collections.Generic;

namespace ConsumableExtras.Patches
{
    [HarmonyPatch]
    internal class GroupReceiveExtra_Patch
    {
        public static MethodBase TargetMethod()
        {
            Type type = AccessTools.FirstInner(typeof(GroupReceiveExtra), t => t.Name.Contains("c__DisplayClass_OnUpdate_LambdaJob0"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("OriginalLambdaBody"));
        }

        public static void Prefix(ref Queue<Entity> __state, ref DynamicBuffer<CWaitingForItem> orders, in CAssignedTable table_set,
            ref BufferFromEntity<CTableSetGrabPoints> ____BufferFromEntity_CTableSetGrabPoints_0, ref ComponentDataFromEntity<CItemHolder> ____ComponentDataFromEntity_CItemHolder_1)
        {
            __state = new Queue<Entity>();
            DynamicBuffer<CTableSetGrabPoints> dynamicBuffer = ____BufferFromEntity_CTableSetGrabPoints_0[table_set];
            for (int i = 0; i < dynamicBuffer.Length; i++)
            {
                CTableSetGrabPoints cTableSetGrabPoints = dynamicBuffer[i];
                CItemHolder cItemHolder = ____ComponentDataFromEntity_CItemHolder_1[cTableSetGrabPoints];
                if (cItemHolder.HeldItem == default(Entity))
                {
                    continue;
                }
                int length = orders.Length;
                for (int j = 0; j < length; j++)
                {
                    if (orders[j].ExtraRequested && !orders[j].ExtraSatisfied && Main.instance.EntityManager.HasComponent<CItem>(cItemHolder.HeldItem))
                    {
                        if (!Main.instance.EntityManager.HasComponent<CConsumableExtra>(cItemHolder.HeldItem))
                        {
                            continue;
                        }

                        CItem comp = Main.instance.EntityManager.GetComponentData<CItem>(cItemHolder.HeldItem);

                        if (comp.ID == orders[j].Extra)
                        {
                            __state.Enqueue(cItemHolder.HeldItem);
                            break;
                        }
                    }
                }
            }
        }

        public static void Postfix(ref Queue<Entity> __state)
        {
            while (__state.Count > 0)
            {
                Entity entity = __state.Dequeue();
                CConsumableExtra consumable = Main.instance.EntityManager.GetComponentData<CConsumableExtra>(entity);

                if (consumable.DirtiesTo == 0)
                {
                    Main.instance.EntityManager.DestroyEntity(entity);
                    continue;
                }
                CItem item = Main.instance.EntityManager.GetComponentData<CItem>(entity);
                item.ID = consumable.DirtiesTo;
                Main.instance.EntityManager.SetComponentData(entity, item);
            }
        }
    }
}
