using System.Collections.Generic;

namespace PLAYERTWO.ARPGProject
{
    public class CharacterEquipments
    {
        public ItemInstance initialRightHand;
        public ItemInstance initialLeftHand;
        public ItemInstance initialHelm;
        public ItemInstance initialChest;
        public ItemInstance initialPants;
        public ItemInstance initialGloves;
        public ItemInstance initialBoots;
        public ItemInstance initialWings;
        public ItemInstance initialLeftRing;
        public ItemInstance initialRightRing;
        public ItemInstance initialNecklace;
        public ItemInstance initialMount;
        public ItemInstance initialPet;
        public ItemInstance initialCharm;
        public ItemInstance[] initialConsumables;

        public ItemInstance currentRightHand => m_items ? m_items.GetRightHand() : initialRightHand;
        public ItemInstance currentLeftHand => m_items ? m_items.GetLeftHand() : initialLeftHand;
        public ItemInstance currentHelm => m_items ? m_items.GetHelm() : initialHelm;
        public ItemInstance currentChest => m_items ? m_items.GetChest() : initialChest;
        public ItemInstance currentPants => m_items ? m_items.GetPants() : initialPants;
        public ItemInstance currentGloves => m_items ? m_items.GetGloves() : initialGloves;
        public ItemInstance currentBoots => m_items ? m_items.GetBoots() : initialBoots;
        public ItemInstance currentWings => m_items ? m_items.GetWings() : initialWings;
        public ItemInstance currentLeftRing => m_items ? m_items.GetLeftRing() : initialLeftRing;
        public ItemInstance currentRightRing => m_items ? m_items.GetRightRing() : initialRightRing;
        public ItemInstance currentNecklace => m_items ? m_items.GetNecklace() : initialNecklace;
        public ItemInstance currentMount => m_items ? m_items.GetMount() : initialMount;
        public ItemInstance currentPet => m_items ? m_items.GetPet() : initialPet;
        public ItemInstance currentCharm => m_items ? m_items.GetCharm() : initialCharm;
        public ItemInstance[] currentConsumables => m_items ? m_items.GetConsumables() : initialConsumables;

        protected EntityItemManager m_items;

        public CharacterEquipments(Character data)
        {
            InstantiateItem(data.rightHand, ref initialRightHand);
            InstantiateItem(data.leftHand, ref initialLeftHand);
            InstantiateItem(data.helm, ref initialHelm);
            InstantiateItem(data.chest, ref initialChest);
            InstantiateItem(data.pants, ref initialPants);
            InstantiateItem(data.gloves, ref initialGloves);
            InstantiateItem(data.boots, ref initialBoots);
            InstantiateItem(data.wings, ref initialWings);
            InstantiateItem(data.leftRing, ref initialLeftRing);
            InstantiateItem(data.rightRing, ref initialRightRing);
            InstantiateItem(data.necklace, ref initialNecklace);
            InstantiateItem(data.mount, ref initialMount);
            InstantiateItem(data.pet, ref initialPet);
            InstantiateItem(data.charm, ref initialCharm);
            InstantiateConsumables(data.initialConsumables, data.maxConsumableSlots);
        }

        public Dictionary<string, ItemInstance> GetEquippedItems()
        {
            return new Dictionary<string, ItemInstance>
            {
                { "RightHand", currentRightHand },
                { "LeftHand", currentLeftHand },
                { "Head", currentHelm },
                { "Chest", currentChest },
                { "Pants", currentPants },
                { "Gloves", currentGloves },
                { "Boots", currentBoots },
                { "Wings", currentWings },
                { "LeftRing", currentLeftRing },
                { "RightRing", currentRightRing },
                { "Necklace", currentNecklace },
                { "Mount", currentMount },
                { "Pet", currentPet },
                { "Charm", currentCharm }
            };
        }

