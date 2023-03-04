using KitchenData;
using Unity.Entities;

namespace ConsumableExtras.Components
{
    public struct CConsumableExtra : IItemProperty, IAttachableProperty, IComponentData
    {
        public int DirtiesTo;
    }
}