        public CharacterEquipments(ItemInstance initialRightHand,
            ItemInstance initialLeftHand, ItemInstance initialHelm,
            ItemInstance initialChest, ItemInstance initialPants,
            ItemInstance initialGloves, ItemInstance initialBoots,
            ItemInstance initialWings, ItemInstance initialLeftRing,
            ItemInstance initialRightRing, ItemInstance initialNecklace,
            ItemInstance initialMount, ItemInstance initialPet,
            ItemInstance initialCharm, ItemInstance[] initialConsumables)
        {
            this.initialRightHand = initialRightHand;
            this.initialLeftHand = initialLeftHand;
            this.initialHelm = initialHelm;
            this.initialChest = initialChest;
            this.initialPants = initialPants;
            this.initialGloves = initialGloves;
            this.initialBoots = initialBoots;
            this.initialWings = initialWings;
            this.initialLeftRing = initialLeftRing;
            this.initialRightRing = initialRightRing;
            this.initialNecklace = initialNecklace;
            this.initialMount = initialMount;
            this.initialPet = initialPet;
            this.initialCharm = initialCharm;
            this.initialConsumables = initialConsumables;
        }

        /// <summary>
        /// Initializes a given Entity Item Manager.
        /// </summary>
        /// <param name="items">The Entity Item Manager you want to initialize.</param>
        public virtual void InitializeEquipments(EntityItemManager items)
        {
            m_items = items;
            m_items.ForceEquip(initialRightHand, ItemSlots.RightHand);
            m_items.ForceEquip(initialLeftHand, ItemSlots.LeftHand);
            m_items.ForceEquip(initialHelm, ItemSlots.Helm);
            m_items.ForceEquip(initialChest, ItemSlots.Chest);
            m_items.ForceEquip(initialPants, ItemSlots.Pants);
            m_items.ForceEquip(initialGloves, ItemSlots.Gloves);
            m_items.ForceEquip(initialBoots, ItemSlots.Boots);
            m_items.ForceEquip(initialWings, ItemSlots.Wings);
            m_items.ForceEquip(initialLeftRing, ItemSlots.LeftRing);
            m_items.ForceEquip(initialRightRing, ItemSlots.RightRing);
            m_items.ForceEquip(initialNecklace, ItemSlots.Necklace);
            m_items.ForceEquip(initialMount, ItemSlots.Mount);
            m_items.ForceEquip(initialPet, ItemSlots.Pet);
            m_items.ForceEquip(initialCharm, ItemSlots.Charm);
            m_items.SetConsumables(initialConsumables);
            m_items.RevalidateEquippedItems();
        }

        protected virtual void InstantiateItem(CharacterItem item, ref ItemInstance reference)
        {
            if (item.data != null) reference = item.ToItemInstance(true);
        }

        protected virtual void InstantiateConsumables(Item[] consumables, int maxCapacity)

        {
            initialConsumables = new ItemInstance[maxCapacity];

            for (int i = 0; i < maxCapacity; i++)
            {
                if (consumables.Length <= i || !consumables[i]) continue;
                
                initialConsumables[i] = new ItemInstance(consumables[i]);
            }
        }

        public static CharacterEquipments CreateFromSerializer(EquipmentsSerializer equipments)

        {
            var consumables = new ItemInstance[equipments.consumables.Length];

            for (int i = 0; i < consumables.Length; i++)
            {
                consumables[i] = ItemInstance.CreateFromSerializer(equipments.consumables[i]);
            }

            return new CharacterEquipments(
                ItemInstance.CreateFromSerializer(equipments.rightHand),
                ItemInstance.CreateFromSerializer(equipments.leftHand),
                ItemInstance.CreateFromSerializer(equipments.helm),
                ItemInstance.CreateFromSerializer(equipments.chest),
                ItemInstance.CreateFromSerializer(equipments.pants),
                ItemInstance.CreateFromSerializer(equipments.gloves),
                ItemInstance.CreateFromSerializer(equipments.boots),
                ItemInstance.CreateFromSerializer(equipments.wings),
                ItemInstance.CreateFromSerializer(equipments.leftRing),
                ItemInstance.CreateFromSerializer(equipments.rightRing),
                ItemInstance.CreateFromSerializer(equipments.necklace),
                ItemInstance.CreateFromSerializer(equipments.mount),
                ItemInstance.CreateFromSerializer(equipments.pet),
                ItemInstance.CreateFromSerializer(equipments.charm),
                consumables
            );
        }
    }
}
